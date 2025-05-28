using System;

namespace UnifiedEmail.Models
{
    public class EmailAccountModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Email { get; set; }
        public string PasswordEncrypted { get; set; }

        public string ImapServer { get; set; }
        public int ImapPort { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }

        public bool UseImap { get; set; } = true;
        public bool UseSsl { get; set; } = true;

        public string Provider { get; set; }
    }
}
