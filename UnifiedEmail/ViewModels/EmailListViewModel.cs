using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MimeKit;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using UnifiedEmail.Models;
using UnifiedEmail.Services;
using UnifiedEmail.Messages;

namespace UnifiedEmail.ViewModels
{
    public partial class EmailListViewModel : ObservableObject, IRecipient<RefreshEmailListMessage>
    {
        private readonly EmailService _emailService = new();
        private readonly System.Timers.Timer _refreshTimer;

        public ObservableCollection<EmailAccountModel> Accounts { get; } = new();
        public ObservableCollection<EmailMessageModel> Messages { get; } = new();
        public ObservableCollection<EmailMessageModel> FilteredMessages { get; } = new();

        private static readonly List<string> LogicalFolderMaster = new()
        {
            "받은메일함", "보낸메일함", "내게쓴메일"
        };

        public ObservableCollection<string> AvailableFolders { get; } = new();

        private List<(UniqueId Uid, MimeMessage Message, MessageFlags Flags)> _rawMimeMessages = new();

        [ObservableProperty] private EmailAccountModel selectedAccount;
        [ObservableProperty] private string searchText;
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string selectedLogicalFolder = "받은메일함";

        public int UnreadCount => Messages.Count(m => !m.IsRead);
        public int FilteredUnreadCount => FilteredMessages.Count(m => !m.IsRead);

        public EmailListViewModel()
        {
            LoadAccounts();

            _refreshTimer = new System.Timers.Timer(60000);
            _refreshTimer.Elapsed += RefreshTimer_Elapsed;
            _refreshTimer.AutoReset = true;
            _refreshTimer.Start();

            WeakReferenceMessenger.Default.Register<RefreshEmailListMessage>(this);
        }

        public void Receive(RefreshEmailListMessage message)
        {
            ReloadAccounts();
        }

