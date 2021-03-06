﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Chat.Providers
{
    public static class MailProvider
    {
        private static readonly string PasswordHash = "P@@Sw0rd";
        private static readonly string SaltKey = "S@LT&KEY";
        private static readonly string VIKey = "@1B2c3D4e5F6g7H8";
        private static string Decrypt(string encryptedText)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }
        public static void SendMail(string sendto, string token)
        {
            Configuration config = System.Web.HttpContext.Current != null ?
                System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~") :
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var mailSettings = config.GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;
            if (mailSettings == null) return;
            try
            {
                int port = mailSettings.Smtp.Network.Port;
                string from = mailSettings.Smtp.From;
                string host = mailSettings.Smtp.Network.Host;
                string pwd = Decrypt(mailSettings.Smtp.Network.Password);
                string uid = Decrypt(mailSettings.Smtp.Network.UserName);
                var message = new MailMessage
                {
                    From = new MailAddress(@from)
                };


                string body = "http://localhost:48088//Home/ActivatePage?token=" + token;
                message.To.Add(new MailAddress(sendto));
                message.CC.Add(new MailAddress(from));
                message.Subject = "Activate your account!";
                message.Body = body;
                var client = new SmtpClient
                {
                    Host = host,
                    Port = port,
                    Credentials = new NetworkCredential(uid, pwd),
                    EnableSsl = true
                };
                client.Send(message);

            }
            catch
            {
                throw;
            }
        }
    }
}
