using Moq;
using SafeTalkApp.DTOs.Account;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class AvailabilityServiceTests
    {
        private Mock<ISafeTalkAppContext> _mockDbContext = null!;
        private AvailabilityService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockDbContext = new Mock<ISafeTalkAppContext>();
            _service = new AvailabilityService(_mockDbContext.Object);
        }

        [TestMethod]
        public void GetAvailability_ShouldReturnAvailabilities_WhenFound()
        {
            // Arrange
            var availabilities = new List<UserAvailabilityTblModel>
            {
                new UserAvailabilityTblModel
                {
                    availabilityID = 1,
                    userID = 10,
                    dayID = 2,
                    availabilityStart = new TimeSpan(9, 0, 0),
                    availabilityEnd = new TimeSpan(17, 0, 0),
                    slotDuration = 30,
                    fee = 500
                }
            }.AsQueryable();

            var days = new List<DaysOfWeekTblModel>
            {
                new DaysOfWeekTblModel { dayID = 2, day = "Tuesday" }
            }.AsQueryable();

            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 10, firstName = "Jane", lastName = "Doe" }
            }.AsQueryable();

            var mockAvailSet = MockDbSetHelper.BuildMockDbSet(availabilities);
            var mockDaysSet = MockDbSetHelper.BuildMockDbSet(days);
            var mockUserSet = MockDbSetHelper.BuildMockDbSet(users);

            _mockDbContext.Setup(db => db.user_availability_tbl).Returns(mockAvailSet.Object);
            _mockDbContext.Setup(db => db.days_of_week_tbl).Returns(mockDaysSet.Object);
            _mockDbContext.Setup(db => db.user_tbl).Returns(mockUserSet.Object);

            // Act
            var result = _service.GetAvailability(10);

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);
            var availabilityList = result.data.ToList();
            Assert.AreEqual(1, availabilityList.Count);
            Assert.AreEqual("Tuesday", availabilityList[0].day);
            Assert.AreEqual("09:00", availabilityList[0].availabilityStart);
            Assert.AreEqual("17:00", availabilityList[0].availabilityEnd);
            Assert.AreEqual(500, availabilityList[0].fee);
        }

        [TestMethod]
        public void GetAvailability_ShouldReturnEmptyList_WhenNoRecordsFound()
        {
            // Arrange
            var availabilities = new List<UserAvailabilityTblModel>().AsQueryable();
            var days = new List<DaysOfWeekTblModel>().AsQueryable();
            var users = new List<UserTblModel>().AsQueryable();

            var mockAvailSet = MockDbSetHelper.BuildMockDbSet(availabilities);
            var mockDaysSet = MockDbSetHelper.BuildMockDbSet(days);
            var mockUserSet = MockDbSetHelper.BuildMockDbSet(users);

            _mockDbContext.Setup(db => db.user_availability_tbl).Returns(mockAvailSet.Object);
            _mockDbContext.Setup(db => db.days_of_week_tbl).Returns(mockDaysSet.Object);
            _mockDbContext.Setup(db => db.user_tbl).Returns(mockUserSet.Object);

            // Act
            var result = _service.GetAvailability(999); // user with no data

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void GetAvailability_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange
            _mockDbContext.Setup(db => db.user_availability_tbl)
                          .Throws(new Exception("DB error"));

            // Act
            var result = _service.GetAvailability(10);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred while retrieving availability");
        }

        [TestMethod]
        public void SaveAvailability_ShouldUpdateExistingRecords_WhenMatchFound()
        {
            // Arrange
            var existingAvailabilities = new List<UserAvailabilityTblModel>
            {
                new UserAvailabilityTblModel
                {
                    availabilityID = 1,
                    userID = 10,
                    dayID = 1,
                    availabilityStart = new TimeSpan(9, 0, 0),
                    availabilityEnd = new TimeSpan(17, 0, 0),
                    slotDuration = 30,
                    fee = 500
                }
            }.AsQueryable();

            var mockAvailSet = MockDbSetHelper.BuildMockDbSet(existingAvailabilities);
            _mockDbContext.Setup(db => db.user_availability_tbl).Returns(mockAvailSet.Object);

            var dtoList = new List<AvailabilityDTO>
            {
                new AvailabilityDTO
                {
                    dayID = 1,
                    availabilityStart = "10:00",
                    availabilityEnd = "16:00",
                    slotDuration = 45,
                    fee = 600
                }
            };

            // Act
            var result = _service.SaveAvailability(10, dtoList);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Availability and slot duration saved successfully.", result.message);
            var updated = existingAvailabilities.First();
            Assert.AreEqual(new TimeSpan(10, 0, 0), updated.availabilityStart);
            Assert.AreEqual(new TimeSpan(16, 0, 0), updated.availabilityEnd);
            Assert.AreEqual(45, updated.slotDuration);
            Assert.AreEqual(600, updated.fee);
            _mockDbContext.Verify(db => db.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void SaveAvailability_ShouldInsertNewRecord_WhenNotExisting()
        {
            // Arrange
            var existingAvailabilities = new List<UserAvailabilityTblModel>().AsQueryable();
            var mockAvailSet = MockDbSetHelper.BuildMockDbSet(existingAvailabilities);
            _mockDbContext.Setup(db => db.user_availability_tbl).Returns(mockAvailSet.Object);

            var dtoList = new List<AvailabilityDTO>
        {
            new AvailabilityDTO
            {
                dayID = 2,
                availabilityStart = "08:00",
                availabilityEnd = "12:00",
                slotDuration = 30,
                fee = 700
            }
        };

            // Act
            var result = _service.SaveAvailability(10, dtoList);

            // Assert
            Assert.IsTrue(result.success);
            _mockDbContext.Verify(db => db.user_availability_tbl.Add(It.IsAny<UserAvailabilityTblModel>()), Times.Once);
            _mockDbContext.Verify(db => db.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void SaveAvailability_ShouldRemoveOldRecord_WhenNotInNewList()
        {
            // Arrange
            var existingAvailabilities = new List<UserAvailabilityTblModel>
            {
                new UserAvailabilityTblModel { availabilityID = 1, userID = 10, dayID = 1 },
                new UserAvailabilityTblModel { availabilityID = 2, userID = 10, dayID = 2 }
            }.AsQueryable();

            var mockAvailSet = MockDbSetHelper.BuildMockDbSet(existingAvailabilities);
            _mockDbContext.Setup(db => db.user_availability_tbl).Returns(mockAvailSet.Object);

            var dtoList = new List<AvailabilityDTO>
            {
                new AvailabilityDTO
                {
                    dayID = 1,
                    availabilityStart = "09:00",
                    availabilityEnd = "17:00",
                    slotDuration = 30,
                    fee = 500
                }
            };

            // Act
            var result = _service.SaveAvailability(10, dtoList);

            // Assert
            Assert.IsTrue(result.success);
            _mockDbContext.Verify(db => db.user_availability_tbl.RemoveRange(It.IsAny<IEnumerable<UserAvailabilityTblModel>>()), Times.Once);
            _mockDbContext.Verify(db => db.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void SaveAvailability_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange
            _mockDbContext.Setup(db => db.user_availability_tbl)
                          .Throws(new Exception("Database error"));

            var dtoList = new List<AvailabilityDTO>
            {
                new AvailabilityDTO
                {
                    dayID = 1,
                    availabilityStart = "09:00",
                    availabilityEnd = "17:00",
                    slotDuration = 30,
                    fee = 500
                }
            };

            // Act
            var result = _service.SaveAvailability(10, dtoList);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "Error saving availability");
        }
    }
}
