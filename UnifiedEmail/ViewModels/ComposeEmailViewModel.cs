using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using UnifiedEmail.Models;
using UnifiedEmail.Services;

namespace UnifiedEmail.ViewModels
{
    // 메일 작성/전송 뷰모델
    public partial class ComposeEmailViewModel : ObservableObject
    {
        private readonly EmailService _emailService = new(); // 메일 전송 서비스
        private readonly EmailAccountModel _account;         // 사용 계정

        public ComposeEmailViewModel(EmailAccountModel account)
        {
            _account = account;
        }

        [ObservableProperty] private string toEmail; // 수신자 이메일
        [ObservableProperty] private string subject; // 제목
        [ObservableProperty] private string body;    // 본문
        [ObservableProperty] private bool isSending; // 전송 중 여부

        public ObservableCollection<string> Attachments { get; } = new(); // 첨부파일 목록

        public Action? CloseAction { get; set; } // 창 닫기 콜백

        // 첨부파일 추가
        [RelayCommand]
        private void AddAttachment()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "모든 파일 (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var file in dialog.FileNames)
                    Attachments.Add(file);
            }
        }

        // 메일 전송
        [RelayCommand]
        private async Task SendAsync()
        {
            if (string.IsNullOrWhiteSpace(ToEmail))
            {
                MessageBox.Show("수신자 이메일을 입력하세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsSending = true;

                await _emailService.SendEmailAsync(_account, ToEmail, Subject, Body, Attachments);

                MessageBox.Show("메일 전송 완료", "성공", MessageBoxButton.OK, MessageBoxImage.Information);

                CloseAction?.Invoke(); // 창 닫기
            }
            catch (Exception ex)
            {
                MessageBox.Show("메일 전송 실패\n\n" + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSending = false;
            }
        }
    }
}