using ApiIdentityService.Application.Security;

namespace ApiTestIdentityService.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashAndVerify_WorksCorrectly()
        {
            var password = "StrongPassword123!";

            var (hash, salt) = PasswordHasher.HashPassword(password);

            Assert.True(PasswordHasher.VerifyPassword(password, hash, salt));
            Assert.False(PasswordHasher.VerifyPassword("wrong", hash, salt));
        }
    }
}
