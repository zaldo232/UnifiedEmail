using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using UnifiedEmail.Messages;
using UnifiedEmail.Models;
using UnifiedEmail.Services;

namespace UnifiedEmail.ViewModels
{
    // 이메일 계정 등록/삭제 뷰모델
    public partial class AccountViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService = new(); // DB 서비스

        [ObservableProperty] private string email; // 입력 이메일
        [ObservableProperty] private string password; // 입력 비밀번호

        [ObservableProperty] private string imapServer; // IMAP 서버
        [ObservableProperty] private int imapPort; // IMAP 포트

        [ObservableProperty] private string smtpServer; // SMTP 서버
        [ObservableProperty] private int smtpPort; // SMTP 포트

        [ObservableProperty] private bool useImap = true; // IMAP 사용 여부
        [ObservableProperty] private bool useSsl = true; // SSL 사용 여부

        [ObservableProperty] private MailProviderModel selectedProvider; // 선택한 메일 제공업체
        [ObservableProperty] private EmailAccountModel selectedAccount; // 선택한 계정

        public ObservableCollection<MailProviderModel> Providers { get; } = new() // 메일 제공업체 목록
        {
            new MailProviderModel { Name = "Gmail", ImapServer = "imap.gmail.com", ImapPort = 993, SmtpServer = "smtp.gmail.com", SmtpPort = 587, UseSsl = true },
            new MailProviderModel { Name = "Gsuite (학교메일)", ImapServer = "imap.gmail.com", ImapPort = 993, SmtpServer = "smtp.gmail.com", SmtpPort = 587, UseSsl = true },
            new MailProviderModel { Name = "Naver", ImapServer = "imap.naver.com", ImapPort = 993, SmtpServer = "smtp.naver.com", SmtpPort = 587, UseSsl = true }
        };

        public ObservableCollection<EmailAccountModel> Accounts { get; } = new(); // 등록된 계정 목록

        public AccountViewModel()
        {
            SelectedProvider = Providers[0]; // 기본 선택
            LoadAccounts(); // 계정 목록 불러오기
        }

        // 제공업체 변경 시 서버/포트 자동 채움
        partial void OnSelectedProviderChanged(MailProviderModel value)
        {
            if (value == null || value.Name == "직접 입력") return;

            ImapServer = value.ImapServer;
            ImapPort = value.ImapPort;
            SmtpServer = value.SmtpServer;
            SmtpPort = value.SmtpPort;
            UseSsl = value.UseSsl;
        }

        // 계정 목록 조회 및 갱신
        private void LoadAccounts()
        {
            Accounts.Clear();
            var list = _dbService.GetAllAccounts();
            foreach (var acc in list)
            { 
                Accounts.Add(acc); 
            }
        }

        // 계정 등록
        [RelayCommand]
        private void RegisterAccount()
        {
            var encrypted = EncryptionService.Encrypt(Password); // 비밀번호 암호화

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

            _dbService.AddEmailAccount(account); // DB 저장
            LoadAccounts(); // 목록 새로고침

            // 이메일 목록 ViewModel에 새로고침 메시지 전송
            WeakReferenceMessenger.Default.Send(new RefreshEmailListMessage());

            // 입력 초기화
            Email = string.Empty;
            Password = string.Empty;
        }

        // 계정 삭제
        [RelayCommand]
        private void DeleteAccount()
        {
            if (SelectedAccount is null) return;

            _dbService.DeleteEmailAccount(SelectedAccount.Id); // DB 삭제
            LoadAccounts(); // 목록 새로고침

            // 이메일 목록 ViewModel에 새로고침 메시지 전송
            WeakReferenceMessenger.Default.Send(new RefreshEmailListMessage());
        }
    }
}