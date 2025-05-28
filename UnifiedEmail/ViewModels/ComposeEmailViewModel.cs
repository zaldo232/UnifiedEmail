using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using UnifiedEmail.Models;
using UnifiedEmail.Services;

namespace UnifiedEmail.ViewModels
{
    public partial class ComposeEmailViewModel : ObservableObject
    {
        private readonly EmailService _emailService = new();
        private readonly EmailAccountModel _account;

        public ComposeEmailViewModel(EmailAccountModel account)
        {
            _account = account;
        }

        [ObservableProperty] private string toEmail;
        [ObservableProperty] private string subject;
        [ObservableProperty] private string body;
        [ObservableProperty] private bool isSending;

        public ObservableCollection<string> Attachments { get; } = new();

        public Action? CloseAction { get; set; } // 창 닫기 콜백

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
