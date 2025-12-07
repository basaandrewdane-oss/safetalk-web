using Moq;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Interfaces;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;
using System.Data.Entity;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class TranscriptionServiceTests
    {
        private Mock<ISafeTalkAppContext> _mockContext = null!;
        private Mock<IEmailService> _mockEmailService = null!;
        private Mock<IFileStorageService> _mockFileStorageService = null!;
        private TranscriptionService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<ISafeTalkAppContext>();
            _mockEmailService = new Mock<IEmailService>();
            _mockFileStorageService = new Mock<IFileStorageService>();
            _service = new TranscriptionService(_mockContext.Object, _mockEmailService.Object, _mockFileStorageService.Object);
        }

        [TestMethod]
        public async Task DownloadTranscriptFile_ReturnsFail_WhenAppointmentNotFound()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>();
            var mockSet = MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockSet.Object);

            // Act
            var result = await _service.DownloadTranscriptFile(1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Transcript not found.", result.message);
        }

        [TestMethod]
        public async Task DownloadTranscriptFile_ReturnsFail_WhenTranscriptPathEmpty()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel { appointmentID = 1, transcriptFilePath = null }
            };
            var mockSet = MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockSet.Object);

            // Act
            var result = await _service.DownloadTranscriptFile(1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Transcript not found.", result.message);
        }

        [TestMethod]
        public async Task DownloadTranscriptFile_ReturnsFail_WhenFileMissing()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel { appointmentID = 1, transcriptFilePath = "file.txt" }
            };
            var mockSet = MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockSet.Object);

            _mockFileStorageService.Setup(f => f.MapPath(It.IsAny<string>())).Returns("fullpath.txt");
            _mockFileStorageService.Setup(f => f.FileExists("fullpath.txt")).Returns(false);

            // Act
            var result = await _service.DownloadTranscriptFile(1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Transcript file missing on server.", result.message);
        }

        [TestMethod]
        public async Task DownloadTranscriptFile_ReturnsFail_WhenHashMismatch()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel { appointmentID = 1, transcriptFilePath = "file.txt", transcriptHash = "hash123" }
            };
            var mockSet = MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockSet.Object);

            _mockFileStorageService.Setup(f => f.MapPath(It.IsAny<string>())).Returns("fullpath.txt");
            _mockFileStorageService.Setup(f => f.FileExists("fullpath.txt")).Returns(true);
            _mockFileStorageService.Setup(f => f.ReadAllText("fullpath.txt")).Returns("content");

            // Act
            var result = await _service.DownloadTranscriptFile(1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Transcript integrity check failed.", result.message);
        }

        [TestMethod]
        public async Task DownloadTranscriptFile_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var content = "file content";
            var hash = TranscriptionService.ComputeStringHash(content);

            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel { appointmentID = 1, transcriptFilePath = "file.txt", transcriptHash = hash }
            };
            var mockSet = MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockSet.Object);

            _mockFileStorageService.Setup(f => f.MapPath(It.IsAny<string>())).Returns("fullpath.txt");
            _mockFileStorageService.Setup(f => f.FileExists("fullpath.txt")).Returns(true);
            _mockFileStorageService.Setup(f => f.ReadAllText("fullpath.txt")).Returns(content);
            _mockFileStorageService.Setup(f => f.ReadAllBytesAsync("fullpath.txt"))
                .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes(content));

            // Act
            var result = await _service.DownloadTranscriptFile(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Transcript download ready.", result.message);
            CollectionAssert.AreEqual(System.Text.Encoding.UTF8.GetBytes(content), result.data);
        }

        [TestMethod]
        public async Task DownloadTranscriptFile_ReturnsFail_WhenExceptionThrown()
        {
            // Arrange
            _mockContext.Setup(c => c.appointments_tbl).Throws(new Exception("DB error"));

            // Act
            var result = await _service.DownloadTranscriptFile(1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsTrue(result.message.Contains("Error during transcript download"));
        }

        [TestMethod]
        public void SaveTextTranscript_ReturnsFail_WhenTranscriptIsNullOrEmpty()
        {
            // Arrange
            var modelNull = new TextTranscriptDTO { appointmentId = 1, transcript = null };
            var modelEmpty = new TextTranscriptDTO { appointmentId = 1, transcript = " " };

            // Act
            var resultNull = _service.SaveTextTranscript(modelNull);
            var resultEmpty = _service.SaveTextTranscript(modelEmpty);

            // Assert
            Assert.IsFalse(resultNull.success);
            Assert.AreEqual("Transcript is empty.", resultNull.message);

            Assert.IsFalse(resultEmpty.success);
            Assert.AreEqual("Transcript is empty.", resultEmpty.message);
        }

        [TestMethod]
        public void SaveTextTranscript_ReturnsFail_WhenAppointmentNotFound()
        {
            // Arrange
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(new List<AppointmentsTblModel>().AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);

            var model = new TextTranscriptDTO { appointmentId = 1, transcript = "Test transcript" };

            _mockFileStorageService.Setup(f => f.MapPath(It.IsAny<string>())).Returns("C:/Uploads/Transcripts");
            _mockFileStorageService.Setup(f => f.CreateDirectory(It.IsAny<string>()));
            _mockFileStorageService.Setup(f => f.CombinePath(It.IsAny<string>(), It.IsAny<string>())).Returns("C:/Uploads/Transcripts/file.txt");
            _mockFileStorageService.Setup(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var result = _service.SaveTextTranscript(model);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Appointment not found.", result.message);
        }

        [TestMethod]
        public void SaveTextTranscript_SavesFileAndUpdatesDb_WhenAppointmentExists()
        {
            // Arrange
            var appointment = new AppointmentsTblModel { appointmentID = 1, patientID = 10, doctorID = 20 };
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(new[] { appointment }.AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);

            var patient = new UserTblModel { userID = 10 };
            var doctor = new UserTblModel { userID = 20 };
            var mockUsers = MockDbSetHelper.BuildMockDbSet(new[] { patient, doctor }.AsQueryable());
            _mockContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);

            var model = new TextTranscriptDTO { appointmentId = 1, transcript = "Test transcript" };

            _mockFileStorageService.Setup(f => f.MapPath(It.IsAny<string>())).Returns("C:/Uploads/Transcripts");
            _mockFileStorageService.Setup(f => f.CreateDirectory(It.IsAny<string>()));
            _mockFileStorageService.Setup(f => f.CombinePath(It.IsAny<string>(), It.IsAny<string>())).Returns("C:/Uploads/Transcripts/file.txt");
            _mockFileStorageService.Setup(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var result = _service.SaveTextTranscript(model);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.success);
            Assert.AreEqual("Transcript saved.", result.data); // <-- use .data, not .message
            Assert.IsNotNull(appointment.transcriptFilePath);
            Assert.IsNotNull(appointment.transcriptHash);

            _mockFileStorageService.Verify(f => f.CreateDirectory(It.IsAny<string>()), Times.Once);
            _mockFileStorageService.Verify(f => f.WriteAllText("C:/Uploads/Transcripts/file.txt", "Test transcript"), Times.Once);
        }


        [TestMethod]
        public void SaveTextTranscript_Continues_WhenEmailSendingFails()
        {
            // Arrange

            // 1️⃣ Mock appointment
            var appointment = new AppointmentsTblModel
            {
                appointmentID = 1,
                patientID = 10,
                doctorID = 20
            };
            var appointments = new List<AppointmentsTblModel> { appointment }.AsQueryable();

            var mockAppointments = new Mock<DbSet<AppointmentsTblModel>>();
            mockAppointments.As<IQueryable<AppointmentsTblModel>>().Setup(m => m.Provider).Returns(appointments.Provider);
            mockAppointments.As<IQueryable<AppointmentsTblModel>>().Setup(m => m.Expression).Returns(appointments.Expression);
            mockAppointments.As<IQueryable<AppointmentsTblModel>>().Setup(m => m.ElementType).Returns(appointments.ElementType);
            mockAppointments.As<IQueryable<AppointmentsTblModel>>().Setup(m => m.GetEnumerator()).Returns(() => appointments.GetEnumerator());

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);
            _mockContext.Setup(c => c.SaveChanges()).Returns(1);

            // 2️⃣ Mock users
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 10 },
                new UserTblModel { userID = 20 }
            }.AsQueryable();

            var mockUsers = new Mock<DbSet<UserTblModel>>();
            mockUsers.As<IQueryable<UserTblModel>>().Setup(m => m.Provider).Returns(users.Provider);
            mockUsers.As<IQueryable<UserTblModel>>().Setup(m => m.Expression).Returns(users.Expression);
            mockUsers.As<IQueryable<UserTblModel>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockUsers.As<IQueryable<UserTblModel>>().Setup(m => m.GetEnumerator()).Returns(() => users.GetEnumerator());

            _mockContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);

            // 3️⃣ Mock FileStorageService
            _mockFileStorageService.Setup(f => f.MapPath(It.IsAny<string>())).Returns("C:/Uploads/Transcripts");
            _mockFileStorageService.Setup(f => f.CreateDirectory(It.IsAny<string>()));
            _mockFileStorageService.Setup(f => f.CombinePath(It.IsAny<string>(), It.IsAny<string>())).Returns("C:/Uploads/Transcripts/file.txt");
            _mockFileStorageService.Setup(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));

            // 4️⃣ Setup email service to throw (simulate failure)
            _mockEmailService
                .Setup(e => e.SendTranscriptionReadyToPatient(It.IsAny<UserTblModel>(), It.IsAny<UserTblModel>(), It.IsAny<AppointmentsTblModel>(), It.IsAny<string>()))
                .Throws(new Exception("Email error"));
            _mockEmailService
                .Setup(e => e.SendTranscriptionReadyToDoctor(It.IsAny<UserTblModel>(), It.IsAny<UserTblModel>(), It.IsAny<AppointmentsTblModel>(), It.IsAny<string>()))
                .Throws(new Exception("Email error"));

            // 5️⃣ Prepare transcript model
            var model = new TextTranscriptDTO
            {
                appointmentId = 1,
                transcript = "Test transcript"
            };

            // Act
            var result = _service.SaveTextTranscript(model);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsTrue(result.success, "Result should be success even if emails fail.");

            // ✅ The string is returned in 'data' because ApiResponse<string>.Ok sets 'data'
            Assert.AreEqual("Transcript saved.", result.data);

            // Verify that SaveChanges was called
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);

            // Verify that WriteAllText was called
            _mockFileStorageService.Verify(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void SaveTextTranscript_ReturnsFail_WhenExceptionThrown()
        {
            // Arrange
            var model = new TextTranscriptDTO { appointmentId = 1, transcript = "Valid transcript" };
            _mockContext.Setup(c => c.appointments_tbl).Throws(new Exception("DB error"));

            // Act
            var result = _service.SaveTextTranscript(model);

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsTrue(result.message.Contains("Error saving transcript"));
        }
    }
}
