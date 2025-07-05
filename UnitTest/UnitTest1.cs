using MicroService.Infrastructure.Security;

namespace UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void AESEncry_Test1()
        {
            string password = "MySecurePassword123!";

            // 哈希密码
            string hashedPassword = Pbkdf2Hasher.HashPassword(password);
            Console.WriteLine($"Hashed Password: {hashedPassword}");

            // 验证密码
            bool isValid = Pbkdf2Hasher.VerifyPassword(password, hashedPassword);
            Console.WriteLine($"Password valid: {isValid}"); // true

            // 使用错误密码验证
            bool isInvalid = Pbkdf2Hasher.VerifyPassword("WrongPassword", hashedPassword);
            Console.WriteLine($"Password valid: {isInvalid}"); // false
        }
    }
}