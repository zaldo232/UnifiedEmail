using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using UnifiedEmail.Models;

namespace UnifiedEmail.Services
{
    // 이메일 계정 DB 관리 서비스
    public class DatabaseService
    {
        private const string DbPath = "email_accounts.db"; // DB 파일 경로

        public DatabaseService()
        {
            InitializeDatabase(); // DB/테이블 초기화
        }

        // 테이블 생성 (최초 1회)
        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS EmailAccounts (
                    Id TEXT PRIMARY KEY,
                    Email TEXT,
                    PasswordEncrypted TEXT,
                    ImapServer TEXT,
                    ImapPort INTEGER,
                    SmtpServer TEXT,
                    SmtpPort INTEGER,
                    UseImap INTEGER,
                    UseSsl INTEGER,
                    Provider TEXT
                );
            ";
            command.ExecuteNonQuery();
        }

        // 이메일 계정 추가
        public void AddEmailAccount(EmailAccountModel account)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                INSERT INTO EmailAccounts (
                    Id, Email, PasswordEncrypted,
                    ImapServer, ImapPort, SmtpServer, SmtpPort,
                    UseImap, UseSsl, Provider
                )
                VALUES (
                    $id, $email, $password,
                    $imapServer, $imapPort, $smtpServer, $smtpPort,
                    $useImap, $useSsl, $provider
                );
            ";

            command.Parameters.AddWithValue("$id", account.Id ?? string.Empty); // 고유 ID
            command.Parameters.AddWithValue("$email", account.Email ?? string.Empty); // 이메일 주소
            command.Parameters.AddWithValue("$password", account.PasswordEncrypted ?? string.Empty); // 암호화 PW
            command.Parameters.AddWithValue("$imapServer", account.ImapServer ?? string.Empty); // IMAP 서버
            command.Parameters.AddWithValue("$imapPort", account.ImapPort); // IMAP 포트
            command.Parameters.AddWithValue("$smtpServer", account.SmtpServer ?? string.Empty); // SMTP 서버
            command.Parameters.AddWithValue("$smtpPort", account.SmtpPort); // SMTP 포트
            command.Parameters.AddWithValue("$useImap", account.UseImap ? 1 : 0); // IMAP 사용 여부
            command.Parameters.AddWithValue("$useSsl", account.UseSsl ? 1 : 0); // SSL 사용 여부
            command.Parameters.AddWithValue("$provider", account.Provider ?? "Custom"); // 제공업체

            command.ExecuteNonQuery();
        }

        // 모든 이메일 계정 리스트 조회
        public List<EmailAccountModel> GetAllAccounts()
        {
            var list = new List<EmailAccountModel>();
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM EmailAccounts";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new EmailAccountModel
                {
                    Id = reader.GetString(0), // ID
                    Email = reader.GetString(1), // 이메일
                    PasswordEncrypted = reader.GetString(2), // 암호화 PW
                    ImapServer = reader.GetString(3), // IMAP 서버
                    ImapPort = reader.GetInt32(4), // IMAP 포트
                    SmtpServer = reader.GetString(5), // SMTP 서버
                    SmtpPort = reader.GetInt32(6), // SMTP 포트
                    UseImap = reader.GetInt32(7) == 1, // IMAP 사용 여부
                    UseSsl = reader.GetInt32(8) == 1, // SSL 사용 여부
                    Provider = reader.IsDBNull(9) ? "Custom" : reader.GetString(9) // 제공업체
                });
            }

            return list;
        }

        // 이메일 계정 삭제
        public void DeleteEmailAccount(string id)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"DELETE FROM EmailAccounts WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id); // 삭제할 ID
            command.ExecuteNonQuery();
        }
    }
}
