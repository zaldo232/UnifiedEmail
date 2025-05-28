namespace UnifiedEmail.Models
{
    public class MailProviderModel
    {
        public string Name { get; set; }
        public string ImapServer { get; set; }
        public int ImapPort { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public bool UseSsl { get; set; }
    }
}
