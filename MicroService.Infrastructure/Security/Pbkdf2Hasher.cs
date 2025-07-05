using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Infrastructure.Security
{
    public class Pbkdf2Hasher
    {
        // 默认迭代次数 (符合NIST建议的最小值)
        private const int DefaultIterations = 100000;

        // 默认密钥长度 (256位 = 32字节)
        private const int DefaultKeyLength = 32;

        // 默认盐长度 (128位 = 16字节)
        private const int DefaultSaltLength = 16;

        /// <summary>
        /// 生成PBKDF2哈希
        /// </summary>
        /// <param name="password">原始密码</param>
        /// <param name="iterations">迭代次数</param>
        /// <param name="salt">可选盐值(为空则自动生成)</param>
        /// <param name="keyLength">密钥长度(字节)</param>
        /// <returns>格式: 迭代次数:盐:哈希值</returns>
        public static string HashPassword(string password, int iterations = DefaultIterations,
            byte[]? salt = null, int keyLength = DefaultKeyLength)
        {
            // 1. 生成盐值(如果未提供)
            salt ??= GenerateSalt(DefaultSaltLength);

            // 2. 使用PBKDF2派生密钥
            using var deriveBytes = new Rfc2898DeriveBytes(
                password: Encoding.UTF8.GetBytes(password),
                salt: salt,
                iterations: iterations,
                hashAlgorithm: HashAlgorithmName.SHA256);

            byte[] hashBytes = deriveBytes.GetBytes(keyLength);

            // 3. 组合格式: 迭代次数:盐:哈希
            return $"{iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hashBytes)}";
        }

        /// <summary>
        /// 验证密码是否匹配
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // 1. 拆分存储的哈希值
            var parts = hashedPassword.Split(':');
            if (parts.Length != 3)
                return false;

            // 2. 解析组件
            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] storedHash = Convert.FromBase64String(parts[2]);

            // 3. 使用相同参数重新计算哈希
            using var deriveBytes = new Rfc2898DeriveBytes(
                password: Encoding.UTF8.GetBytes(password),
                salt: salt,
                iterations: iterations,
                hashAlgorithm: HashAlgorithmName.SHA256);

            byte[] computedHash = deriveBytes.GetBytes(storedHash.Length);

            // 4. 恒定时间比较(防止时序攻击)
            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }

        /// <summary>
        /// 生成加密安全的随机盐
        /// </summary>
        public static byte[] GenerateSalt(int length = DefaultSaltLength)
        {
            byte[] salt = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }
    }
}
