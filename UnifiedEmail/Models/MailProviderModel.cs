namespace UnifiedEmail.Models
{
    // 메일 서비스 제공업체 정보 모델
    public class MailProviderModel
    {
        public string Name { get; set; } // 제공업체 이름
        public string ImapServer { get; set; } // IMAP 서버 주소
        public int ImapPort { get; set; } // IMAP 포트
        public string SmtpServer { get; set; } // SMTP 서버 주소
        public int SmtpPort { get; set; } // SMTP 포트
        public bool UseSsl { get; set; } // SSL 사용 여부
    }
}
