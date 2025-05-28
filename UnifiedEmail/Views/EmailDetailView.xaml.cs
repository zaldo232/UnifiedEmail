using MimeKit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.IO;

namespace UnifiedEmail.Views
{
    public partial class EmailDetailView : UserControl
    {
        public EmailDetailView()
        {
            InitializeComponent();
            this.Loaded += EmailDetailView_Loaded;
        }

        private void EmailDetailView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.EmailDetailViewModel vm)
            {
                string htmlContent = vm.Body ?? "(본문 없음)";

                // charset UTF-8 명시
                string htmlWrapped = $@"
                    <html>
                        <head>
                            <meta charset=""UTF-8"">
                        </head>
                        <body>
                            {htmlContent}
                        </body>
                    </html>";

                Browser.NavigateToString(htmlWrapped);
            }
        }

        private void SaveAttachment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not MimePart attachment)
                return;

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = attachment.FileName,
                Filter = "All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                using var stream = File.Create(dialog.FileName);
                attachment.Content.DecodeTo(stream);
                MessageBox.Show("저장 완료", "완료", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


    }
}