        private void RefreshTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                await FetchEmailsAsync();
            });
        }

        private void LoadAccounts()
        {
            Accounts.Clear();
            var db = new DatabaseService();
            var list = db.GetAllAccounts();
            foreach (var acc in list)
                Accounts.Add(acc);
        }

        partial void OnSelectedAccountChanged(EmailAccountModel? account)
        {
            AvailableFolders.Clear();

            if (account == null)
                return;

            var isGoogle = account.Provider.Contains("Gmail") || account.Provider.Contains("Gsuite");

            foreach (var folder in LogicalFolderMaster)
            {
                if (folder == "내게쓴메일" && isGoogle)
                    continue;

                AvailableFolders.Add(folder);
            }

            SelectedLogicalFolder = AvailableFolders.FirstOrDefault() ?? "받은메일함";
        }

        [RelayCommand]
        public async Task FetchEmailsAsync()
        {
            if (SelectedAccount == null || IsLoading)
                return;

            try
            {
                IsLoading = true;

                Messages.Clear();
                _rawMimeMessages.Clear();

                var provider = SelectedAccount?.Provider ?? string.Empty;
                var logical = SelectedLogicalFolder ?? string.Empty;

                string folderName = "INBOX";

                if (ImapFolderMap.TryGetValue(provider, out var folderDict)
                    && folderDict.TryGetValue(logical, out var resolved))
                {
                    folderName = resolved;
                }

                var rawMessages = await _emailService.FetchFromFolderAsync(SelectedAccount, folderName);

                var sorted = rawMessages
                    .OrderByDescending(x => x.Item2.Date)
                    .ToList();

                _rawMimeMessages = sorted;

                foreach (var (uid, msg, flags) in sorted)
                {
                    Messages.Add(new EmailMessageModel
                    {
                        Subject = msg.Subject,
                        From = msg.From.ToString(),
                        Date = msg.Date.DateTime,
                        UniqueId = uid,
                        Flags = flags
                    });
                }

                System.Diagnostics.Debug.WriteLine($"[FETCH] Provider: {provider}, Logical: {logical}, Resolved IMAP Folder: {folderName}");
                ApplyFilter();

                OnPropertyChanged(nameof(UnreadCount));
                OnPropertyChanged(nameof(FilteredUnreadCount));
            }
            catch (Exception ex)
            {
                MessageBox.Show("메일 불러오기 실패: " + ex.Message, "오류");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task DeleteEmailAsync(EmailMessageModel? selected)
        {
            if (selected == null || SelectedAccount == null || IsLoading)
                return;

            try
            {
                IsLoading = true;
                await _emailService.DeleteEmailAsync(SelectedAccount, selected.UniqueId);
                Messages.Remove(selected);
                ApplyFilter();

                OnPropertyChanged(nameof(UnreadCount));
                OnPropertyChanged(nameof(FilteredUnreadCount));
            }
            catch (Exception ex)
            {
                MessageBox.Show("메일 삭제 실패: " + ex.Message, "오류");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task MarkAsReadAsync(EmailMessageModel email)
        {
            if (SelectedAccount == null || email.IsRead)
                return;

            try
            {
                var imapOption = SelectedAccount.ImapPort switch
                {
                    993 => SecureSocketOptions.SslOnConnect,
                    143 => SecureSocketOptions.StartTls,
                    _ => SecureSocketOptions.Auto
                };

                using var client = new ImapClient();
                var password = EncryptionService.Decrypt(SelectedAccount.PasswordEncrypted);
                await client.ConnectAsync(SelectedAccount.ImapServer, SelectedAccount.ImapPort, imapOption);
                await client.AuthenticateAsync(SelectedAccount.Email, password);

                var folderName = "INBOX";
                if (ImapFolderMap.TryGetValue(SelectedAccount.Provider, out var map) &&
                    map.TryGetValue(SelectedLogicalFolder, out var resolved))
                {
                    folderName = resolved;
                }

                var folder = await client.GetFolderAsync(folderName);
                await folder.OpenAsync(FolderAccess.ReadWrite);

                await folder.AddFlagsAsync(email.UniqueId, MessageFlags.Seen, true);

                // 이 줄만 있으면 ObservableObject가 UI에 자동 반영해줌
                email.Flags |= MessageFlags.Seen;

                OnPropertyChanged(nameof(UnreadCount));
                OnPropertyChanged(nameof(FilteredUnreadCount));
            }
            catch (Exception ex)
            {
                MessageBox.Show("읽음 처리 실패: " + ex.Message, "오류");
            }
        }

        public Task<MimeMessage?> GetRawMessageByIndex(int index)
        {
            if (index >= 0 && index < _rawMimeMessages.Count)
                return Task.FromResult<MimeMessage?>(_rawMimeMessages[index].Message);

            return Task.FromResult<MimeMessage?>(null);
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
            OnPropertyChanged(nameof(FilteredUnreadCount));
        }

        private void ApplyFilter()
        {
            FilteredMessages.Clear();
            var query = (SearchText ?? "").ToLower();

            foreach (var msg in Messages)
            {
                if (msg.Subject.ToLower().Contains(query) || msg.From.ToLower().Contains(query))
                    FilteredMessages.Add(msg);
            }
        }

        public void StopAutoRefresh()
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }

        public void ReloadAccounts()
        {
            Accounts.Clear();
            var db = new DatabaseService();
            var list = db.GetAllAccounts();
            foreach (var acc in list)
                Accounts.Add(acc);
        }

        private static readonly Dictionary<string, Dictionary<string, string>> ImapFolderMap = new()
        {
            ["Gmail"] = new()
            {
                ["받은메일함"] = "INBOX",
                ["보낸메일함"] = "[Gmail]/보낸편지함",
                ["내게쓴메일"] = "[Gmail]/Sent Mail"
            },
            ["Gsuite (학교메일)"] = new()
            {
                ["받은메일함"] = "INBOX",
                ["보낸메일함"] = "[Gmail]/보낸편지함",
                ["내게쓴메일"] = "[Gmail]/Sent Mail"
            },
            ["Naver"] = new()
            {
                ["받은메일함"] = "INBOX",
                ["보낸메일함"] = "SentBox",
                ["내게쓴메일"] = "내게쓴메일함"
            }
        };
    }
}