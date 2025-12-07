using Moq;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using SafeTalkApp.Services;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class EmailServiceTests
    {
        private Mock<EmailService> _mockEmailService = null!;
        private EmailService _service = null!;
        private EmailMessageDTO _captured;

        [TestInitialize]
        public void Setup()
        {
            // Mock EmailService so we can intercept SendEmail() calls
            _mockEmailService = new Mock<EmailService>() { CallBase = true };
            _service = _mockEmailService.Object;

            _mockEmailService
                .Setup(s => s.SendEmail(It.IsAny<EmailMessageDTO>()))
                .Callback<EmailMessageDTO>(dto => _captured = dto);
        }

        [TestMethod]
        public void SendVerificationEmail_ShouldCallSendEmail_WithCorrectParameters()
        {
            // Arrange
            string toEmail = "test@example.com";
            string verificationLink = "https://example.com/verify?token=abc123";

            // Act
            _service.SendVerificationEmail(toEmail, verificationLink);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.IsNotNull(_captured);
            Assert.AreEqual(toEmail, _captured.To);
            Assert.AreEqual("Email Verification", _captured.Subject);
            StringAssert.Contains(_captured.Body, verificationLink);
            StringAssert.Contains(_captured.Body, "Verify Email");
            Assert.IsTrue(_captured.IsBodyHtml);
        }

        [TestMethod]
        public void SendVerificationEmail_ShouldNotThrow_WhenEmailIsValid()
        {
            // Arrange
            string toEmail = "valid@example.com";
            string verificationLink = "https://example.com/verify";

            _mockEmailService
                .Setup(s => s.SendEmail(It.IsAny<EmailMessageDTO>()));

            // Act & Assert
            _service.SendVerificationEmail(toEmail, verificationLink);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendVerificationEmail_ShouldThrow_WhenToEmailIsNull()
        {
            // Arrange
            string toEmail = null;
            string verificationLink = "https://example.com/verify";

            // Act
            _service.SendVerificationEmail(toEmail, verificationLink);
        }

        [TestMethod]
        public void SendDoctorVerifiedAccount_ShouldCallSendEmail_WithCorrectParameters()
        {
            // Arrange
            var doctor = new UserTblModel
            {
                userID = 1,
                firstName = "John",
                lastName = "Doe",
                email = "doctor@example.com"
            };

            // Act
            _service.SendDoctorVerifiedAccount(doctor);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.IsNotNull(_captured);
            Assert.AreEqual(doctor.email, _captured.To);
            Assert.AreEqual("Your Doctor Account Has Been Verified", _captured.Subject);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, doctor.lastName);
            StringAssert.Contains(_captured.Body, "SafeTalk doctor account has been successfully verified");
            Assert.IsTrue(_captured.IsBodyHtml);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendDoctorVerifiedAccount_ShouldThrow_WhenDoctorIsNull()
        {
            // Act
            _service.SendDoctorVerifiedAccount(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendDoctorVerifiedAccount_ShouldThrow_WhenDoctorEmailIsNull()
        {
            // Arrange
            var doctor = new UserTblModel
            {
                userID = 1,
                firstName = "John",
                lastName = "Doe",
                email = null
            };

            // Act
            _service.SendDoctorVerifiedAccount(doctor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendDoctorVerifiedAccount_ShouldThrow_WhenDoctorEmailIsEmpty()
        {
            // Arrange
            var doctor = new UserTblModel
            {
                userID = 1,
                firstName = "John",
                lastName = "Doe",
                email = ""
            };

            // Act
            _service.SendDoctorVerifiedAccount(doctor);
        }

        [TestMethod]
        public void SendPasswordResetEmail_ShouldCallSendEmail_WithCorrectParameters()
        {
            // Arrange
            string toEmail = "user@example.com";
            string resetLink = "https://example.com/reset?token=abc123";

            // Act
            _service.SendPasswordResetEmail(toEmail, resetLink);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.IsNotNull(_captured);
            Assert.AreEqual(toEmail, _captured.To);
            Assert.AreEqual("Password Reset Request", _captured.Subject);
            StringAssert.Contains(_captured.Body, resetLink);
            StringAssert.Contains(_captured.Body, "Reset Password");
            StringAssert.Contains(_captured.Body, "expire in 1 hour");
            Assert.IsTrue(_captured.IsBodyHtml);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPasswordResetEmail_ShouldThrow_WhenToEmailIsNull()
        {
            // Arrange
            string toEmail = null!;
            string resetLink = "https://example.com/reset";

            // Act
            _service.SendPasswordResetEmail(toEmail, resetLink);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPasswordResetEmail_ShouldThrow_WhenToEmailIsEmpty()
        {
            // Arrange
            string toEmail = "";
            string resetLink = "https://example.com/reset";

            // Act
            _service.SendPasswordResetEmail(toEmail, resetLink);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPasswordResetEmail_ShouldThrow_WhenResetLinkIsNull()
        {
            // Arrange
            string toEmail = "user@example.com";
            string resetLink = null!;

            // Act
            _service.SendPasswordResetEmail(toEmail, resetLink);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPasswordResetEmail_ShouldThrow_WhenResetLinkIsEmpty()
        {
            // Arrange
            string toEmail = "user@example.com";
            string resetLink = "";

            // Act
            _service.SendPasswordResetEmail(toEmail, resetLink);
        }

        [TestMethod]
        public void SendDoctorAppointmentNotification_ShouldCallSendEmail_WithCorrectParameters()
        {
            // Arrange
            var doctor = new UserTblModel
            {
                firstName = "John",
                lastName = "Doe",
                email = "doctor@example.com"
            };

            var patient = new UserTblModel
            {
                firstName = "Jane",
                lastName = "Smith",
                email = "patient@example.com"
            };

            var appointment = new AppointmentsTblModel
            {
                date = new DateTime(2025, 11, 12),
                startTime = new TimeSpan(14, 30, 0)
            };

            // Act
            _service.SendDoctorAppointmentNotification(doctor, patient, appointment);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.IsNotNull(_captured);
            Assert.AreEqual(doctor.email, _captured.To);
            Assert.AreEqual("New Appointment Request", _captured.Subject);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, doctor.lastName);
            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, patient.lastName);
            StringAssert.Contains(_captured.Body, "November 12, 2025");
            StringAssert.Contains(_captured.Body, "14:30");
            Assert.IsTrue(_captured.IsBodyHtml);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendDoctorAppointmentNotification_ShouldThrow_WhenDoctorIsNull()
        {
            // Arrange
            UserTblModel doctor = null!;
            var patient = new UserTblModel();
            var appointment = new AppointmentsTblModel();

            // Act
            _service.SendDoctorAppointmentNotification(doctor, patient, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendDoctorAppointmentNotification_ShouldThrow_WhenPatientIsNull()
        {
            // Arrange
            var doctor = new UserTblModel();
            UserTblModel patient = null!;
            var appointment = new AppointmentsTblModel();

            // Act
            _service.SendDoctorAppointmentNotification(doctor, patient, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendDoctorAppointmentNotification_ShouldThrow_WhenAppointmentIsNull()
        {
            // Arrange
            var doctor = new UserTblModel();
            var patient = new UserTblModel();
            AppointmentsTblModel appointment = null!;

            // Act
            _service.SendDoctorAppointmentNotification(doctor, patient, appointment);
        }

        [TestMethod]
        public void SendPatientAppointmentConfirmation_ShouldCallSendEmail_WithCorrectParameters()
        {
            // Arrange
            var patient = new UserTblModel
            {
                firstName = "Jane",
                lastName = "Smith",
                email = "patient@example.com"
            };

            var doctor = new UserTblModel
            {
                firstName = "John",
                lastName = "Doe",
                email = "doctor@example.com"
            };

            var appointment = new AppointmentsTblModel
            {
                date = new DateTime(2025, 11, 12),
                startTime = new TimeSpan(14, 30, 0)
            };

            // Act
            _service.SendPatientAppointmentConfirmation(patient, doctor, appointment);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.IsNotNull(_captured);
            Assert.AreEqual(patient.email, _captured.To);
            Assert.AreEqual("Appointment Confirmation", _captured.Subject);
            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, patient.lastName);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, doctor.lastName);
            StringAssert.Contains(_captured.Body, "November 12, 2025");
            StringAssert.Contains(_captured.Body, "14:30");
            Assert.IsTrue(_captured.IsBodyHtml);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPatientAppointmentConfirmation_ShouldThrow_WhenPatientIsNull()
        {
            // Arrange
            UserTblModel patient = null!;
            var doctor = new UserTblModel();
            var appointment = new AppointmentsTblModel();

            // Act
            _service.SendPatientAppointmentConfirmation(patient, doctor, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPatientAppointmentConfirmation_ShouldThrow_WhenDoctorIsNull()
        {
            // Arrange
            var patient = new UserTblModel();
            UserTblModel doctor = null!;
            var appointment = new AppointmentsTblModel();

            // Act
            _service.SendPatientAppointmentConfirmation(patient, doctor, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPatientAppointmentConfirmation_ShouldThrow_WhenAppointmentIsNull()
        {
            // Arrange
            var patient = new UserTblModel();
            var doctor = new UserTblModel();
            AppointmentsTblModel appointment = null!;

            // Act
            _service.SendPatientAppointmentConfirmation(patient, doctor, appointment);
        }

        [TestMethod]
        public void SendPatientAppointmentApproved_ShouldCallSendEmail_WithCorrectParameters()
        {
            // Arrange
            var patient = new UserTblModel
            {
                firstName = "Jane",
                lastName = "Smith",
                email = "patient@example.com"
            };

            var doctor = new UserTblModel
            {
                firstName = "John",
                lastName = "Doe",
                email = "doctor@example.com"
            };

            var appointment = new AppointmentsTblModel
            {
                date = new DateTime(2025, 11, 12),
                startTime = new TimeSpan(14, 30, 0)
            };

            // Act
            _service.SendPatientAppointmentApproved(patient, doctor, appointment);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.IsNotNull(_captured);
            Assert.AreEqual(patient.email, _captured.To);
            Assert.AreEqual("Appointment Confirmation", _captured.Subject);
            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, doctor.lastName);
            StringAssert.Contains(_captured.Body, "2025-11-12");
            StringAssert.Contains(_captured.Body, "14:30");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPatientAppointmentApproved_ShouldThrow_WhenPatientIsNull()
        {
            var doctor = new UserTblModel();
            var appointment = new AppointmentsTblModel();

            _service.SendPatientAppointmentApproved(null, doctor, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPatientAppointmentApproved_ShouldThrow_WhenDoctorIsNull()
        {
            var patient = new UserTblModel();
            var appointment = new AppointmentsTblModel();

            _service.SendPatientAppointmentApproved(patient, null, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPatientAppointmentApproved_ShouldThrow_WhenAppointmentIsNull()
        {
            var patient = new UserTblModel();
            var doctor = new UserTblModel();

            _service.SendPatientAppointmentApproved(patient, doctor, null);
        }

        [TestMethod]
        public void SendDoctorAppointmentApproved_ShouldCallSendEmail_WithCorrectParameters()
        {
            // Arrange
            var doctor = new UserTblModel
            {
                firstName = "John",
                lastName = "Doe",
                email = "doctor@example.com"
            };

            var patient = new UserTblModel
            {
                firstName = "Jane",
                lastName = "Smith",
                email = "patient@example.com"
            };

            var appointment = new AppointmentsTblModel
            {
                date = new DateTime(2025, 11, 12),
                startTime = new TimeSpan(14, 30, 0)
            };

            // Act
            _service.SendDoctorAppointmentApproved(doctor, patient, appointment);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.IsNotNull(_captured);
            Assert.AreEqual(doctor.email, _captured.To);
            Assert.AreEqual("Appointment Confirmation", _captured.Subject);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, patient.lastName);
            StringAssert.Contains(_captured.Body, "2025-11-12");
            StringAssert.Contains(_captured.Body, "14:30");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendDoctorAppointmentApproved_ShouldThrow_WhenDoctorIsNull()
        {
            var patient = new UserTblModel();
            var appointment = new AppointmentsTblModel();

            _service.SendDoctorAppointmentApproved(null, patient, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendDoctorAppointmentApproved_ShouldThrow_WhenPatientIsNull()
        {
            var doctor = new UserTblModel();
            var appointment = new AppointmentsTblModel();

            _service.SendDoctorAppointmentApproved(doctor, null, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendDoctorAppointmentApproved_ShouldThrow_WhenAppointmentIsNull()
        {
            var doctor = new UserTblModel();
            var patient = new UserTblModel();

            _service.SendDoctorAppointmentApproved(doctor, patient, null);
        }

        [TestMethod]
        public void SendDoctorAppointmentCancellation_ShouldCallSendEmail_WithCorrectParameters()
        {
            // Arrange
            var doctor = new UserTblModel
            {
                firstName = "John",
                lastName = "Doe",
                email = "doctor@example.com"
            };

            var patient = new UserTblModel
            {
                firstName = "Jane",
                lastName = "Smith",
                email = "patient@example.com"
            };

            var appointment = new AppointmentsTblModel
            {
                date = new DateTime(2025, 11, 12),
                startTime = new TimeSpan(14, 30, 0)
            };

            // Act
            _service.SendDoctorAppointmentCancellation(doctor, patient, appointment);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.IsNotNull(_captured);
            Assert.AreEqual(doctor.email, _captured.To);
            Assert.AreEqual("Appointment Cancelled", _captured.Subject);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, doctor.lastName);
            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, patient.lastName);
            StringAssert.Contains(_captured.Body, "2025-11-12");
            StringAssert.Contains(_captured.Body, "14:30");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendDoctorAppointmentCancellation_ShouldThrow_WhenDoctorIsNull()
        {
            var patient = new UserTblModel();
            var appointment = new AppointmentsTblModel();

            _service.SendDoctorAppointmentCancellation(null, patient, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendDoctorAppointmentCancellation_ShouldThrow_WhenPatientIsNull()
        {
            var doctor = new UserTblModel();
            var appointment = new AppointmentsTblModel();

            _service.SendDoctorAppointmentCancellation(doctor, null, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendDoctorAppointmentCancellation_ShouldThrow_WhenAppointmentIsNull()
        {
            var doctor = new UserTblModel();
            var patient = new UserTblModel();

            _service.SendDoctorAppointmentCancellation(doctor, patient, null);
        }

        [TestMethod]
        public void SendPatientAppointmentCancellation_ShouldCallSendEmail_WithCorrectParameters()
        {
            // Arrange
            var patient = new UserTblModel
            {
                firstName = "Jane",
                lastName = "Smith",
                email = "patient@example.com"
            };

            var doctor = new UserTblModel
            {
                firstName = "John",
                lastName = "Doe",
                email = "doctor@example.com"
            };

            var appointment = new AppointmentsTblModel
            {
                date = new DateTime(2025, 11, 12),
                startTime = new TimeSpan(14, 30, 0)
            };

            // Act
            _service.SendPatientAppointmentCancellation(patient, doctor, appointment);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.IsNotNull(_captured);
            Assert.AreEqual(patient.email, _captured.To);
            Assert.AreEqual("Appointment Cancelled", _captured.Subject);
            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, doctor.lastName);
            StringAssert.Contains(_captured.Body, "2025-11-12");
            StringAssert.Contains(_captured.Body, "14:30");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPatientAppointmentCancellation_ShouldThrow_WhenPatientIsNull()
        {
            var doctor = new UserTblModel();
            var appointment = new AppointmentsTblModel();

            _service.SendPatientAppointmentCancellation(null, doctor, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPatientAppointmentCancellation_ShouldThrow_WhenDoctorIsNull()
        {
            var patient = new UserTblModel();
            var appointment = new AppointmentsTblModel();

            _service.SendPatientAppointmentCancellation(patient, null, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPatientAppointmentCancellation_ShouldThrow_WhenAppointmentIsNull()
        {
            var patient = new UserTblModel();
            var doctor = new UserTblModel();

            _service.SendPatientAppointmentCancellation(patient, doctor, null);
        }

        [TestMethod]
        public void SendPatientAppointmentRejected_ShouldCallSendEmail_WithCorrectParameters()
        {
            // Arrange
            var patient = new UserTblModel
            {
                firstName = "Jane",
                lastName = "Smith",
                email = "patient@example.com"
            };

            var doctor = new UserTblModel
            {
                firstName = "John",
                lastName = "Doe",
                email = "doctor@example.com"
            };

            var appointment = new AppointmentsTblModel
            {
                date = new DateTime(2025, 11, 12),
                startTime = new TimeSpan(10, 0, 0)
            };

            // Act
            _service.SendPatientAppointmentRejected(patient, doctor, appointment);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.IsNotNull(_captured);
            Assert.AreEqual(patient.email, _captured.To);
            Assert.AreEqual("Appointment Rejected", _captured.Subject);
            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, doctor.lastName);
            StringAssert.Contains(_captured.Body, "2025-11-12");
            StringAssert.Contains(_captured.Body, "10:00");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPatientAppointmentRejected_ShouldThrow_WhenPatientIsNull()
        {
            var doctor = new UserTblModel();
            var appointment = new AppointmentsTblModel();
            _service.SendPatientAppointmentRejected(null, doctor, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPatientAppointmentRejected_ShouldThrow_WhenDoctorIsNull()
        {
            var patient = new UserTblModel();
            var appointment = new AppointmentsTblModel();
            _service.SendPatientAppointmentRejected(patient, null, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPatientAppointmentRejected_ShouldThrow_WhenAppointmentIsNull()
        {
            var patient = new UserTblModel();
            var doctor = new UserTblModel();
            _service.SendPatientAppointmentRejected(patient, doctor, null);
        }

        [TestMethod]
        public void SendDoctorAppointmentRejected_ShouldCallSendEmail_WithCorrectParameters()
        {
            // Arrange
            var doctor = new UserTblModel
            {
                firstName = "John",
                lastName = "Doe",
                email = "doctor@example.com"
            };

            var patient = new UserTblModel
            {
                firstName = "Jane",
                lastName = "Smith",
                email = "patient@example.com"
            };

            var appointment = new AppointmentsTblModel
            {
                date = new DateTime(2025, 11, 12),
                startTime = new TimeSpan(14, 30, 0)
            };

            // Act
            _service.SendDoctorAppointmentRejected(doctor, patient, appointment);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.IsNotNull(_captured);
            Assert.AreEqual(doctor.email, _captured.To);
            Assert.AreEqual("Appointment Rejected", _captured.Subject);

            // Body checks
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, doctor.lastName);
            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, patient.lastName);
            StringAssert.Contains(_captured.Body, "2025-11-12");
            StringAssert.Contains(_captured.Body, "14:30");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendDoctorAppointmentRejected_ShouldThrow_WhenDoctorIsNull()
        {
            var patient = new UserTblModel();
            var appointment = new AppointmentsTblModel();
            _service.SendDoctorAppointmentRejected(null, patient, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendDoctorAppointmentRejected_ShouldThrow_WhenPatientIsNull()
        {
            var doctor = new UserTblModel();
            var appointment = new AppointmentsTblModel();
            _service.SendDoctorAppointmentRejected(doctor, null, appointment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendDoctorAppointmentRejected_ShouldThrow_WhenAppointmentIsNull()
        {
            var doctor = new UserTblModel();
            var patient = new UserTblModel();
            _service.SendDoctorAppointmentRejected(doctor, patient, null);
        }

        [TestMethod]
        public void SendReferralCreatedEmail_ShouldCallSendEmail_WithCorrectParameters()
        {
            // Arrange
            var patient = new UserTblModel
            {
                firstName = "Alice",
                lastName = "Walker",
                email = "patient@example.com"
            };

            var doctor = new UserTblModel
            {
                firstName = "John",
                lastName = "Doe",
                email = "doctor@example.com"
            };

            var referral = new ReferralTblModel
            {
                sentTo = "Cardiology Department",
                reason = "Chest pain",
                urgencyLevel = (int)UrgencyLevel.High,
                dateCreated = new DateTime(2025, 11, 15)
            };

            // Act
            _service.SendReferralCreatedEmail(patient, doctor, referral);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.IsNotNull(_captured);
            Assert.AreEqual(patient.email, _captured.To);
            Assert.AreEqual("New Referral Created for You", _captured.Subject);

            // Body checks
            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, doctor.lastName);
            StringAssert.Contains(_captured.Body, referral.sentTo);
            StringAssert.Contains(_captured.Body, referral.reason);
            StringAssert.Contains(_captured.Body, UrgencyLevel.High.ToString());
            StringAssert.Contains(_captured.Body, "November 15, 2025"); // formatted date

            Assert.IsTrue(_captured.IsBodyHtml);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendReferralCreatedEmail_ShouldThrow_WhenPatientIsNull()
        {
            var doctor = new UserTblModel();
            var referral = new ReferralTblModel();

            _service.SendReferralCreatedEmail(null, doctor, referral);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendReferralCreatedEmail_ShouldThrow_WhenDoctorIsNull()
        {
            var patient = new UserTblModel();
            var referral = new ReferralTblModel();

            _service.SendReferralCreatedEmail(patient, null, referral);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendReferralCreatedEmail_ShouldThrow_WhenReferralIsNull()
        {
            var patient = new UserTblModel();
            var doctor = new UserTblModel();

            _service.SendReferralCreatedEmail(patient, doctor, null);
        }

        [TestMethod]
        public void SendPaymentSubmittedEmail_ShouldCallSendEmail_WithCorrectParameters()
        {
            // Arrange
            var admin = new UserTblModel
            {
                firstName = "Admin",
                lastName = "Account",
                email = "admin@example.com"
            };

            var patient = new UserTblModel
            {
                firstName = "Alice",
                lastName = "Walker",
                email = "patient@example.com"
            };

            var doctor = new UserTblModel
            {
                firstName = "John",
                lastName = "Doe",
                email = "doctor@example.com"
            };

            var appointment = new AppointmentsTblModel
            {
                appointmentID = 42,
                date = new DateTime(2025, 11, 20),
                startTime = new TimeSpan(10, 30, 0)
            };

            var payment = new PaymentTblModel
            {
                amount = 1500.00m,
                paymentDate = new DateTime(2025, 11, 18, 14, 45, 00),
                status = PaymentStatus.Pending
            };

            // Act
            _service.SendPaymentSubmittedEmail(admin, appointment, payment, patient, doctor);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.IsNotNull(_captured);
            Assert.AreEqual(admin.email, _captured.To);
            Assert.AreEqual("New Payment Submitted – Appointment #42", _captured.Subject);

            // Body checks
            StringAssert.Contains(_captured.Body, "42");
            StringAssert.Contains(_captured.Body, "Alice Walker");
            StringAssert.Contains(_captured.Body, "Dr. John Doe");
            StringAssert.Contains(_captured.Body, "₱1,500.00");
            StringAssert.Contains(_captured.Body, "Pending");
            StringAssert.Contains(_captured.Body, "November 18, 2025 02:45 PM");
            StringAssert.Contains(_captured.Body, "View Payment Details");

            Assert.IsTrue(_captured.IsBodyHtml);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPaymentSubmittedEmail_ShouldThrow_WhenAdminIsNull()
        {
            var appointment = new AppointmentsTblModel();
            var payment = new PaymentTblModel();
            var patient = new UserTblModel();
            var doctor = new UserTblModel();

            _service.SendPaymentSubmittedEmail(null, appointment, payment, patient, doctor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPaymentSubmittedEmail_ShouldThrow_WhenAppointmentIsNull()
        {
            var admin = new UserTblModel();
            var payment = new PaymentTblModel();
            var patient = new UserTblModel();
            var doctor = new UserTblModel();

            _service.SendPaymentSubmittedEmail(admin, null, payment, patient, doctor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPaymentSubmittedEmail_ShouldThrow_WhenPaymentIsNull()
        {
            var admin = new UserTblModel();
            var appointment = new AppointmentsTblModel();
            var patient = new UserTblModel();
            var doctor = new UserTblModel();

            _service.SendPaymentSubmittedEmail(admin, appointment, null, patient, doctor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPaymentSubmittedEmail_ShouldThrow_WhenPatientIsNull()
        {
            var admin = new UserTblModel();
            var appointment = new AppointmentsTblModel();
            var payment = new PaymentTblModel();
            var doctor = new UserTblModel();

            _service.SendPaymentSubmittedEmail(admin, appointment, payment, null, doctor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPaymentSubmittedEmail_ShouldThrow_WhenDoctorIsNull()
        {
            var admin = new UserTblModel();
            var appointment = new AppointmentsTblModel();
            var payment = new PaymentTblModel();
            var patient = new UserTblModel();

            _service.SendPaymentSubmittedEmail(admin, appointment, payment, patient, null);
        }

        [TestMethod]
        public void SendPaymentVerifiedEmailToPatient_ShouldSendEmail_WhenValidInputs()
        {
            // Arrange
            var patient = new UserTblModel { email = "patient@test.com", firstName = "Alice", lastName = "Walker" };
            var doctor = new UserTblModel { email = "doc@test.com", firstName = "John", lastName = "Doe" };
            var appointment = new AppointmentsTblModel
            {
                appointmentID = 42,
                date = new DateTime(2025, 11, 18),
                startTime = new TimeSpan(14, 0, 0),
                endTime = new TimeSpan(15, 0, 0)
            };
            var payment = new PaymentTblModel
            {
                amount = 1500m,
                status = PaymentStatus.Completed
            };

            // Act
            _service.SendPaymentVerifiedEmailToPatient(patient, doctor, appointment, payment);

            // Assert
            _mockEmailService.Verify(s => s.SendEmail(It.IsAny<EmailMessageDTO>()), Times.Once);

            Assert.AreEqual(patient.email, _captured.To);
            Assert.AreEqual("Your Payment Has Been Verified", _captured.Subject);

            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, "₱1,500.00");
            StringAssert.Contains(_captured.Body, "Verified");

            // Date & time checks
            StringAssert.Contains(_captured.Body, "November 18, 2025");
            StringAssert.Contains(_captured.Body, "02:00 PM");
            StringAssert.Contains(_captured.Body, "03:00 PM");
        }

        [TestMethod]
        public void SendPaymentVerifiedEmailToPatient_ShouldThrow_WhenPatientIsNull()
        {
            var doctor = new UserTblModel();
            var appointment = new AppointmentsTblModel();
            var payment = new PaymentTblModel();

            Assert.ThrowsException<ArgumentNullException>(() =>
                _service.SendPaymentVerifiedEmailToPatient(null, doctor, appointment, payment));
        }

        [TestMethod]
        public void SendPaymentVerifiedEmailToPatient_ShouldThrow_WhenDoctorIsNull()
        {
            var patient = new UserTblModel();
            var appointment = new AppointmentsTblModel();
            var payment = new PaymentTblModel();

            Assert.ThrowsException<ArgumentNullException>(() =>
                _service.SendPaymentVerifiedEmailToPatient(patient, null, appointment, payment));
        }

        [TestMethod]
        public void SendPaymentVerifiedEmailToPatient_ShouldThrow_WhenAppointmentIsNull()
        {
            var patient = new UserTblModel();
            var doctor = new UserTblModel();
            var payment = new PaymentTblModel();

            Assert.ThrowsException<ArgumentNullException>(() =>
                _service.SendPaymentVerifiedEmailToPatient(patient, doctor, null, payment));
        }

        [TestMethod]
        public void SendPaymentVerifiedEmailToPatient_ShouldThrow_WhenPaymentIsNull()
        {
            var patient = new UserTblModel();
            var doctor = new UserTblModel();
            var appointment = new AppointmentsTblModel();

            Assert.ThrowsException<ArgumentNullException>(() =>
                _service.SendPaymentVerifiedEmailToPatient(patient, doctor, appointment, null));
        }

        [TestMethod]
        public void SendPaymentVerifiedEmailToDoctor_ShouldThrow_WhenDoctorIsNull()
        {
            // Arrange
            UserTblModel doctor = null!;
            var patient = new UserTblModel();
            var appointment = new AppointmentsTblModel();
            var payment = new PaymentTblModel();

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
                _service.SendPaymentVerifiedEmailToDoctor(doctor, patient, appointment, payment)
            );
        }

        [TestMethod]
        public void SendPaymentVerifiedEmailToDoctor_ShouldThrow_WhenPatientIsNull()
        {
            // Arrange
            UserTblModel patient = null!;
            var doctor = new UserTblModel();
            var appointment = new AppointmentsTblModel();
            var payment = new PaymentTblModel();

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
                _service.SendPaymentVerifiedEmailToDoctor(doctor, patient, appointment, payment)
            );
        }

        [TestMethod]
        public void SendPaymentVerifiedEmailToDoctor_ShouldThrow_WhenAppointmentIsNull()
        {
            // Arrange
            AppointmentsTblModel appointment = null!;
            var doctor = new UserTblModel();
            var patient = new UserTblModel();
            var payment = new PaymentTblModel();

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
                _service.SendPaymentVerifiedEmailToDoctor(doctor, patient, appointment, payment)
            );
        }

        [TestMethod]
        public void SendPaymentVerifiedEmailToDoctor_ShouldThrow_WhenPaymentIsNull()
        {
            // Arrange
            PaymentTblModel payment = null!;
            var doctor = new UserTblModel();
            var patient = new UserTblModel();
            var appointment = new AppointmentsTblModel();

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
                _service.SendPaymentVerifiedEmailToDoctor(doctor, patient, appointment, payment)
            );
        }

        [TestMethod]
        public void SendPaymentVerifiedEmailToDoctor_ShouldSendCorrectEmail_WhenValid()
        {
            // Arrange
            var doctor = new UserTblModel
            {
                email = "dr.john@example.com",
                firstName = "John",
                lastName = "Doe"
            };

            var patient = new UserTblModel
            {
                firstName = "Alice",
                lastName = "Walker"
            };

            var appointment = new AppointmentsTblModel
            {
                date = new DateTime(2025, 11, 20),
                startTime = new TimeSpan(14, 0, 0),
                endTime = new TimeSpan(14, 30, 0)
            };

            var payment = new PaymentTblModel
            {
                amount = 1500.00m,
                status = PaymentStatus.Completed
            };

            // Act
            _service.SendPaymentVerifiedEmailToDoctor(doctor, patient, appointment, payment);

            // Assert
            Assert.IsNotNull(_captured, "SendEmail was not called.");
            Assert.AreEqual(doctor.email, _captured!.To);
            Assert.AreEqual("A Patient’s Payment Has Been Verified", _captured.Subject);

            // Body checks
            StringAssert.Contains(_captured.Body, "John");
            StringAssert.Contains(_captured.Body, "Alice Walker");
            StringAssert.Contains(_captured.Body, "November 20, 2025");
            StringAssert.Contains(_captured.Body, "02:00 PM");
            StringAssert.Contains(_captured.Body, "₱1,500.00");
            StringAssert.Contains(_captured.Body, "Paid and Confirmed");
        }

        [TestMethod]
        public void SendPaymentRejectedEmailToPatient_ShouldSendEmail_WithCorrectContent()
        {
            // Arrange
            var patient = new UserTblModel { firstName = "Alice", lastName = "Walker", email = "alice@example.com" };
            var doctor = new UserTblModel { firstName = "John", lastName = "Doe", email = "john@example.com" };
            var appointment = new AppointmentsTblModel
            {
                date = DateTime.Today,
                startTime = new TimeSpan(14, 30, 0),
                endTime = new TimeSpan(15, 0, 0)
            };
            var payment = new PaymentTblModel { amount = 1500m };

            // Act
            _service.SendPaymentRejectedEmailToPatient(patient, doctor, appointment, payment);

            // Assert
            Assert.IsNotNull(_captured);
            Assert.AreEqual(patient.email, _captured.To);
            Assert.AreEqual("Your Payment Has Been Rejected", _captured.Subject);
            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, "Rejected");
            StringAssert.Contains(_captured.Body, "₱1,500.00");
            Assert.IsTrue(_captured.IsBodyHtml);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPaymentRejectedEmailToPatient_ShouldThrow_WhenPatientIsNull()
        {
            _service.SendPaymentRejectedEmailToPatient(null, new UserTblModel(), new AppointmentsTblModel(), new PaymentTblModel());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPaymentRejectedEmailToPatient_ShouldThrow_WhenDoctorIsNull()
        {
            _service.SendPaymentRejectedEmailToPatient(new UserTblModel(), null, new AppointmentsTblModel(), new PaymentTblModel());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPaymentRejectedEmailToPatient_ShouldThrow_WhenAppointmentIsNull()
        {
            _service.SendPaymentRejectedEmailToPatient(new UserTblModel(), new UserTblModel(), null, new PaymentTblModel());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPaymentRejectedEmailToPatient_ShouldThrow_WhenPaymentIsNull()
        {
            _service.SendPaymentRejectedEmailToPatient(new UserTblModel(), new UserTblModel(), new AppointmentsTblModel(), null);
        }

        [TestMethod]
        public void SendPaymentRejectedEmailToDoctor_ShouldSendEmail_WithCorrectContent()
        {
            // Arrange
            var doctor = new UserTblModel { firstName = "John", lastName = "Doe", email = "john@example.com" };
            var patient = new UserTblModel { firstName = "Alice", lastName = "Walker", email = "alice@example.com" };
            var appointment = new AppointmentsTblModel
            {
                date = DateTime.Today,
                startTime = new TimeSpan(14, 30, 0),
                endTime = new TimeSpan(15, 0, 0)
            };
            var payment = new PaymentTblModel { amount = 1500m };

            // Act
            _service.SendPaymentRejectedEmailToDoctor(doctor, patient, appointment, payment);

            // Assert
            Assert.IsNotNull(_captured);
            Assert.AreEqual(doctor.email, _captured.To);
            Assert.AreEqual("A Patient’s Payment Has Been Rejected", _captured.Subject);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, "Rejected");
            StringAssert.Contains(_captured.Body, "₱1,500.00");
            Assert.IsTrue(_captured.IsBodyHtml);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPaymentRejectedEmailToDoctor_ShouldThrow_WhenPatientIsNull()
        {
            _service.SendPaymentRejectedEmailToDoctor(new UserTblModel(), null, new AppointmentsTblModel(), new PaymentTblModel());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPaymentRejectedEmailToDoctor_ShouldThrow_WhenDoctorIsNull()
        {
            _service.SendPaymentRejectedEmailToDoctor(null, new UserTblModel(), new AppointmentsTblModel(), new PaymentTblModel());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPaymentRejectedEmailToDoctor_ShouldThrow_WhenAppointmentIsNull()
        {
            _service.SendPaymentRejectedEmailToDoctor(new UserTblModel(), new UserTblModel(), null, new PaymentTblModel());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendPaymentRejectedEmailToDoctor_ShouldThrow_WhenPaymentIsNull()
        {
            _service.SendPaymentRejectedEmailToDoctor(new UserTblModel(), new UserTblModel(), new AppointmentsTblModel(), null);
        }

        [TestMethod]
        public void SendTranscriptionReadyToPatient_ShouldSendEmail_WithCorrectContent()
        {
            // Arrange
            var patient = new UserTblModel { firstName = "Alice", lastName = "Walker", email = "alice@example.com" };
            var doctor = new UserTblModel { firstName = "John", lastName = "Doe", email = "john@example.com" };
            var appointment = new AppointmentsTblModel
            {
                date = DateTime.Today,
                startTime = new TimeSpan(14, 0, 0),
                endTime = new TimeSpan(14, 30, 0)
            };
            string transcriptFileName = "transcript123.pdf";

            // Act
            _service.SendTranscriptionReadyToPatient(patient, doctor, appointment, transcriptFileName);

            // Assert
            Assert.IsNotNull(_captured);
            Assert.AreEqual(patient.email, _captured.To);
            Assert.AreEqual("Your Consultation Transcript is Ready!", _captured.Subject);
            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, transcriptFileName);
            Assert.IsTrue(_captured.IsBodyHtml);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendTranscriptionReadyToPatient_ShouldThrow_WhenPatientIsNull()
        {
            _service.SendTranscriptionReadyToPatient(null, new UserTblModel(), new AppointmentsTblModel(), "file.pdf");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendTranscriptionReadyToPatient_ShouldThrow_WhenDoctorIsNull()
        {
            _service.SendTranscriptionReadyToPatient(new UserTblModel(), null, new AppointmentsTblModel(), "file.pdf");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendTranscriptionReadyToPatient_ShouldThrow_WhenAppointmentIsNull()
        {
            _service.SendTranscriptionReadyToPatient(new UserTblModel(), new UserTblModel(), null, "file.pdf");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendTranscriptionReadyToPatient_ShouldThrow_WhenTranscriptFileNameIsNull()
        {
            _service.SendTranscriptionReadyToPatient(new UserTblModel(), new UserTblModel(), new AppointmentsTblModel(), null);
        }

        [TestMethod]
        public void SendTranscriptionReadyToDoctor_ShouldSendEmail_WithCorrectContent()
        {
            // Arrange
            var doctor = new UserTblModel { firstName = "John", lastName = "Doe", email = "john@example.com" };
            var patient = new UserTblModel { firstName = "Alice", lastName = "Walker", email = "alice@example.com" };
            var appointment = new AppointmentsTblModel
            {
                date = DateTime.Today,
                startTime = new TimeSpan(14, 0, 0),
                endTime = new TimeSpan(14, 30, 0)
            };
            string transcriptFileName = "transcript123.pdf";

            // Act
            _service.SendTranscriptionReadyToDoctor(doctor, patient, appointment, transcriptFileName);

            // Assert
            Assert.IsNotNull(_captured);
            Assert.AreEqual(doctor.email, _captured.To);
            Assert.AreEqual("Consultation Transcript Generated", _captured.Subject);
            StringAssert.Contains(_captured.Body, doctor.firstName);
            StringAssert.Contains(_captured.Body, patient.firstName);
            StringAssert.Contains(_captured.Body, transcriptFileName);
            Assert.IsTrue(_captured.IsBodyHtml);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendTranscriptionReadyToDoctor_ShouldThrow_WhenPatientIsNull()
        {
            _service.SendTranscriptionReadyToDoctor(new UserTblModel(), null, new AppointmentsTblModel(), "file.pdf");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendTranscriptionReadyToDoctor_ShouldThrow_WhenDoctorIsNull()
        {
            _service.SendTranscriptionReadyToDoctor(null, new UserTblModel(), new AppointmentsTblModel(), "file.pdf");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendTranscriptionReadyToDoctor_ShouldThrow_WhenAppointmentIsNull()
        {
            _service.SendTranscriptionReadyToDoctor(new UserTblModel(), new UserTblModel(), null, "file.pdf");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendTranscriptionReadyToDoctor_ShouldThrow_WhenTranscriptFileNameIsNull()
        {
            _service.SendTranscriptionReadyToDoctor(new UserTblModel(), new UserTblModel(), new AppointmentsTblModel(), null);
        }
    }
}
