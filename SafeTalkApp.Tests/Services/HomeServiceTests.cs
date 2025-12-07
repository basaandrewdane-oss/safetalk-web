using Moq;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;
using System.Data.Entity;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class HomeServiceTests
    {
        private Mock<ISafeTalkAppContext> _mockDbContext = null!;
        private HomeService _service = null!;
        private Mock<DbSet<FeedbackTblModel>> _mockFeedbackSet = null!;
        private Mock<DbSet<TermsTblModel>> _mockTermsSet = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockDbContext = new Mock<ISafeTalkAppContext>();
            _mockFeedbackSet = new Mock<DbSet<FeedbackTblModel>>();
            _mockDbContext.Setup(c => c.feedback_tbl).Returns(_mockFeedbackSet.Object);
            _service = new HomeService(_mockDbContext.Object);
        }

        [TestMethod]
        public void GetVerifiedDoctors_ShouldReturnEmpty_WhenNoDoctorsExist()
        {
            // Arrange
            var users = new List<UserTblModel>().AsQueryable();
            var userRoles = new List<UserRoleTblModel>().AsQueryable();
            var availabilities = new List<UserAvailabilityTblModel>().AsQueryable();
            var days = new List<DaysOfWeekTblModel>().AsQueryable();

            _mockDbContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users).Object);
            _mockDbContext.Setup(c => c.user_role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(userRoles).Object);
            _mockDbContext.Setup(c => c.user_availability_tbl).Returns(MockDbSetHelper.BuildMockDbSet(availabilities).Object);
            _mockDbContext.Setup(c => c.days_of_week_tbl).Returns(MockDbSetHelper.BuildMockDbSet(days).Object);

            // Act
            var result = _service.GetVerifiedDoctors();

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void GetVerifiedDoctors_ShouldReturnVerifiedDoctors_WhenDataExists()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel
                {
                    userID = 1,
                    firstName = "John",
                    middleName = "A",
                    lastName = "Doe",
                    specialization = "Cardiology",
                    phoneNumber = "123456789",
                    email = "john@example.com",
                    profilePictureUrl = null,
                    isVerified = true,
                    isEmailVerified = true
                }
            }.AsQueryable();

            var userRoles = new List<UserRoleTblModel>
            {
                new UserRoleTblModel { userID = 1, roleID = 2 } // 2 = Doctor
            }.AsQueryable();

            var availabilities = new List<UserAvailabilityTblModel>
            {
                new UserAvailabilityTblModel { userID = 1, dayID = 1, availabilityStart = new TimeSpan(9,0,0), availabilityEnd = new TimeSpan(17,0,0), fee = 500 }
            }.AsQueryable();

            var days = new List<DaysOfWeekTblModel>
            {
                new DaysOfWeekTblModel { dayID = 1, day = "Monday" }
            }.AsQueryable();

            _mockDbContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users).Object);
            _mockDbContext.Setup(c => c.user_role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(userRoles).Object);
            _mockDbContext.Setup(c => c.user_availability_tbl).Returns(MockDbSetHelper.BuildMockDbSet(availabilities).Object);
            _mockDbContext.Setup(c => c.days_of_week_tbl).Returns(MockDbSetHelper.BuildMockDbSet(days).Object);

            // Act
            var result = _service.GetVerifiedDoctors();

            // Assert
            Assert.IsTrue(result.success);
            var doctor = result.data.First();
            Assert.AreEqual("John A Doe", doctor.fullName);
            Assert.AreEqual("Cardiology", doctor.specialization);
            Assert.AreEqual("123456789", doctor.phoneNumber);
            Assert.AreEqual("john@example.com", doctor.email);
            Assert.AreEqual("/Uploads/ProfilePictures/default-avatar.png", doctor.profilePictureUrl);
            Assert.AreEqual(1, doctor.availabilities.Count);
            Assert.AreEqual("Monday", doctor.availabilities.First().day);
            Assert.AreEqual(new TimeSpan(9, 0, 0), doctor.availabilities.First().startTime);
            Assert.AreEqual(new TimeSpan(17, 0, 0), doctor.availabilities.First().endTime);
            Assert.AreEqual(500, doctor.availabilities.First().fee);
        }

        [TestMethod]
        public void GetVerifiedDoctors_ShouldIgnoreUnverifiedDoctors()
        {
            // Arrange
            var users = new List<UserTblModel>
        {
            new UserTblModel
            {
                userID = 1,
                firstName = "John",
                lastName = "Doe",
                isVerified = false,
                isEmailVerified = true
            }
        }.AsQueryable();

            var userRoles = new List<UserRoleTblModel>
        {
            new UserRoleTblModel { userID = 1, roleID = 2 }
        }.AsQueryable();

            _mockDbContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users).Object);
            _mockDbContext.Setup(c => c.user_role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(userRoles).Object);
            _mockDbContext.Setup(c => c.user_availability_tbl).Returns(MockDbSetHelper.BuildMockDbSet(new List<UserAvailabilityTblModel>().AsQueryable()).Object);
            _mockDbContext.Setup(c => c.days_of_week_tbl).Returns(MockDbSetHelper.BuildMockDbSet(new List<DaysOfWeekTblModel>().AsQueryable()).Object);

            // Act
            var result = _service.GetVerifiedDoctors();

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void SubmitFeedback_ShouldAddFeedback_AndReturnSuccess()
        {
            // Arrange
            var feedbackDto = new FeedbackDTO
            {
                email = "user@example.com",
                feedback = "Great service!"
            };

            FeedbackTblModel capturedFeedback = null!;
            _mockFeedbackSet.Setup(f => f.Add(It.IsAny<FeedbackTblModel>()))
                            .Callback<FeedbackTblModel>(f => capturedFeedback = f);

            _mockDbContext.Setup(d => d.SaveChanges()).Returns(1);

            // Act
            var result = _service.SubmitFeedback(feedbackDto);

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsTrue(result.data);
            Assert.AreEqual("Feedback submitted successfully.", result.message);

            Assert.IsNotNull(capturedFeedback);
            Assert.AreEqual(feedbackDto.email, capturedFeedback.email);
            Assert.AreEqual(feedbackDto.feedback, capturedFeedback.feedback);
            Assert.IsTrue(capturedFeedback.dateCreated <= DateTime.Now);
        }

        [TestMethod]
        public void SubmitFeedback_ShouldReturnFail_WhenExceptionOccurs()
        {
            // Arrange
            var feedbackDto = new FeedbackDTO
            {
                email = "user@example.com",
                feedback = "Great service!"
            };

            _mockFeedbackSet.Setup(f => f.Add(It.IsAny<FeedbackTblModel>()))
                            .Throws(new Exception("DB error"));

            // Act
            var result = _service.SubmitFeedback(feedbackDto);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred");
        }

        [TestMethod]
        public void GetTerms_ShouldReturnLatestTerms_WhenTermsExist()
        {
            // Arrange
            var termsData = new List<TermsTblModel>
            {
                new TermsTblModel { content = "Old terms", dateUpdated = new DateTime(2023, 1, 1) },
                new TermsTblModel { content = "Latest terms", dateUpdated = new DateTime(2025, 11, 14) }
            }.AsQueryable();

            _mockTermsSet = MockDbSetHelper.BuildMockDbSet(termsData);
            _mockDbContext.Setup(c => c.terms_tbl).Returns(_mockTermsSet.Object);

            // Act
            var result = _service.GetTerms();

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);
            Assert.AreEqual("Latest terms", result.data.content);
            Assert.AreEqual(new DateTime(2025, 11, 14), result.data.dateUpdated);
            Assert.AreEqual("Terms retrieved successfully.", result.message);
        }

        [TestMethod]
        public void GetTerms_ShouldReturnNull_WhenNoTermsExist()
        {
            // Arrange
            var emptyData = new List<TermsTblModel>().AsQueryable();
            _mockTermsSet = MockDbSetHelper.BuildMockDbSet(emptyData);
            _mockDbContext.Setup(c => c.terms_tbl).Returns(_mockTermsSet.Object);

            // Act
            var result = _service.GetTerms();

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNull(result.data);
            Assert.AreEqual("Terms retrieved successfully.", result.message);
        }

        [TestMethod]
        public void GetTerms_ShouldReturnFail_WhenExceptionOccurs()
        {
            // Arrange
            // Create a normal (empty) mock DbSet using the helper
            var mockSet = MockDbSetHelper.BuildMockDbSet(new List<TermsTblModel>().AsQueryable());

            // Throw on Provider access
            mockSet.As<IQueryable<TermsTblModel>>()
                   .Setup(m => m.Provider)
                   .Throws(new Exception("DB error"));

            _mockDbContext.Setup(c => c.terms_tbl).Returns(mockSet.Object);

            var result = _service.GetTerms();

            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred");
        }
    }
}
