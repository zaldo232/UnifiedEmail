using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using UnifiedEmail.Models;

namespace UnifiedEmail.Services
{
    public class EmailService
    {
        // 메일 수신 (기본 Inbox)
        public async Task<List<(UniqueId Uid, MimeMessage Message, MessageFlags Flags)>> FetchInboxAsync(EmailAccountModel account)
        {
            var result = new List<(UniqueId, MimeMessage, MessageFlags)>();
            using var client = new ImapClient();

            try
            {
                var decryptedPassword = EncryptionService.Decrypt(account.PasswordEncrypted);
                await client.ConnectAsync(account.ImapServer, account.ImapPort,
                    account.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(account.Email, decryptedPassword);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                int fetchCount = Math.Min(30, inbox.Count);
                if (fetchCount == 0)
                {
                    await client.DisconnectAsync(true);
                    return result;
                }

                var summaries = inbox.Fetch(
                    inbox.Count - fetchCount,
                    -1,
                    MessageSummaryItems.UniqueId | MessageSummaryItems.Flags
                );

                foreach (var summary in summaries)
                {
                    var uid = summary.UniqueId;
                    var flags = summary.Flags ?? MessageFlags.None;
                    var message = await inbox.GetMessageAsync(uid);
                    result.Add((uid, message, flags));
                }

                await client.DisconnectAsync(true);
            }
            catch
            {
                throw;
            }

            return result;
        }

        // 메일 전송
        public async Task SendEmailAsync(EmailAccountModel account, string to, string subject, string body, IEnumerable<string> attachments)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(account.Email));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };

            foreach (var file in attachments)
                builder.Attachments.Add(file);

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            var password = EncryptionService.Decrypt(account.PasswordEncrypted);

            var socketOption = account.SmtpPort switch
            {
                465 => SecureSocketOptions.SslOnConnect,
                587 => SecureSocketOptions.StartTls,
                _ => SecureSocketOptions.Auto
            };

            await client.ConnectAsync(account.SmtpServer, account.SmtpPort, socketOption);
            await client.AuthenticateAsync(account.Email, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        // 메일 삭제
        public async Task DeleteEmailAsync(EmailAccountModel account, UniqueId uid)
        {
            using var client = new ImapClient();
            var password = EncryptionService.Decrypt(account.PasswordEncrypted);

            await client.ConnectAsync(account.ImapServer, account.ImapPort, account.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(account.Email, password);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadWrite);

            await inbox.AddFlagsAsync(uid, MessageFlags.Deleted, true);
            await inbox.ExpungeAsync();

            await client.DisconnectAsync(true);
        }

        // 폴더 지정 후 메일 수신
        public async Task<List<(UniqueId, MimeMessage, MessageFlags)>> FetchFromFolderAsync(EmailAccountModel account, string folderName)
        {
            var result = new List<(UniqueId, MimeMessage, MessageFlags)>();
            using var client = new ImapClient();

            var password = EncryptionService.Decrypt(account.PasswordEncrypted);
            var imapOption = account.ImapPort switch
            {
                993 => SecureSocketOptions.SslOnConnect,
                143 => SecureSocketOptions.StartTls,
                _ => SecureSocketOptions.Auto
            };

            await client.ConnectAsync(account.ImapServer, account.ImapPort, imapOption);
            await client.AuthenticateAsync(account.Email, password);

            try
            {
                var folder = await client.GetFolderAsync(folderName);
                await folder.OpenAsync(FolderAccess.ReadOnly);

                var summaries = folder.Fetch(
                    Math.Max(0, folder.Count - 30),
                    -1,
                    MessageSummaryItems.UniqueId | MessageSummaryItems.Flags
                );

                foreach (var summary in summaries)
                {
                    var uid = summary.UniqueId;
                    var flags = summary.Flags ?? MessageFlags.None;
                    var message = await folder.GetMessageAsync(uid);
                    result.Add((uid, message, flags));
                }
            }
            catch (FolderNotFoundException ex)
            {
                throw new Exception("IMAP 폴더를 찾을 수 없습니다: " + folderName, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("메일 불러오기 중 오류 발생: " + ex.Message, ex);
            }
            finally
            {   
                await client.DisconnectAsync(true);
            }

            return result;
        }
    }
}
