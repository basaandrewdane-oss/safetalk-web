using Moq;
using SafeTalkApp.DTOs.Appointment;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SafeTalkApp.DTOs.Appointment.DoctorAvailabilityDTO;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class AppointmentServiceTests
    {
        private Mock<ISafeTalkAppContext> _mockContext = null!;
        private Mock<IEmailService> _mockEmail = null!;
        private AppointmentService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<ISafeTalkAppContext>();
            _mockEmail = new Mock<IEmailService>();
            _service = new AppointmentService(_mockContext.Object, _mockEmail.Object);
        }

        [TestMethod]
        public void GetAppointmentStatus_ShouldReturnAppointment_WhenFound()
        {
            // Arrange
            var date = new DateTime(2025, 10, 3, 14, 0, 0); // Oct 3, 2025 2:00PM
            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel
                {
                    appointmentID = 1,
                    status = 3,
                    date = date,
                    endTime = new TimeSpan(1, 30, 0) // 1hr 30min
                }
            }.AsQueryable();

            var mockAppointmentsDbSet = MockDbSetHelper.BuildMockDbSet(appointments);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointmentsDbSet.Object);

            // Act
            var result = _service.GetAppointmentStatus(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);
            Assert.AreEqual(3, result.data.status);
            Assert.AreEqual(date.Add(new TimeSpan(1, 30, 0)), result.data.endTime);
        }

        [TestMethod]
        public void GetAppointmentStatus_ShouldReturnFail_WhenNotFound()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>().AsQueryable();
            var mockAppointmentsDbSet = MockDbSetHelper.BuildMockDbSet(appointments);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointmentsDbSet.Object);

            // Act
            var result = _service.GetAppointmentStatus(999);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Appointment not found.", result.message);
            Assert.IsNull(result.data);
        }

        [TestMethod]
        public void GetAppointmentStatus_ShouldHandleException()
        {
            // Arrange
            // Don’t setup appointments_tbl -> will cause NullReferenceException inside service
            _mockContext.Setup(c => c.appointments_tbl).Throws(new Exception("DB error"));

            // Act
            var result = _service.GetAppointmentStatus(1);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "Error retrieving appointment status");
        }

        [TestMethod]
        public void GetDoctors_ShouldReturnDoctors_WhenFound()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 1, firstName = "John", lastName = "Doe", email = "john@hospital.com", specialization = "Cardiology", isVerified = true },
                new UserTblModel { userID = 2, firstName = "Jane", lastName = "Smith", email = "jane@hospital.com", specialization = "Neurology", isVerified = true }
            }.AsQueryable();

            var roles = new List<RoleTblModel>
            {
                new RoleTblModel { roleID = 2, roleName = "Doctor" },
                new RoleTblModel { roleID = 1, roleName = "Patient" }
            }.AsQueryable();

            var userRoles = new List<UserRoleTblModel>
            {
                new UserRoleTblModel { userID = 1, roleID = 2 },
                new UserRoleTblModel { userID = 2, roleID = 2 }
            }.AsQueryable();

            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users).Object);
            _mockContext.Setup(c => c.role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(roles).Object);
            _mockContext.Setup(c => c.user_role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(userRoles).Object);

            // Act
            var result = _service.GetDoctors();

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(2, result.data.Count());
            Assert.AreEqual("John Doe", result.data.First().fullName);
            Assert.AreEqual("Cardiology", result.data.First().specialization);
        }

        [TestMethod]
        public void GetDoctors_ShouldReturnEmpty_WhenNoDoctorsFound()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 1, firstName = "Patient", lastName = "One", email = "patient@hospital.com", specialization = "N/A", isVerified = true }
            }.AsQueryable();

            var roles = new List<RoleTblModel>
            {
                new RoleTblModel { roleID = 2, roleName = "User" }
            }.AsQueryable();

            var userRoles = new List<UserRoleTblModel>
            {
                new UserRoleTblModel { userID = 1, roleID = 2 }
            }.AsQueryable();

            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users).Object);
            _mockContext.Setup(c => c.role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(roles).Object);
            _mockContext.Setup(c => c.user_role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(userRoles).Object);

            // Act
            var result = _service.GetDoctors();

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void GetDoctors_ShouldHandleException()
        {
            // Arrange
            _mockContext.Setup(c => c.user_tbl).Throws(new Exception("DB error"));

            // Act
            var result = _service.GetDoctors();

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "Error retrieving doctors");
        }

        [TestMethod]
        public void GetDoctorsAvailability_ShouldReturnAvailability_WhenFound()
        {
            // Arrange
            var availabilities = new List<UserAvailabilityTblModel>
            {
                new UserAvailabilityTblModel
                {
                    availabilityID = 1,
                    userID = 10,
                    dayID = 1,
                    availabilityStart = new TimeSpan(9, 0, 0),
                    availabilityEnd = new TimeSpan(12, 0, 0),
                    fee = 500
                }
            }.AsQueryable();

            var days = new List<DaysOfWeekTblModel>
            {
                new DaysOfWeekTblModel { dayID = 1, day = "Monday" }
            }.AsQueryable();

            _mockContext.Setup(c => c.user_availability_tbl).Returns(MockDbSetHelper.BuildMockDbSet(availabilities).Object);
            _mockContext.Setup(c => c.days_of_week_tbl).Returns(MockDbSetHelper.BuildMockDbSet(days).Object);

            // Act
            var result = _service.GetDoctorsAvailability(10);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(1, result.data.Count());

            var dto = result.data.First();
            Assert.AreEqual(1, dto.availabilityID);
            Assert.AreEqual("Monday", dto.day);
            Assert.AreEqual(500, dto.fee);
            Assert.IsTrue(dto.slots.Any(), "Slots should be generated");
        }

        [TestMethod]
        public void GetDoctorsAvailability_ShouldReturnEmpty_WhenNoAvailability()
        {
            // Arrange
            var availabilities = new List<UserAvailabilityTblModel>().AsQueryable();
            var days = new List<DaysOfWeekTblModel>
            {
                new DaysOfWeekTblModel { dayID = 1, day = "Monday" }
            }.AsQueryable();

            _mockContext.Setup(c => c.user_availability_tbl).Returns(MockDbSetHelper.BuildMockDbSet(availabilities).Object);
            _mockContext.Setup(c => c.days_of_week_tbl).Returns(MockDbSetHelper.BuildMockDbSet(days).Object);

            // Act
            var result = _service.GetDoctorsAvailability(99);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void GetDoctorsAvailability_ShouldHandleException()
        {
            // Arrange
            _mockContext.Setup(c => c.user_availability_tbl).Throws(new Exception("DB error"));

            // Act
            var result = _service.GetDoctorsAvailability(10);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "Error retrieving availability");
        }

        [TestMethod]
        public void GenerateTimeSlots_ShouldReturnCorrectSlots()
        {
            // Arrange
            var start = new TimeSpan(9, 0, 0);   // 9:00 AM
            var end = new TimeSpan(12, 0, 0);    // 12:00 PM
            var interval = 30;                   // 30 minutes

            // Act (use reflection to call private method)
            var methodInfo = typeof(AppointmentService)
                .GetMethod("GenerateTimeSlots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.IsNotNull(methodInfo, "GenerateTimeSlots method not found via reflection.");

            var result = methodInfo.Invoke(_service, new object[] { start, end, interval }) as IEnumerable<TimeSlotDTO>;

            // Assert
            Assert.IsNotNull(result);
            var slots = result.ToList();
            Assert.AreEqual(6, slots.Count); // 9:00–9:30, 9:30–10:00, 10:00–10:30, 10:30–11:00, 11:00–11:30, 11:30–12:00

            Assert.AreEqual("09:00:00", slots.First().start);
            Assert.AreEqual("09:30:00", slots.First().end);

            Assert.AreEqual("11:30:00", slots.Last().start);
            Assert.AreEqual("12:00:00", slots.Last().end);
        }

        [TestMethod]
        public void GenerateTimeSlots_ShouldReturnEmpty_WhenStartEqualsEnd()
        {
            // Arrange
            var start = new TimeSpan(9, 0, 0);
            var end = new TimeSpan(9, 0, 0);

            // Act
            var methodInfo = typeof(AppointmentService)
                .GetMethod("GenerateTimeSlots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = methodInfo.Invoke(_service, new object[] { start, end, 30 }) as IEnumerable<TimeSlotDTO>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void GenerateTimeSlots_ShouldReturnEmpty_WhenIntervalGreaterThanRange()
        {
            // Arrange
            var start = new TimeSpan(9, 0, 0);
            var end = new TimeSpan(9, 15, 0);  // only 15 minutes later
            var interval = 30;                 // interval too large

            // Act
            var methodInfo = typeof(AppointmentService)
                .GetMethod("GenerateTimeSlots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = methodInfo.Invoke(_service, new object[] { start, end, interval }) as IEnumerable<TimeSlotDTO>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void BookAppointment_ShouldSucceed_WhenNoOverlap()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>().AsQueryable();

            var mockAppointments = MockDbSetHelper.BuildMockDbSet(appointments);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);

            var model = new BookAppointmentDTO
            {
                doctorID = 1,
                date = new DateTime(2025, 10, 3),
                startTime = new TimeSpan(9, 0, 0),
                endTime = new TimeSpan(9, 30, 0),
                fee = 500,
                chiefComplaint = "Headache"
            };

            // Act
            var result = _service.BookAppointment(model, patientID: 10);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Appointment booked successfully.", result.message);
            Assert.IsNotNull(result.data);
            Assert.AreEqual(1, result.data.doctorID);
            Assert.AreEqual(10, result.data.patientID);
            Assert.AreEqual(new TimeSpan(9, 0, 0), result.data.startTime);
        }

        [TestMethod]
        public void BookAppointment_ShouldFail_WhenOverlapExists()
        {
            // Arrange
            var existingAppointment = new AppointmentsTblModel
            {
                appointmentID = 1,
                doctorID = 1,
                patientID = 99,
                date = new DateTime(2025, 10, 3),
                startTime = new TimeSpan(9, 0, 0),
                endTime = new TimeSpan(10, 0, 0),
                fee = 500,
                status = AppointmentStatus.Pending
            };

            var appointments = new List<AppointmentsTblModel> { existingAppointment }.AsQueryable();
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(appointments);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);

            var model = new BookAppointmentDTO
            {
                doctorID = 1,
                date = new DateTime(2025, 10, 3),
                startTime = new TimeSpan(9, 30, 0),
                endTime = new TimeSpan(10, 0, 0),
                fee = 600,
                chiefComplaint = "Back pain"
            };

            // Act
            var result = _service.BookAppointment(model, patientID: 10);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("This time slot is already booked.", result.message);
            Assert.IsNull(result.data);
        }

        [TestMethod]
        public void BookAppointment_ShouldHandleException()
        {
            // Arrange
            _mockContext.Setup(c => c.appointments_tbl).Throws(new Exception("DB error"));

            var model = new BookAppointmentDTO
            {
                doctorID = 1,
                date = new DateTime(2025, 10, 3),
                startTime = new TimeSpan(9, 0, 0),
                endTime = new TimeSpan(9, 30, 0),
                fee = 500,
                chiefComplaint = "Migraine"
            };

            // Act
            var result = _service.BookAppointment(model, patientID: 10);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "Error booking appointment");
        }

        [TestMethod]
        public void GetPatientAppointments_ShouldReturnAppointments_WhenFound()
        {
            // Arrange
            var patientId = 101;

            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel
                {
                    appointmentID = 1,
                    patientID = patientId,
                    doctorID = 201,
                    date = new DateTime(2025, 10, 10),
                    startTime = new TimeSpan(9, 0, 0),
                    endTime = new TimeSpan(9, 30, 0),
                    fee = 500,
                    status = AppointmentStatus.Pending
                },
                new AppointmentsTblModel
                {
                    appointmentID = 2,
                    patientID = patientId,
                    doctorID = 202,
                    date = new DateTime(2025, 9, 10),
                    startTime = new TimeSpan(14, 0, 0),
                    endTime = new TimeSpan(14, 30, 0),
                    fee = 700,
                    status = AppointmentStatus.Pending
                }
            }.AsQueryable();

            var doctors = new List<UserTblModel>
            {
                new UserTblModel { userID = 201, firstName = "John", lastName = "Smith", email = "john@doc.com", phoneNumber = "123456789" },
                new UserTblModel { userID = 202, firstName = "Jane", lastName = "Doe", email = "jane@doc.com", phoneNumber = "987654321" }
            }.AsQueryable();

            var mockAppointments = MockDbSetHelper.BuildMockDbSet(appointments);
            var mockUsers = MockDbSetHelper.BuildMockDbSet(doctors);

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);
            _mockContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);

            // Act
            var result = _service.GetPatientAppointments(patientId);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(2, result.data.Count());
            Assert.AreEqual("John Smith", result.data.First().doctorName);
            Assert.AreEqual("123456789", result.data.First().phoneNumber);
        }

        [TestMethod]
        public void GetPatientAppointments_ShouldReturnEmpty_WhenNoAppointments()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>().AsQueryable();
            var users = new List<UserTblModel>().AsQueryable();

            var mockAppointments = MockDbSetHelper.BuildMockDbSet(appointments);
            var mockUsers = MockDbSetHelper.BuildMockDbSet(users);

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);
            _mockContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);

            // Act
            var result = _service.GetPatientAppointments(999);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void GetPatientAppointments_ShouldReturnFail_OnException()
        {
            
            // Arrange
            var mockAppointments = new Mock<DbSet<AppointmentsTblModel>>();
            mockAppointments.As<IQueryable<AppointmentsTblModel>>().Setup(m => m.Provider).Throws(new Exception("DB error"));
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);

            // Act
            var result = _service.GetPatientAppointments(101);

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsTrue(result.message.StartsWith("Error retrieving patient appointments"));
        }

        [TestMethod]
        public void CancelAppointment_WhenAppointmentExists_ShouldCancelSuccessfully()
        {
            // Arrange
            var appointmentId = 1;
            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel
                {
                    appointmentID = appointmentId,
                    status = AppointmentStatus.Pending,
                    dateCreated = DateTime.Now
                }
            }.AsQueryable();

            var mockAppointments = MockDbSetHelper.BuildMockDbSet(appointments, args => appointments.FirstOrDefault(a => a.appointmentID == (int)args[0]));
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);

            // Act
            var result = _service.CancelAppointment(appointmentId);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Appointment cancelled successfully.", result.message);
            Assert.AreEqual(AppointmentStatus.Canceled, appointments.First().status);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void CancelAppointment_WhenAppointmentDoesNotExist_ShouldReturnFail()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>().AsQueryable();
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(appointments, args => null!); // No appointment found
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);

            // Act
            var result = _service.CancelAppointment(999);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Appointment not found.", result.message);
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }

        [TestMethod]
        public void CancelAppointment_WhenExceptionThrown_ShouldReturnFail()
        {
            // Arrange
            _mockContext.Setup(c => c.appointments_tbl.Find(It.IsAny<int>())).Throws(new Exception("DB error"));

            // Act
            var result = _service.CancelAppointment(1);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "Error cancelling appointment: DB error");
        }

        [TestMethod]
        public void GetDoctorAppointments_WhenAppointmentsExist_ShouldReturnAppointments()
        {
            // Arrange
            var doctorId = 10;
            var patient = new UserTblModel { userID = 1, firstName = "John", lastName = "Doe", email = "john@test.com" };

            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel { appointmentID = 1, doctorID = doctorId, patientID = 1, date = DateTime.Today, startTime = new TimeSpan(9,0,0), endTime = new TimeSpan(9,30,0), status = AppointmentStatus.Completed, transcriptFilePath = "/transcripts/file1.txt" }
            }.AsQueryable();

            var payments = new List<PaymentTblModel>
            {
                new PaymentTblModel { paymentID = 100, appointmentID = 1, imagePath = "/payments/img1.png" }
            }.AsQueryable();

            var users = new List<UserTblModel> { patient }.AsQueryable();

            var mockAppointments = MockDbSetHelper.BuildMockDbSet(appointments, args => appointments.FirstOrDefault(a => a.appointmentID == (int)args[0]));
            var mockPayments = MockDbSetHelper.BuildMockDbSet(payments, args => payments.FirstOrDefault(p => p.paymentID == (int)args[0]));
            var mockUsers = MockDbSetHelper.BuildMockDbSet(users, args => users.FirstOrDefault(u => u.userID == (int)args[0]));

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);
            _mockContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);

            // Act
            var result = _service.GetDoctorAppointments(doctorId);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(1, result.data.Count());
            var dto = result.data.First();
            Assert.AreEqual("John Doe", dto.patientName);
            Assert.AreEqual("/payments/img1.png", dto.paymentImage);
            Assert.AreEqual("/transcripts/file1.txt", dto.transcriptPath);
        }

        [TestMethod]
        public void GetDoctorAppointments_WhenNoAppointments_ShouldReturnEmpty()
        {
            // Arrange
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(new List<AppointmentsTblModel>().AsQueryable());
            var mockPayments = MockDbSetHelper.BuildMockDbSet(new List<PaymentTblModel>().AsQueryable());
            var mockUsers = MockDbSetHelper.BuildMockDbSet(new List<UserTblModel>().AsQueryable());

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);
            _mockContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);

            // Act
            var result = _service.GetDoctorAppointments(123);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void GetDoctorAppointments_WhenExceptionThrown_ShouldReturnFail()
        {
            // Arrange
            _mockContext.Setup(c => c.appointments_tbl).Throws(new Exception("DB error"));

            // Act
            var result = _service.GetDoctorAppointments(1);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "Error fetching doctor appointments: DB error");
        }

        [TestMethod]
        public void ApproveAppointment_WhenAppointmentExists_ShouldUpdateStatusAndReturnSuccess()
        {
            // Arrange
            var appointment = new AppointmentsTblModel
            {
                appointmentID = 1,
                doctorID = 10,
                patientID = 20,
                status = AppointmentStatus.Pending,
                dateCreated = DateTime.Now
            };

            var appointments = new List<AppointmentsTblModel> { appointment }.AsQueryable();
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(
                appointments,
                args => appointments.FirstOrDefault(a => a.appointmentID == (int)args[0])
            );

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);

            // Act
            var result = _service.ApproveAppointment(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Appointment approved and confirmation email sent.", result.message);
            Assert.AreEqual(AppointmentStatus.Approved, appointment.status);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void ApproveAppointment_WhenAppointmentNotFound_ShouldReturnFail()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>().AsQueryable();
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(
                appointments,
                args => null!
            );

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);

            // Act
            var result = _service.ApproveAppointment(123);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Appointment not found.", result.message);
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }

        [TestMethod]
        public void ApproveAppointment_WhenExceptionThrown_ShouldReturnFail()
        {
            // Arrange
            _mockContext.Setup(c => c.appointments_tbl.Find(It.IsAny<int>()))
                        .Throws(new Exception("DB error"));

            // Act
            var result = _service.ApproveAppointment(1);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "Error approving appointment: DB error");
        }

        [TestMethod]
        public void RejectAppointment_WhenAppointmentExists_ShouldUpdateStatusAndReturnSuccess()
        {
            // Arrange
            var appointment = new AppointmentsTblModel
            {
                appointmentID = 1,
                doctorID = 10,
                patientID = 20,
                status = AppointmentStatus.Pending,
                dateCreated = DateTime.Now
            };

            var appointments = new List<AppointmentsTblModel> { appointment }.AsQueryable();
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(
                appointments,
                args => appointments.FirstOrDefault(a => a.appointmentID == (int)args[0])
            );

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);

            // Act
            var result = _service.RejectAppointment(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Appointment rejected and notification email sent.", result.message);
            Assert.AreEqual(AppointmentStatus.Rejected, appointment.status);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void RejectAppointment_WhenAppointmentNotFound_ShouldReturnFail()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>().AsQueryable();
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(
                appointments,
                args => null!
            );

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);

            // Act
            var result = _service.RejectAppointment(999);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Appointment not found.", result.message);
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }

        [TestMethod]
        public void RejectAppointment_WhenExceptionThrown_ShouldReturnFail()
        {
            // Arrange
            _mockContext.Setup(c => c.appointments_tbl.Find(It.IsAny<int>()))
                        .Throws(new Exception("DB error"));

            // Act
            var result = _service.RejectAppointment(1);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "Error rejecting appointment: DB error");
        }
    }
}
