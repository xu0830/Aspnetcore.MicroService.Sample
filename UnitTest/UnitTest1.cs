using MicroService.Infrastructure.Security;

namespace UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void AESEncry_Test1()
        {
            var secret = "";
            var re = EncryUtils.AESEncrypt("123", secret);

            var de = EncryUtils.Decrypt(re, secret);
        }
    }
}