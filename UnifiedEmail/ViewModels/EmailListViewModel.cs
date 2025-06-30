using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows;
using UnifiedEmail.Messages;
using UnifiedEmail.Models;
using UnifiedEmail.Services;

namespace UnifiedEmail.ViewModels
{
    // 메일 목록/조회/삭제/읽음/검색/자동새로고침 뷰모델
    public partial class EmailListViewModel : ObservableObject, IRecipient<RefreshEmailListMessage>
    {
        private readonly EmailService _emailService = new();                // 메일 서비스
        private readonly System.Timers.Timer _refreshTimer;                 // 자동 새로고침 타이머

        public ObservableCollection<EmailAccountModel> Accounts { get; } = new();              // 계정 목록
        public ObservableCollection<EmailMessageModel> Messages { get; } = new();              // 전체 메일
        public ObservableCollection<EmailMessageModel> FilteredMessages { get; } = new();      // 검색 필터링 결과

        private static readonly List<string> LogicalFolderMaster = new()    // 논리 폴더명
        {
            "받은메일함", "보낸메일함", "내게쓴메일"
        };

        public ObservableCollection<string> AvailableFolders { get; } = new(); // UI에서 선택 가능한 폴더

        private List<(UniqueId Uid, MimeMessage Message, MessageFlags Flags)> _rawMimeMessages = new(); // 원본 메일 캐시

        [ObservableProperty] private EmailAccountModel selectedAccount;      // 선택 계정
        [ObservableProperty] private string searchText;                     // 검색어
        [ObservableProperty] private bool isLoading;                        // 로딩 중 여부
        [ObservableProperty] private string selectedLogicalFolder = "받은메일함"; // 선택 논리폴더

        public int UnreadCount => Messages.Count(m => !m.IsRead);           // 전체 안읽은 메일
        public int FilteredUnreadCount => FilteredMessages.Count(m => !m.IsRead); // 검색결과 안읽은 메일

        public EmailListViewModel()
        {
            LoadAccounts(); // 계정 목록 로드

            _refreshTimer = new System.Timers.Timer(60000); // 60초 주기
            _refreshTimer.Elapsed += RefreshTimer_Elapsed;
            _refreshTimer.AutoReset = true;
            _refreshTimer.Start();

            WeakReferenceMessenger.Default.Register<RefreshEmailListMessage>(this); // 메시지 구독
        }

        // 메시지 수신(계정 새로고침)
        public void Receive(RefreshEmailListMessage message)
        {
            ReloadAccounts();
        }

        // 타이머: 자동 새로고침
        private void RefreshTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                await FetchEmailsAsync();
            });
        }

        // 계정 목록 로드
        private void LoadAccounts()
        {
            Accounts.Clear();
            var db = new DatabaseService();
            var list = db.GetAllAccounts();
            foreach (var acc in list)
            { 
                Accounts.Add(acc); 
            }
        }

        // 계정 선택 변경 시 폴더 목록 갱신
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

        // 메일 목록 가져오기
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

                // 논리폴더 → 실제 IMAP 폴더명 변환
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

        // 메일 삭제
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

        // 읽음 처리(플래그 추가)
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

                // UI에 읽음 표시 적용
                email.Flags |= MessageFlags.Seen;

                OnPropertyChanged(nameof(UnreadCount));
                OnPropertyChanged(nameof(FilteredUnreadCount));
            }
            catch (Exception ex)
            {
                MessageBox.Show("읽음 처리 실패: " + ex.Message, "오류");
            }
        }

        // index로 원본 메시지 가져오기
        public Task<MimeMessage?> GetRawMessageByIndex(int index)
        {
            if (index >= 0 && index < _rawMimeMessages.Count)
                return Task.FromResult<MimeMessage?>(_rawMimeMessages[index].Message);

            return Task.FromResult<MimeMessage?>(null);
        }

        // 검색어 변경 시 필터 적용
        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
            OnPropertyChanged(nameof(FilteredUnreadCount));
        }

        // 메시지 필터링
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

        // 타이머 종료(앱종료 등)
        public void StopAutoRefresh()
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }

        // 계정 새로고침(메시지 수신 등)
        public void ReloadAccounts()
        {
            Accounts.Clear();
            var db = new DatabaseService();
            var list = db.GetAllAccounts();
            foreach (var acc in list)
                Accounts.Add(acc);
        }

        // 논리폴더 <-> IMAP 폴더 맵
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
