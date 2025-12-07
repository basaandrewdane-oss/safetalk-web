using Moq;
using SafeTalkApp.DTOs.Consultation;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;
using System.Data.Entity;

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

            _service = new ConsultationService(_mockContext.Object, _mockEmail.Object);
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
            int currentUserId = 100;
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

            var userData = new List<UserTblModel>
            {
                new UserTblModel { userID = 100, firstName = "John", lastName = "Doe" },
                new UserTblModel { userID = 200, firstName = "Jane", lastName = "Smith" },
                new UserTblModel { userID = 300, firstName = "Mark", lastName = "Brown" }
            }.AsQueryable();

            var mockChatSet = MockDbSetHelper.BuildMockDbSet(chatData);
            var mockUserSet = MockDbSetHelper.BuildMockDbSet(userData);

            _mockContext.Setup(c => c.chat_message_tbl).Returns(mockChatSet.Object);
            _mockContext.Setup(c => c.user_tbl).Returns(mockUserSet.Object);

            // Act
            var result = _service.GetChatMessages(appointmentID, currentUserId);

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
            int appointmentID = 1;
            int currentUserId = 100;
            var chatData = new List<ChatMessageTblModel>
            {
                new ChatMessageTblModel { appointmentID = 999, message = "Other chat" }
            }.AsQueryable();

            var mockSet = MockDbSetHelper.BuildMockDbSet(chatData);
            _mockContext.Setup(c => c.chat_message_tbl).Returns(mockSet.Object);

            var userData = new List<UserTblModel>
            {
                new UserTblModel { userID = 1, firstName = "John", lastName = "Doe" }
            }.AsQueryable();

            var mockUserSet = MockDbSetHelper.BuildMockDbSet(userData);
            _mockContext.Setup(c => c.user_tbl).Returns(mockUserSet.Object);

            // Act
            var result = _service.GetChatMessages(appointmentID, currentUserId);

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void GetChatMessages_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange
            int appointmentID = 1;
            int currentUserId = 100;
            _mockContext.Setup(c => c.chat_message_tbl)
                .Throws(new Exception("DB error"));

            // Act
            var result = _service.GetChatMessages(appointmentID, currentUserId);

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

        [TestMethod]
        public void GetDoctorConsultations_ShouldReturnConsultations_WhenFound()
        {
            // Arrange
            int doctorID = 100;

            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel
                {
                    appointmentID = 10,
                    doctorID = doctorID,
                    patientID = 200,
                    date = new DateTime(2025, 10, 25),
                    startTime = new TimeSpan(10, 0, 0),
                    endTime = new TimeSpan(11, 0, 0),
                    status = AppointmentStatus.Completed,
                    transcriptFilePath = "transcript1.pdf"
                },
                new AppointmentsTblModel
                {
                    appointmentID = 20,
                    doctorID = doctorID,
                    patientID = 201,
                    date = new DateTime(2025, 10, 24),
                    startTime = new TimeSpan(9, 0, 0),
                    endTime = new TimeSpan(9, 30, 0),
                    status = AppointmentStatus.Completed,
                    transcriptFilePath = "transcript2.pdf"
                },
                // Should be excluded (different doctor)
                new AppointmentsTblModel
                {
                    appointmentID = 99,
                    doctorID = 300,
                    patientID = 999,
                    date = DateTime.Now,
                    startTime = new TimeSpan(8, 0, 0),
                    endTime = new TimeSpan(8, 30, 0)
                }
            }.AsQueryable();

            var patients = new List<UserTblModel>
            {
                new UserTblModel { userID = 200, firstName = "Alice", lastName = "Brown", email = "alice@mail.com" },
                new UserTblModel { userID = 201, firstName = "Bob", lastName = "Green", email = "bob@mail.com" }
            }.AsQueryable();

            var payments = new List<PaymentTblModel>
            {
                new PaymentTblModel { paymentID = 1, appointmentID = 10, status = PaymentStatus.Completed },
                new PaymentTblModel { paymentID = 2, appointmentID = 20, status = PaymentStatus.Completed },
                new PaymentTblModel { paymentID = 3, appointmentID = 99, status = PaymentStatus.Pending }
            }.AsQueryable();

            var referrals = new List<ReferralTblModel>
            {
                new ReferralTblModel { referralID = 5, appointmentID = 10 }
            }.AsQueryable();

            // Mock DbSets
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(appointments);
            var mockUsers = MockDbSetHelper.BuildMockDbSet(patients);
            var mockPayments = MockDbSetHelper.BuildMockDbSet(payments);
            var mockReferrals = MockDbSetHelper.BuildMockDbSet(referrals);

            // Setup context
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);
            _mockContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);
            _mockContext.Setup(c => c.referrals_tbl).Returns(mockReferrals.Object);

            // Act
            var result = _service.GetDoctorConsultations(doctorID);

            // Assert
            Assert.IsTrue(result.success);
            var consultations = result.data.ToList();

            // Should contain 2 consultations (appointmentID 10 & 20)
            Assert.AreEqual(2, consultations.Count);

            // Check correct order (most recent first)
            Assert.AreEqual(10, consultations[0].appointmentID);

            // Validate joined patient info
            Assert.AreEqual("Alice Brown", consultations[0].patientName);
            Assert.AreEqual("alice@mail.com", consultations[0].patientEmail);

            // Validate referral check
            Assert.IsTrue(consultations[0].hasReferral);
            Assert.IsFalse(consultations[1].hasReferral);
        }

        [TestMethod]
        public void GetDoctorConsultations_ShouldReturnEmpty_WhenNoCompletedPayments()
        {
            // Arrange
            int doctorID = 101;

            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel { appointmentID = 1, doctorID = doctorID, patientID = 200 }
            }.AsQueryable();

            var patients = new List<UserTblModel>
            {
                new UserTblModel { userID = 200, firstName = "Alice", lastName = "Brown", email = "alice@mail.com" }
            }.AsQueryable();

            var payments = new List<PaymentTblModel>
            {
                new PaymentTblModel { paymentID = 1, appointmentID = 1, status = PaymentStatus.Pending }
            }.AsQueryable();

            var referrals = new List<ReferralTblModel>().AsQueryable();

            _mockContext.Setup(c => c.appointments_tbl).Returns(MockDbSetHelper.BuildMockDbSet(appointments).Object);
            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(patients).Object);
            _mockContext.Setup(c => c.payment_tbl).Returns(MockDbSetHelper.BuildMockDbSet(payments).Object);
            _mockContext.Setup(c => c.referrals_tbl).Returns(MockDbSetHelper.BuildMockDbSet(referrals).Object);

            // Act
            var result = _service.GetDoctorConsultations(doctorID);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void GetDoctorConsultations_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange
            _mockContext.Setup(c => c.appointments_tbl)
                .Throws(new Exception("DB error"));

            // Act
            var result = _service.GetDoctorConsultations(100);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred while retrieving consultations");
        }

        [TestMethod]
        public void CreateReferral_ShouldCreateAndSendEmail_WhenValidData()
        {
            // Arrange
            var doctor = new UserTblModel { userID = 1, email = "doctor@test.com" };
            var patient = new UserTblModel { userID = 2, email = "patient@test.com" };

            var referrals = new List<ReferralTblModel>().AsQueryable();
            var users = new List<UserTblModel> { doctor, patient }.AsQueryable();

            var mockReferralSet = MockDbSetHelper.BuildMockDbSet(referrals);
            var mockUserSet = MockDbSetHelper.BuildMockDbSet(users, find: args =>
            {
                int id = (int)args[0];
                return users.FirstOrDefault(u => u.userID == id);
            });

            _mockContext.Setup(db => db.referrals_tbl).Returns(mockReferralSet.Object);
            _mockContext.Setup(db => db.user_tbl).Returns(mockUserSet.Object);

            var model = new ReferralDTO
            {
                appointmentID = 10,
                doctorID = 1,
                patientID = 2,
                reason = "Needs further tests",
                notes = "Urgent checkup required",
                urgencyLevel = UrgencyLevel.High,
                status = 1,
                sentTo = "Specialist"
            };

            // Act
            var result = _service.CreateReferral(model);

            // Assert
            Assert.IsTrue(result.success);
            _mockContext.Verify(db => db.SaveChanges(), Times.Once);
            _mockEmail.Verify(es => es.SendReferralCreatedEmail(
                patient, doctor, It.IsAny<ReferralTblModel>()), Times.Once);
        }

        [TestMethod]
        public void CreateReferral_ShouldSkipEmail_WhenDoctorOrPatientNotFound()
        {
            // Arrange
            var referrals = new List<ReferralTblModel>().AsQueryable();
            var users = new List<UserTblModel>().AsQueryable(); // empty list

            var mockReferralSet = MockDbSetHelper.BuildMockDbSet(referrals);
            var mockUserSet = MockDbSetHelper.BuildMockDbSet(users, find: args => null);

            _mockContext.Setup(db => db.referrals_tbl).Returns(mockReferralSet.Object);
            _mockContext.Setup(db => db.user_tbl).Returns(mockUserSet.Object);

            var model = new ReferralDTO
            {
                appointmentID = 5,
                doctorID = 99,
                patientID = 100,
                reason = "Follow-up",
                urgencyLevel = UrgencyLevel.Medium,
                status = 1,
                sentTo = "Lab"
            };

            // Act
            var result = _service.CreateReferral(model);

            // Assert
            Assert.IsTrue(result.success);
            _mockContext.Verify(db => db.SaveChanges(), Times.Once);
            _mockEmail.Verify(es => es.SendReferralCreatedEmail(
                It.IsAny<UserTblModel>(), It.IsAny<UserTblModel>(), It.IsAny<ReferralTblModel>()), Times.Never);
        }

        [TestMethod]
        public void CreateReferral_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange
            var mockReferralSet = new Mock<DbSet<ReferralTblModel>>();
            mockReferralSet.Setup(m => m.Add(It.IsAny<ReferralTblModel>()))
                           .Throws(new Exception("Database error"));

            _mockContext.Setup(db => db.referrals_tbl).Returns(mockReferralSet.Object);

            var model = new ReferralDTO
            {
                appointmentID = 1,
                doctorID = 1,
                patientID = 2,
                reason = "Check",
                urgencyLevel = UrgencyLevel.Low,
                status = 1,
                sentTo = "Specialist"
            };

            // Act
            var result = _service.CreateReferral(model);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred while creating the referral");
        }

        [TestMethod]
        public void GetReferralDetails_ShouldReturnReferral_WhenFound()
        {
            // Arrange
            var referrals = new List<ReferralTblModel>
            {
                new ReferralTblModel
                {
                    referralID = 1,
                    appointmentID = 10,
                    doctorID = 1,
                    patientID = 2,
                    reason = "Follow-up required",
                    notes = "Check again in 1 week",
                    urgencyLevel = (int)UrgencyLevel.Medium,
                    status = 1,
                    dateCreated = new DateTime(2025, 10, 1),
                    sentTo = "Specialist"
                }
            }.AsQueryable();

            var mockReferralSet = MockDbSetHelper.BuildMockDbSet(referrals);
            _mockContext.Setup(db => db.referrals_tbl).Returns(mockReferralSet.Object);

            // Act
            var result = _service.GetReferralDetails(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);
            Assert.AreEqual(1, result.data.doctorID);
            Assert.AreEqual("Follow-up required", result.data.reason);
            Assert.AreEqual(UrgencyLevel.Medium, result.data.urgencyLevel);
        }

        [TestMethod]
        public void GetReferralDetails_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var referrals = new List<ReferralTblModel>
            {
                new ReferralTblModel { referralID = 2, reason = "Not this one" }
            }.AsQueryable();

            var mockReferralSet = MockDbSetHelper.BuildMockDbSet(referrals);
            _mockContext.Setup(db => db.referrals_tbl).Returns(mockReferralSet.Object);

            // Act
            var result = _service.GetReferralDetails(99);

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNull(result.data);
        }

        [TestMethod]
        public void GetReferralDetails_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange
            var mockReferralSet = new Mock<DbSet<ReferralTblModel>>();
            mockReferralSet.Setup(m => m.Add(It.IsAny<ReferralTblModel>()))
                           .Throws(new Exception("Database error"));

            _mockContext.Setup(db => db.referrals_tbl).Returns(mockReferralSet.Object);

            // Act
            var result = _service.GetReferralDetails(1);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred while retrieving the referral details");
        }
    }
}
