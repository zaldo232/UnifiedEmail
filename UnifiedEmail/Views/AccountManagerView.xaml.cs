using System.Windows;
using System.Windows.Controls;
using UnifiedEmail.ViewModels;

namespace UnifiedEmail.Views
{
    public partial class AccountManagerView : UserControl
    {
        public AccountManagerView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AccountViewModel vm)
            {
                vm.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}
