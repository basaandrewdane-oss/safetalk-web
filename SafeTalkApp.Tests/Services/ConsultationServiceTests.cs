using Moq;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class ConsultationServiceTests
    {
        private Mock<ISafeTalkAppContext> _mockContext = null!;
        private Mock<IEmailService> _mockEmail = null!;
        private ConsultationService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<ISafeTalkAppContext>();
            _mockEmail = new Mock<IEmailService>();

            _service = new ConsultationService(_mockContext.Object);
        }

        [TestMethod]
        public void GetAppointment_ShouldReturnAppointment_WhenFound()
        {
            // Arrange
            var sampleData = new List<AppointmentsTblModel>
        {
            new AppointmentsTblModel
            {
                appointmentID = 1,
                doctorID = 10,
                patientID = 20,
                date = new DateTime(2025, 10, 25),
                startTime = new TimeSpan(9, 0, 0),
                endTime = new TimeSpan(10, 0, 0)
            }
        }.AsQueryable();

            var mockSet = MockDbSetHelper.BuildMockDbSet(sampleData);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockSet.Object);

            // Act
            var result = _service.GetAppointment(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);
            Assert.AreEqual(1, result.data.appointmentID);
            Assert.AreEqual(10, result.data.doctorID);
            Assert.AreEqual(20, result.data.patientID);
        }

        [TestMethod]
        public void GetAppointment_ShouldReturnFail_WhenNotFound()
        {
            // Arrange
            var sampleData = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel { appointmentID = 99, doctorID = 1, patientID = 2 }
            }.AsQueryable();

            var mockSet = MockDbSetHelper.BuildMockDbSet(sampleData);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockSet.Object);

            // Act
            var result = _service.GetAppointment(123); // Non-existent ID

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Appointment not found", result.message);
            Assert.IsNull(result.data);
        }

        [TestMethod]
        public void GetAppointment_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange
            _mockContext.Setup(c => c.appointments_tbl)
                .Throws(new Exception("DB error"));

            // Act
            var result = _service.GetAppointment(1);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred while retrieving the appointment");
        }

        [TestMethod]
        public void GetChatMessages_ShouldReturnMessages_WhenFound()
        {
            // Arrange
            int appointmentID = 1;
            var chatData = new List<ChatMessageTblModel>
            {
                new ChatMessageTblModel
                {
                    messageID = 1,
                    appointmentID = 1,
                    senderID = 100,
                    message = "Hello",
                    sentAt = new DateTime(2025, 10, 24, 9, 0, 0)
                },
                new ChatMessageTblModel
                {
                    messageID = 2,
                    appointmentID = 1,
                    senderID = 200,
                    message = "Hi there!",
                    sentAt = new DateTime(2025, 10, 24, 9, 1, 0)
                },
                new ChatMessageTblModel
                {
                    messageID = 3,
                    appointmentID = 2, // different appointment
                    senderID = 300,
                    message = "Not included",
                    sentAt = new DateTime(2025, 10, 24, 9, 2, 0)
                }
            }.AsQueryable();

            var mockSet = MockDbSetHelper.BuildMockDbSet(chatData);
            _mockContext.Setup(c => c.chat_message_tbl).Returns(mockSet.Object);

            // Act
            var result = _service.GetChatMessages(appointmentID);

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);
            var messages = result.data.ToList();
            Assert.AreEqual(2, messages.Count);
            Assert.AreEqual("Hello", messages[0].message);
            Assert.AreEqual("Hi there!", messages[1].message);
            Assert.IsTrue(messages[0].sentAt < messages[1].sentAt); // check ordering
        }

        [TestMethod]
        public void GetChatMessages_ShouldReturnEmptyList_WhenNoMessagesFound()
        {
            // Arrange
            var chatData = new List<ChatMessageTblModel>
            {
                new ChatMessageTblModel { appointmentID = 999, message = "Other chat" }
            }.AsQueryable();

            var mockSet = MockDbSetHelper.BuildMockDbSet(chatData);
            _mockContext.Setup(c => c.chat_message_tbl).Returns(mockSet.Object);

            // Act
            var result = _service.GetChatMessages(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void GetChatMessages_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange
            _mockContext.Setup(c => c.chat_message_tbl)
                .Throws(new Exception("DB error"));

            // Act
            var result = _service.GetChatMessages(1);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred while retrieving chat messages");
        }

        [TestMethod]
        public void GetPatientConsultations_ShouldReturnConsultations_WhenFound()
        {
            // Arrange
            int patientID = 1;

            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel
                {
                    appointmentID = 10,
                    doctorID = 100,
                    patientID = patientID,
                    date = new DateTime(2025, 10, 25),
                    startTime = new TimeSpan(10, 0, 0),
                    endTime = new TimeSpan(11, 0, 0),
                    status = AppointmentStatus.Completed,
                    transcriptFilePath = "transcript1.pdf"
                },
                new AppointmentsTblModel
                {
                    appointmentID = 20,
                    doctorID = 101,
                    patientID = patientID,
                    date = new DateTime(2025, 10, 24),
                    startTime = new TimeSpan(9, 0, 0),
                    endTime = new TimeSpan(9, 30, 0),
                    status = AppointmentStatus.Completed,
                    transcriptFilePath = "transcript2.pdf"
                }
            }.AsQueryable();

            var doctors = new List<UserTblModel>
            {
                new UserTblModel { userID = 100, firstName = "Jane", lastName = "Doe", email = "jane@clinic.com" },
                new UserTblModel { userID = 101, firstName = "John", lastName = "Smith", email = "john@clinic.com" }
            }.AsQueryable();

            var payments = new List<PaymentTblModel>
            {
                new PaymentTblModel { paymentID = 1, appointmentID = 10, status = PaymentStatus.Completed },
                new PaymentTblModel { paymentID = 2, appointmentID = 20, status = PaymentStatus.Completed },
                new PaymentTblModel { paymentID = 3, appointmentID = 99, status = PaymentStatus.Pending }
            }.AsQueryable();

            var referrals = new List<ReferralTblModel>
            {
                new ReferralTblModel { referralID = 5, appointmentID = 10 },
            }.AsQueryable();

            // Mock DbSets
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(appointments);
            var mockUsers = MockDbSetHelper.BuildMockDbSet(doctors);
            var mockPayments = MockDbSetHelper.BuildMockDbSet(payments);
            var mockReferrals = MockDbSetHelper.BuildMockDbSet(referrals);

            // Setup context
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);
            _mockContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);
            _mockContext.Setup(c => c.referrals_tbl).Returns(mockReferrals.Object);

            // Act
            var result = _service.GetPatientConsultations(patientID);

            // Assert
            Assert.IsTrue(result.success);
            var consultations = result.data.ToList();
            Assert.AreEqual(2, consultations.Count);

            // Ordered descending by date/startTime
            Assert.AreEqual(10, consultations[0].appointmentID);
            Assert.AreEqual("Jane Doe", consultations[0].doctorName);
            Assert.IsTrue(consultations[0].hasReferral);
            Assert.AreEqual(5, consultations[0].referralID);
        }

        [TestMethod]
        public void GetPatientConsultations_ShouldReturnEmpty_WhenNoCompletedPayments()
        {
            // Arrange
            int patientID = 2;

            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel { appointmentID = 1, doctorID = 100, patientID = patientID }
            }.AsQueryable();

            var doctors = new List<UserTblModel>
            {
                new UserTblModel { userID = 100, firstName = "Jane", lastName = "Doe", email = "jane@clinic.com" }
            }.AsQueryable();

            var payments = new List<PaymentTblModel>
            {
                new PaymentTblModel { paymentID = 1, appointmentID = 1, status = PaymentStatus.Pending }
            }.AsQueryable();

            var referrals = new List<ReferralTblModel>().AsQueryable();

            // Mock DbSets
            _mockContext.Setup(c => c.appointments_tbl).Returns(MockDbSetHelper.BuildMockDbSet(appointments).Object);
            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(doctors).Object);
            _mockContext.Setup(c => c.payment_tbl).Returns(MockDbSetHelper.BuildMockDbSet(payments).Object);
            _mockContext.Setup(c => c.referrals_tbl).Returns(MockDbSetHelper.BuildMockDbSet(referrals).Object);

            // Act
            var result = _service.GetPatientConsultations(patientID);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void GetPatientConsultations_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange
            _mockContext.Setup(c => c.appointments_tbl)
                .Throws(new Exception("DB Error"));

            // Act
            var result = _service.GetPatientConsultations(1);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred while retrieving consultations");
        }
    }
}
