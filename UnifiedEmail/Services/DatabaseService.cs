using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using UnifiedEmail.Models;

namespace UnifiedEmail.Services
{
    public class DatabaseService
    {
        private const string DbPath = "email_accounts.db";

        public DatabaseService()
        {
            InitializeDatabase();
        }

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

            command.Parameters.AddWithValue("$id", account.Id ?? string.Empty);
            command.Parameters.AddWithValue("$email", account.Email ?? string.Empty);
            command.Parameters.AddWithValue("$password", account.PasswordEncrypted ?? string.Empty);
            command.Parameters.AddWithValue("$imapServer", account.ImapServer ?? string.Empty);
            command.Parameters.AddWithValue("$imapPort", account.ImapPort);
            command.Parameters.AddWithValue("$smtpServer", account.SmtpServer ?? string.Empty);
            command.Parameters.AddWithValue("$smtpPort", account.SmtpPort);
            command.Parameters.AddWithValue("$useImap", account.UseImap ? 1 : 0);
            command.Parameters.AddWithValue("$useSsl", account.UseSsl ? 1 : 0);
            command.Parameters.AddWithValue("$provider", account.Provider ?? "Custom");

            command.ExecuteNonQuery();
        }

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
                    Id = reader.GetString(0),
                    Email = reader.GetString(1),
                    PasswordEncrypted = reader.GetString(2),
                    ImapServer = reader.GetString(3),
                    ImapPort = reader.GetInt32(4),
                    SmtpServer = reader.GetString(5),
                    SmtpPort = reader.GetInt32(6),
                    UseImap = reader.GetInt32(7) == 1,
                    UseSsl = reader.GetInt32(8) == 1,
                    Provider = reader.IsDBNull(9) ? "Custom" : reader.GetString(9)
                });
            }

            return list;
        }

        public void DeleteEmailAccount(string id)
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"DELETE FROM EmailAccounts WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }
    }
}
