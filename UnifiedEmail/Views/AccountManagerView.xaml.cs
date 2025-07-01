using System.Windows;
using System.Windows.Controls;
using UnifiedEmail.ViewModels;

namespace UnifiedEmail.Views
{
    // 계정 관리 뷰
    public partial class AccountManagerView : UserControl
    {
        // 생성자
        public AccountManagerView()
        {
            InitializeComponent();
        }

        // 비밀번호 입력 시 ViewModel에 값 동기화
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AccountViewModel vm)
            {
                vm.Password = ((PasswordBox)sender).Password; // 비밀번호 전달
            }
        }
    }
}
