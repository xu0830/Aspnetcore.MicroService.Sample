using MicroService.Infrastructure.Security;

namespace UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void AESEncry_Test1()
        {
            string password = "MySecurePassword123!";

            // ��ϣ����
            string hashedPassword = Pbkdf2Hasher.HashPassword(password);
            Console.WriteLine($"Hashed Password: {hashedPassword}");

            // ��֤����
            bool isValid = Pbkdf2Hasher.VerifyPassword(password, hashedPassword);
            Console.WriteLine($"Password valid: {isValid}"); // true

            // ʹ�ô���������֤
            bool isInvalid = Pbkdf2Hasher.VerifyPassword("WrongPassword", hashedPassword);
            Console.WriteLine($"Password valid: {isInvalid}"); // false
        }
    }
}