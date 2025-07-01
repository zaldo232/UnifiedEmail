using MimeKit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UnifiedEmail.Models;
using UnifiedEmail.ViewModels;

namespace UnifiedEmail.Views
{
    // 메일 목록 뷰
    public partial class EmailListView : UserControl
    {
        // 생성자
        public EmailListView()
        {
            InitializeComponent();
        }

        // 메일 더블클릭 시 상세보기 팝업
        private async void EmailList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not EmailListViewModel vm) return;
            if (EmailList.SelectedIndex < 0) return;

            // 선택된 메일 가져오기
            var selected = EmailList.SelectedItem as EmailMessageModel;
            if (selected == null) return;

            // 먼저 읽음 처리
            await vm.MarkAsReadAsync(selected);

            // 본문 메시지 가져오기
            var message = await vm.GetRawMessageByIndex(EmailList.SelectedIndex);
            if (message == null) return;

            // 본문 뷰 모델 + 뷰 구성
            var detailVm = new EmailDetailViewModel(message, vm.SelectedAccount);
            var detailView = new EmailDetailView { DataContext = detailVm };

            // 팝업 윈도우로 열기
            var win = new Window
            {
                Title = "메일 본문",
                Content = detailView,
                Width = 600,
                Height = 600
            };
            win.ShowDialog();
        }

        // 언로드 시 자동 새로고침 중지
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is EmailListViewModel vm)
            { 
                vm.StopAutoRefresh(); 
            }
        }
    }
}
