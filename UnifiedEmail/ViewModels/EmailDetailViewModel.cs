using CommunityToolkit.Mvvm.ComponentModel;
using MimeKit;
using UnifiedEmail.Models;

namespace UnifiedEmail.ViewModels
{
    public partial class EmailDetailViewModel : ObservableObject
    {
        [ObservableProperty] private string subject;
        [ObservableProperty] private string from;
        [ObservableProperty] private string date;
        [ObservableProperty] private string body;

        public List<MimePart> Attachments { get; } = new();
        public MimeMessage OriginalMessage { get; }
        public EmailAccountModel ReplyAccount { get; }

        public EmailDetailViewModel(MimeMessage message, EmailAccountModel account)
        {
            OriginalMessage = message;
            ReplyAccount = account;

            Subject = message.Subject;
            From = message.From.ToString();
            Date = message.Date.ToString("yyyy-MM-dd HH:mm");
            Body = message.HtmlBody ?? message.TextBody ?? "(본문 없음)";

            // 첨부파일 추출
            foreach (var part in message.Attachments)
            {
                if (part is MimePart mimePart)
                    Attachments.Add(mimePart);
            }
        }
    }
}
