using System.Windows;
using UnifiedEmail.Models;
using UnifiedEmail.ViewModels;
using UnifiedEmail.Views;

namespace UnifiedEmail
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenAccountManager_Click(object sender, RoutedEventArgs e)
        {
            var win = new Window
            {
                Title = "계정 관리",
                Content = new AccountManagerView(),
                Width = 600,
                Height = 500
            };
            win.ShowDialog();
        }

        private void ComposeEmail_Click(object sender, RoutedEventArgs e)
        {
            // EmailListView 내부 ViewModel에 접근
            if (FindName("MainEmailListView") is not EmailListView emailListView)
                return;

            if (emailListView.DataContext is not EmailListViewModel vm)
                return;

            if (vm.SelectedAccount is null)
            {
                MessageBox.Show("메일을 보낼 계정을 선택해주세요.");
                return;
            }

            var composeVm = new ComposeEmailViewModel(vm.SelectedAccount);
            var view = new ComposeEmailView { DataContext = composeVm };

            var window = new Window
            {
                Title = "메일 작성",
                Content = view,
                Width = 600,
                Height = 600
            };
            window.ShowDialog();
        }
    }
}
