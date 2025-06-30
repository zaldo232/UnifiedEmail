namespace UnifiedEmail.Models
{
    // 이메일 계정 정보 모델
    public class EmailAccountModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // 계정 고유 ID
        public string Email { get; set; } // 이메일 주소
        public string PasswordEncrypted { get; set; } // 암호화된 비밀번호

        public string ImapServer { get; set; } // IMAP 서버 주소
        public int ImapPort { get; set; } // IMAP 포트
        public string SmtpServer { get; set; } // SMTP 서버 주소
        public int SmtpPort { get; set; } // SMTP 포트

        public bool UseImap { get; set; } = true; // IMAP 사용 여부
        public bool UseSsl { get; set; } = true; // SSL 사용 여부

        public string Provider { get; set; } // 메일 제공업체 이름
    }
}
