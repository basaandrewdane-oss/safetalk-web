using Moq;
using SafeTalkApp.DTOs.Account;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class AccountServiceTests
    {
        private Mock<ISafeTalkAppContext> _mockContext = null!;
        private Mock<IEmailService> _mockEmail = null!;
        private AccountService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<ISafeTalkAppContext>();
            _mockEmail = new Mock<IEmailService>();

            _service = new AccountService(_mockContext.Object, _mockEmail.Object);
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

            var mockUserDbSet = MockDbSetHelper.BuildMockDbSet(new List<UserTblModel>().AsQueryable());
            var mockUserRoleDbSet = MockDbSetHelper.BuildMockDbSet(new List<UserRoleTblModel>().AsQueryable());
            var mockAvailabilityDbSet = MockDbSetHelper.BuildMockDbSet(new List<UserAvailabilityTblModel>().AsQueryable());

            _mockContext.Setup(c => c.user_tbl).Returns(mockUserDbSet.Object);
            _mockContext.Setup(c => c.user_role_tbl).Returns(mockUserRoleDbSet.Object);
            _mockContext.Setup(c => c.user_availability_tbl).Returns(mockAvailabilityDbSet.Object);

            _mockContext.Setup(c => c.SaveChanges()).Returns(1);

            // Act
            var result = _service.RegisterUser(dto);

            // Assert
            Assert.IsTrue(result.success);
            mockUserDbSet.Verify(db => db.Add(It.Is<UserTblModel>(u => u.email == dto.email)    ), Times.Once);
            mockUserRoleDbSet.Verify(db => db.Add(It.Is<UserRoleTblModel>(ur => ur.roleID == dto.roleID)), Times.Once);
            _mockContext.Verify(c => c.SaveChanges(), Times.AtLeast(2));
        }

        [TestMethod]
        public void RegisterUser_ShouldCreateDoctorWithAvailability()
        {
            // Arrange
            var dto = new SignUpDTO
            {
                firstName = "Alice",
                lastName = "Smith",
                birthDate = new DateTime(1985, 5, 5),
                genderID = 2,
                phoneNumber = "09998887777",
                email = "alice@doc.com",
                password = "DocPassword123!",
                roleID = 2,
                licenseNumber = "1236543",
                specialization = "Gynecologist",
                availability = new List<AvailabilityDTO>
                {
                    new AvailabilityDTO { dayID = 1, availabilityStart = "09:00", availabilityEnd = "17:00", fee = 500 }
                }
            };

            var mockUserDbSet = MockDbSetHelper.BuildMockDbSet(new List<UserTblModel>().AsQueryable());
            var mockUserRoleDbSet = MockDbSetHelper.BuildMockDbSet(new List<UserRoleTblModel>().AsQueryable());
            var mockAvailabilityDbSet = MockDbSetHelper.BuildMockDbSet(new List<UserAvailabilityTblModel>().AsQueryable());

            _mockContext.Setup(c => c.user_tbl).Returns(mockUserDbSet.Object);
            _mockContext.Setup(c => c.user_role_tbl).Returns(mockUserRoleDbSet.Object);
            _mockContext.Setup(c => c.user_availability_tbl).Returns(mockAvailabilityDbSet.Object);
            _mockContext.Setup(c => c.SaveChanges()).Returns(1);

            // Act
            var result = _service.RegisterUser(dto);

            // Assert
            Assert.IsTrue(result.success);
            mockUserDbSet.Verify(db => db.Add(It.Is<UserTblModel>(u => u.isVerified == false && u.licenseNumber == "1236543")), Times.Once);
            mockUserRoleDbSet.Verify(db => db.Add(It.Is<UserRoleTblModel>(ur => ur.roleID == 2)), Times.Once);
            mockAvailabilityDbSet.Verify(db => db.Add(It.IsAny<UserAvailabilityTblModel>()), Times.Once);
        }

        [TestMethod]
        public void RegisterUser_ShouldFail_WhenDbThrows()
        {
            // Arrange
            var dto = new SignUpDTO
            {
                firstName = "Bob",
                lastName = "Fail",
                birthDate = DateTime.Now,
                genderID = 1,
                phoneNumber = "09001112222",
                email = "fail@test.com",
                password = "Fail123!",
                roleID = 1
            };

            _mockContext.Setup(c => c.user_tbl).Throws(new Exception("DB error"));

            // Act
            var result = _service.RegisterUser(dto);

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsTrue(result.message.Contains("DB error"));
        }

        [TestMethod]
        public void AuthenticateUser_ShouldFail_WhenEmailNotFound()
        {
            // Arrange
            var fakeData = new List<UserTblModel>().AsQueryable(); // Empty list to simulate no users
            var mockUserDbSet = MockDbSetHelper.BuildMockDbSet(fakeData);

            _mockContext.Setup(c => c.user_tbl).Returns(mockUserDbSet.Object);

            // Act
            var result = _service.AuthenticateUser(new LoginDTO
            {
                email = "notfound@test.com",
                password = "wrong"
            });

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Invalid email.", result.message);
        }

        [TestMethod]
        public void AuthenticateUser_ShouldFail_WhenEmailNotVerified()
        {
            // Arrange
            var fakeUser = new UserTblModel
            {
                userID = 1,
                email = "johndoe@email.com",
                password = BCrypt.Net.BCrypt.HashPassword("Password!1"),
                isEmailVerified = false, // ✅ this triggers the failure
                isVerified = true,       // irrelevant for non-doctor
                firstName = "John",
                lastName = "Doe"
            };

            var fakeData = new List<UserTblModel> { fakeUser }.AsQueryable();
            var mockSet = MockDbSetHelper.BuildMockDbSet(fakeData);

            _mockContext.Setup(c => c.user_tbl).Returns(mockSet.Object);

            // Act
            var result = _service.AuthenticateUser(new LoginDTO
            {
                email = "johndoe@email.com",
                password = "Password!1"
            });

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Please verify your email before logging in.", result.message);
        }

        [TestMethod]
        public void AuthenticateUser_ShouldFail_WhenDoctorNotVerified()
        {
            // Arrange
            var fakeUser = new UserTblModel
            {
                userID = 1,
                email = "johndoe@email.com",
                password = BCrypt.Net.BCrypt.HashPassword("Password!1"),
                isEmailVerified = true,   // ✅ must be true to reach doctor check
                isVerified = false,       // ✅ doctor not verified
                firstName = "John",
                lastName = "Doe"
            };

            var fakeUsers = new List<UserTblModel> { fakeUser }.AsQueryable();
            var fakeUserRoles = new List<UserRoleTblModel>
            {
                new UserRoleTblModel { userID = fakeUser.userID, roleID = 2 }
            }.AsQueryable();
            var fakeRoles = new List<RoleTblModel>
            {
                new RoleTblModel { roleID = 2, roleName = "Doctor" }
            }.AsQueryable();

            var mockUsers = MockDbSetHelper.BuildMockDbSet(fakeUsers);
            var mockUserRoles = MockDbSetHelper.BuildMockDbSet(fakeUserRoles);
            var mockRoles = MockDbSetHelper.BuildMockDbSet(fakeRoles);

            _mockContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);
            _mockContext.Setup(c => c.user_role_tbl).Returns(mockUserRoles.Object);
            _mockContext.Setup(c => c.role_tbl).Returns(mockRoles.Object);

            // Act
            var result = _service.AuthenticateUser(new LoginDTO
            {
                email = "johndoe@email.com",
                password = "Password!1"
            });

            // Assert
            Assert.IsFalse(result.success);
            // doctor flagged as not verified
            Assert.AreEqual("Your account is pending verification. Please wait for an administrator to verify your account.", result.message);
        }

        
    }
}
