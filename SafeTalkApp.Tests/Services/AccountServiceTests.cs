using Moq;
using SafeTalkApp.DTOs.Account;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Fakes;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class AccountServiceTests
    {
        private SafeTalkAppContext? _fakeContext;
        private AccountService? _service;
        private Mock<IEmailService>? _mockEmail;

        [TestInitialize]
        public void Setup()
        {
            _fakeContext = new SafeTalkAppContext
            {
                user_tbl = new FakeDbSet<UserTblModel>(),
                user_role_tbl = new FakeDbSet<UserRoleTblModel>(),
                user_availability_tbl = new FakeDbSet<UserAvailabilityTblModel>(),
                role_tbl = new FakeDbSet<RoleTblModel>()
            };

            _mockEmail = new Mock<IEmailService>();
            _service = new AccountService(_fakeContext, _mockEmail.Object);
        }

        [TestMethod]
        public void RegisterUser_ShouldCreateUser()
        {
            // Arrange
            var dto = new SignUpDTO
            {
                firstName = "John",
                lastName = "Doe",
                birthDate = new DateTime(1990, 1, 1),
                genderID = 1,
                phoneNumber = "09123456789",
                email = "john@test.com",
                password = "Password123!",
                
                roleID = 1
            };

            // Act
            var result = _service.RegisterUser(dto);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(1, _fakeContext.user_tbl.Count());
        }

        [TestMethod]
        public void AuthenticateUser_ShouldFail_WhenEmailNotFound()
        {
            // Act
            var result = _service.AuthenticateUser(new LoginDTO
            {
                email = "notfound@test.com",
                password = "wrong"
            });

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Invalid email or password.", result.message);
        }
    }
}
