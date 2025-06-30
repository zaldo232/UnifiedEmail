using CommunityToolkit.Mvvm.ComponentModel;
using MimeKit;
using UnifiedEmail.Models;

namespace UnifiedEmail.ViewModels
{
    // 이메일 상세보기 뷰모델
    public partial class EmailDetailViewModel : ObservableObject
    {
        [ObservableProperty] private string subject; // 제목
        [ObservableProperty] private string from;    // 발신자
        [ObservableProperty] private string date;    // 날짜/시간
        [ObservableProperty] private string body;    // 본문

        public List<MimePart> Attachments { get; } = new();      // 첨부파일 목록
        public MimeMessage OriginalMessage { get; }              // 원본 메시지
        public EmailAccountModel ReplyAccount { get; }           // 답장 계정 정보

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
                { 
                    Attachments.Add(mimePart); 
                }
            }
        }
    }
}
