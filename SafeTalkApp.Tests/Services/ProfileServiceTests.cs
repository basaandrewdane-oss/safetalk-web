using Moq;
using SafeTalkApp.DTOs.Profile;
using SafeTalkApp.Interfaces;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;
using System.Web;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class ProfileServiceTests
    {
        private Mock<ISafeTalkAppContext> _mockContext = null!;
        private Mock<IFileStorageService> _fileStorageService = null!;
        private ProfileService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<ISafeTalkAppContext>();
            _fileStorageService = new Mock<IFileStorageService>();
            _service = new ProfileService(_mockContext.Object, _fileStorageService.Object);
        }

        [TestMethod]
        public void GetProfile_ShouldReturnUserDTO_WhenUserExistsWithProfilePicture()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 1, email = "test@example.com", firstName = "John", lastName = "Doe", phoneNumber = "123", specialization = "Spec", profilePictureUrl = "john.png" }
            }.AsQueryable();

            var userRoles = new List<UserRoleTblModel>
            {
                new UserRoleTblModel { userID = 1, roleID = 2 }
            }.AsQueryable();

            var roles = new List<RoleTblModel>
            {
                new RoleTblModel { roleID = 2, roleName = "Admin" }
            }.AsQueryable();

            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users).Object);
            _mockContext.Setup(c => c.user_role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(userRoles).Object);
            _mockContext.Setup(c => c.role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(roles).Object);

            // Act
            var result = _service.GetProfile("1");

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);
            Assert.AreEqual("/Profile/GetProfilePicture?fileName=john.png", result.data.profilePictureUrl);
            Assert.AreEqual("John", result.data.firstName);
            Assert.AreEqual("Admin", result.data.role);
        }

        [TestMethod]
        public void GetProfile_ShouldReturnUserDTOWithDefaultAvatar_WhenUserExistsWithoutProfilePicture()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 2, email = "jane@example.com", firstName = "Jane", lastName = "Smith", phoneNumber = "456", specialization = "Spec", profilePictureUrl = null }
            }.AsQueryable();

            var userRoles = new List<UserRoleTblModel>
            {
                new UserRoleTblModel { userID = 2, roleID = 3 }
            }.AsQueryable();

            var roles = new List<RoleTblModel>
            {
                new RoleTblModel { roleID = 3, roleName = "User" }
            }.AsQueryable();

            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users).Object);
            _mockContext.Setup(c => c.user_role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(userRoles).Object);
            _mockContext.Setup(c => c.role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(roles).Object);

            // Act
            var result = _service.GetProfile("2");

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);
            Assert.AreEqual("/Uploads/ProfilePictures/default-avatar.png", result.data.profilePictureUrl);
            Assert.AreEqual("Jane", result.data.firstName);
            Assert.AreEqual("User", result.data.role);
        }

        [TestMethod]
        public void GetProfile_ShouldReturnFail_WhenUserDoesNotExist()
        {
            // Arrange
            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(new List<UserTblModel>().AsQueryable()).Object);
            _mockContext.Setup(c => c.user_role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(new List<UserRoleTblModel>().AsQueryable()).Object);
            _mockContext.Setup(c => c.role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(new List<RoleTblModel>().AsQueryable()).Object);

            // Act
            var result = _service.GetProfile("99");

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("User not found.", result.message);
            Assert.IsNull(result.data);
        }

        [TestMethod]
        public void UpdateProfile_ShouldFail_WhenDtoIsNull()
        {
            // Act
            var result = _service.UpdateProfile(null, "1");

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Invalid input.", result.message);
        }

        [TestMethod]
        public void UpdateProfile_ShouldFail_WhenUserIdIsEmpty()
        {
            // Act
            var result = _service.UpdateProfile(new ProfileUpdateDTO(), "");

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Invalid input.", result.message);
        }

        [TestMethod]
        public void UpdateProfile_ShouldFail_WhenUserNotFound()
        {
            // Arrange
            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(new List<UserTblModel>().AsQueryable()).Object);

            // Act
            var result = _service.UpdateProfile(new ProfileUpdateDTO { firstName = "Test" }, "1");

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("User not found.", result.message);
        }

        [TestMethod]
        public void UpdateProfile_ShouldSucceed_WithoutProfilePicture()
        {
            // Arrange
            var user = new UserTblModel { userID = 1, firstName = "Old", lastName = "Name", specialization = "Spec", phoneNumber = "123" };
            var users = new List<UserTblModel> { user }.AsQueryable();
            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users).Object);
            _mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var dto = new ProfileUpdateDTO
            {
                firstName = "New",
                lastName = "Name",
                specialization = "NewSpec",
                contactNumber = "456",
                file = null
            };

            // Act
            var result = _service.UpdateProfile(dto, "1");

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("New", user.firstName);
            Assert.AreEqual("NewSpec", user.specialization);
            Assert.AreEqual("456", user.phoneNumber);
        }

        [TestMethod]
        public void UpdateProfile_ShouldSucceed_WithProfilePicture()
        {
            // Arrange
            var user = new UserTblModel { userID = 2 };
            var users = new List<UserTblModel> { user }.AsQueryable();
            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users).Object);
            _mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockFile = new Mock<HttpPostedFileBase>();
            mockFile.Setup(f => f.ContentLength).Returns(100);

            _fileStorageService.Setup(s => s.SaveProfilePicture(mockFile.Object))
                .Returns(new FileSaveResult { Success = true, FileName = "profile.png" });

            var dto = new ProfileUpdateDTO
            {
                firstName = "Pic",
                lastName = "User",
                specialization = "Spec",
                contactNumber = "123",
                file = mockFile.Object
            };

            // Act
            var result = _service.UpdateProfile(dto, "2");

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("profile.png", user.profilePictureUrl);
            _fileStorageService.Verify(f => f.SaveProfilePicture(mockFile.Object), Times.Once);
        }

        [TestMethod]
        public void UpdateProfile_ShouldFail_WhenFileUploadFails()
        {
            // Arrange
            var user = new UserTblModel { userID = 3 };
            var users = new List<UserTblModel> { user }.AsQueryable();
            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users).Object);

            var mockFile = new Mock<HttpPostedFileBase>();
            mockFile.Setup(f => f.ContentLength).Returns(100);

            _fileStorageService.Setup(s => s.SaveProfilePicture(mockFile.Object))
                .Returns(new FileSaveResult { Success = false, ErrorMessage = "Upload failed" });

            var dto = new ProfileUpdateDTO { file = mockFile.Object };

            // Act
            var result = _service.UpdateProfile(dto, "3");

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Upload failed", result.message);
        }

        [TestMethod]
        public void UpdateProfile_ShouldFail_WhenSaveChangesThrowsException()
        {
            // Arrange
            var user = new UserTblModel { userID = 4 };
            var users = new List<UserTblModel> { user }.AsQueryable();
            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users).Object);
            _mockContext.Setup(c => c.SaveChanges()).Throws(new Exception("DB error"));

            var dto = new ProfileUpdateDTO { firstName = "Test" };

            // Act
            var result = _service.UpdateProfile(dto, "4");

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("DB error", result.message);
        }

        [TestMethod]
        public void GetUserById_ShouldReturnUserDTO_WhenUserExists()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 1, firstName = "John", lastName = "Doe", email = "john@example.com", profilePictureUrl = "john.png" }
            }.AsQueryable();

            var userRoles = new List<UserRoleTblModel>
            {
                new UserRoleTblModel { userID = 1, roleID = 2 }
            }.AsQueryable();

            var roles = new List<RoleTblModel>
            {
                new RoleTblModel { roleID = 2, roleName = "Admin" }
            }.AsQueryable();

            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users).Object);
            _mockContext.Setup(c => c.user_role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(userRoles).Object);
            _mockContext.Setup(c => c.role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(roles).Object);

            // Act
            var result = _service.GetUserById("1");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.userID);
            Assert.AreEqual("John", result.firstName);
            Assert.AreEqual("Doe", result.lastName);
            Assert.AreEqual("Admin", result.role);
            Assert.AreEqual("john.png", result.profilePictureUrl);
        }

        [TestMethod]
        public void GetUserById_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(new List<UserTblModel>().AsQueryable()).Object);
            _mockContext.Setup(c => c.user_role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(new List<UserRoleTblModel>().AsQueryable()).Object);
            _mockContext.Setup(c => c.role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(new List<RoleTblModel>().AsQueryable()).Object);

            // Act
            var result = _service.GetUserById("99");

            // Assert
            Assert.IsNull(result);
        }
    }
}
