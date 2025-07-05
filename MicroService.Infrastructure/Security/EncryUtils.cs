using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MicroService.Infrastructure.Security
{
    public static class EncryUtils
    {
        /// <summary>
        /// AES 加密
        /// </summary>
        /// <param name="plainText">加密文本</param>
        /// <param name="secret">密钥</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string AESEncrypt(string plainText, string secret)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));
            if (secret == null || secret.Length == 0)
                throw new ArgumentNullException(nameof(secret));
            
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(secret);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // 生成随机IV
                aes.GenerateIV();

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream ms = new MemoryStream())
                {
                    // 先写入IV（未加密）
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// AES 解密
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <param name="secret">密钥</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Decrypt(string cipherText, string secret)
        {
            // 输入验证
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));
            if (string.IsNullOrEmpty(secret))
                throw new ArgumentNullException(nameof(secret));
           
            byte[] fullCipher = Convert.FromBase64String(cipherText);

            // 提取IV（前16字节）
            byte[] iv = new byte[16];
            byte[] cipherBytes = new byte[fullCipher.Length - iv.Length];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            Array.Copy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(secret);
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream ms = new MemoryStream(cipherBytes))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }


    }
}
