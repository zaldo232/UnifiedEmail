using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using UnifiedEmail.Models;
using UnifiedEmail.Services;
using UnifiedEmail.Messages;

namespace UnifiedEmail.ViewModels
{
    public partial class AccountViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService = new();

        [ObservableProperty] private string email;
        [ObservableProperty] private string password;

        [ObservableProperty] private string imapServer;
        [ObservableProperty] private int imapPort;

        [ObservableProperty] private string smtpServer;
        [ObservableProperty] private int smtpPort;

        [ObservableProperty] private bool useImap = true;
        [ObservableProperty] private bool useSsl = true;

        [ObservableProperty] private MailProviderModel selectedProvider;
        [ObservableProperty] private EmailAccountModel selectedAccount;

        public ObservableCollection<MailProviderModel> Providers { get; } = new()
        {
            new MailProviderModel { Name = "Gmail", ImapServer = "imap.gmail.com", ImapPort = 993, SmtpServer = "smtp.gmail.com", SmtpPort = 587, UseSsl = true },
            new MailProviderModel { Name = "Gsuite (학교메일)", ImapServer = "imap.gmail.com", ImapPort = 993, SmtpServer = "smtp.gmail.com", SmtpPort = 587, UseSsl = true },
            new MailProviderModel { Name = "Naver", ImapServer = "imap.naver.com", ImapPort = 993, SmtpServer = "smtp.naver.com", SmtpPort = 587, UseSsl = true }
        };

        public ObservableCollection<EmailAccountModel> Accounts { get; } = new();

        public AccountViewModel()
        {
            SelectedProvider = Providers[0]; // "직접 입력" 기본 선택
            LoadAccounts();
        }

        partial void OnSelectedProviderChanged(MailProviderModel value)
        {
            if (value == null || value.Name == "직접 입력") return;

            ImapServer = value.ImapServer;
            ImapPort = value.ImapPort;
            SmtpServer = value.SmtpServer;
            SmtpPort = value.SmtpPort;
            UseSsl = value.UseSsl;
        }

        private void LoadAccounts()
        {
            Accounts.Clear();
            var list = _dbService.GetAllAccounts();
            foreach (var acc in list)
                Accounts.Add(acc);
        }

        [RelayCommand]
        private void RegisterAccount()
        {
            var encrypted = EncryptionService.Encrypt(Password);

            var account = new EmailAccountModel
            {
                Email = Email,
                PasswordEncrypted = encrypted,
                ImapServer = ImapServer,
                ImapPort = ImapPort,
                SmtpServer = SmtpServer,
                SmtpPort = SmtpPort,
                UseImap = UseImap,
                UseSsl = UseSsl,
                Provider = SelectedProvider?.Name ?? "Custom"
            };

            _dbService.AddEmailAccount(account);
            LoadAccounts();

            // 이메일 목록 ViewModel에 새로고침 요청 메시지 전송
            WeakReferenceMessenger.Default.Send(new RefreshEmailListMessage());

            // 입력 초기화
            Email = string.Empty;
            Password = string.Empty;
        }

        [RelayCommand]
        private void DeleteAccount()
        {
            if (SelectedAccount is null) return;

            _dbService.DeleteEmailAccount(SelectedAccount.Id);
            LoadAccounts();

            // 이메일 목록 ViewModel에 새로고침 요청 메시지 전송
            WeakReferenceMessenger.Default.Send(new RefreshEmailListMessage());
        }

    }
}
