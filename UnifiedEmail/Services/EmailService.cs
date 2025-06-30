using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using UnifiedEmail.Models;

namespace UnifiedEmail.Services
{
    // 메일 송수신/삭제 등 IMAP/SMTP 서비스
    public class EmailService
    {
        // 메일 수신 (Inbox, 최대 30개)
        public async Task<List<(UniqueId Uid, MimeMessage Message, MessageFlags Flags)>> FetchInboxAsync(EmailAccountModel account)
        {
            var result = new List<(UniqueId, MimeMessage, MessageFlags)>();
            using var client = new ImapClient();

            try
            {
                var decryptedPassword = EncryptionService.Decrypt(account.PasswordEncrypted); // 비밀번호 복호화
                await client.ConnectAsync(account.ImapServer, account.ImapPort, account.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls); // IMAP 접속
                await client.AuthenticateAsync(account.Email, decryptedPassword); // 인증

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                int fetchCount = Math.Min(30, inbox.Count); // 최대 30개 제한
                if (fetchCount == 0)
                {
                    await client.DisconnectAsync(true);
                    return result;
                }

                // 최근 fetchCount개 메일 요약 조회
                var summaries = inbox.Fetch(
                    inbox.Count - fetchCount,
                    -1,
                    MessageSummaryItems.UniqueId | MessageSummaryItems.Flags
                );

                foreach (var summary in summaries) // 각 메일 상세 가져오기
                {
                    var uid = summary.UniqueId;
                    var flags = summary.Flags ?? MessageFlags.None;
                    var message = await inbox.GetMessageAsync(uid);
                    result.Add((uid, message, flags));
                }

                await client.DisconnectAsync(true); // 연결 종료
            }
            catch
            {
                throw; // 예외 재전파
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

            foreach (var file in attachments) // 첨부파일 추가
            { 
                builder.Attachments.Add(file); 
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            var password = EncryptionService.Decrypt(account.PasswordEncrypted);

            // 포트별 보안옵션 자동 선택
            var socketOption = account.SmtpPort switch
            {
                465 => SecureSocketOptions.SslOnConnect,
                587 => SecureSocketOptions.StartTls,
                _ => SecureSocketOptions.Auto
            };

            await client.ConnectAsync(account.SmtpServer, account.SmtpPort, socketOption); // SMTP 접속
            await client.AuthenticateAsync(account.Email, password); // 인증
            await client.SendAsync(message); // 메일 전송
            await client.DisconnectAsync(true); // 연결 종료
        }

        // 메일 삭제 (특정 UID)
        public async Task DeleteEmailAsync(EmailAccountModel account, UniqueId uid)
        {
            using var client = new ImapClient();
            var password = EncryptionService.Decrypt(account.PasswordEncrypted);

            await client.ConnectAsync(account.ImapServer, account.ImapPort, account.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(account.Email, password);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadWrite);

            await inbox.AddFlagsAsync(uid, MessageFlags.Deleted, true); // 삭제 플래그 추가
            await inbox.ExpungeAsync(); // 실제 삭제

            await client.DisconnectAsync(true);
        }

        // 폴더명 지정 후 메일 수신 (최대 30개)
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
                var folder = await client.GetFolderAsync(folderName); // 폴더 열기
                await folder.OpenAsync(FolderAccess.ReadOnly);

                // 최근 30개 요약 가져오기
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