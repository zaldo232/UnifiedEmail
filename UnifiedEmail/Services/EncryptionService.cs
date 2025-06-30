using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UnifiedEmail.Services
{
    // AES 대칭키 문자열 암복호화 서비스
    public static class EncryptionService
    {
        private static readonly string _key = "7fKxZ92qP1vT3rGbLcN8mYd5AwRQ0zXe"; // 32바이트(256bit) 키
        private static readonly string _iv = "HT9eL1q8Zr5pWb3K"; // 16바이트(128bit) IV

        // 평문을 암호화해서 Base64 문자열로 반환
        public static string Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_key);
            aes.IV = Encoding.UTF8.GetBytes(_iv);

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
                sw.Write(plainText); // 평문 작성

            return Convert.ToBase64String(ms.ToArray()); // Base64 인코딩 반환
        }

        // 암호문(Base64)을 복호화해서 평문 반환
        public static string Decrypt(string cipherText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_key);
            aes.IV = Encoding.UTF8.GetBytes(_iv);

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd(); // 평문 반환
        }
    }
}
