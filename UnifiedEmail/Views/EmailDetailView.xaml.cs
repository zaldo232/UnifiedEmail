using MimeKit;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace UnifiedEmail.Views
{
    // 메일 상세 보기 뷰
    public partial class EmailDetailView : UserControl
    {
        // 생성자
        public EmailDetailView()
        {
            InitializeComponent();
            this.Loaded += EmailDetailView_Loaded;
        }

        // 뷰 로드 시 본문 HTML 표시
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

                Browser.NavigateToString(htmlWrapped); // 본문 표시
            }
        }

        // 첨부파일 저장 버튼 클릭
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
                attachment.Content.DecodeTo(stream); // 파일 저장
                MessageBox.Show("저장 완료", "완료", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
