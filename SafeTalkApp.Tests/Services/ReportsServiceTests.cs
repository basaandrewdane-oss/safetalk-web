using Moq;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;
using System.Data.Entity;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class ReportsServiceTests
    {
        private Mock<ISafeTalkAppContext> _mockContext = null!;
        private ReportsService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<ISafeTalkAppContext>();
            _service = new ReportsService(_mockContext.Object);
        }

        [TestMethod]
        public void GetConsultationReport_ShouldReturnReport_WhenConsultationsExist()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>
        {
            new AppointmentsTblModel { appointmentID = 1, doctorID = 1, status = 6, date = new DateTime(2025, 1, 10) },
            new AppointmentsTblModel { appointmentID = 2, doctorID = 1, status = 6, date = new DateTime(2025, 1, 15) },
            new AppointmentsTblModel { appointmentID = 3, doctorID = 1, status = 6, date = new DateTime(2025, 2, 5) },
            new AppointmentsTblModel { appointmentID = 4, doctorID = 2, status = 6, date = new DateTime(2025, 1, 10) } // another doctor
        };

            var mockAppointmentsSet = MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointmentsSet.Object);

            // Act
            var result = _service.GetConsultationReport(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Report generated successfully.", result.message);
            var report = result.data.ToList();
            Assert.AreEqual(2, report.Count); // Jan and Feb
            Assert.AreEqual(2025, report[0].Year);
            Assert.AreEqual(1, report[0].Month);
            Assert.AreEqual(2, report[0].ConsultationCount); // 2 in Jan
            Assert.AreEqual(2025, report[1].Year);
            Assert.AreEqual(2, report[1].Month);
            Assert.AreEqual(1, report[1].ConsultationCount); // 1 in Feb
        }

        [TestMethod]
        public void GetConsultationReport_ShouldReturnEmpty_WhenNoConsultationsExist()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>(); // empty
            var mockAppointmentsSet = MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointmentsSet.Object);

            // Act
            var result = _service.GetConsultationReport(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Report generated successfully.", result.message);
            Assert.IsNotNull(result.data);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void GetConsultationReport_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange: create a mock DbSet that throws when enumerated
            var mockAppointmentsSet = new Mock<DbSet<AppointmentsTblModel>>();
            mockAppointmentsSet.As<IQueryable<AppointmentsTblModel>>().Setup(m => m.Provider).Throws(new Exception("DB error"));

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointmentsSet.Object);

            // Act
            var result = _service.GetConsultationReport(1);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred while generating the report");
            StringAssert.Contains(result.message, "DB error");
        }

        [TestMethod]
        public void GetPatientHistory_ShouldReturnAllPatients_WhenPatientIDIsNull()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 1, firstName = "John", lastName = "Doe", email = "john@example.com", phoneNumber = "123" },
                new UserTblModel { userID = 2, firstName = "Jane", lastName = "Smith", email = "jane@example.com", phoneNumber = "456" }
            };
            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel { appointmentID = 1, doctorID = 1, patientID = 1, status = 6 },
                new AppointmentsTblModel { appointmentID = 2, doctorID = 1, patientID = 2, status = 6 },
                new AppointmentsTblModel { appointmentID = 3, doctorID = 2, patientID = 1, status = 6 } // different doctor
            };

            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users.AsQueryable()).Object);
            _mockContext.Setup(c => c.appointments_tbl).Returns(MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable()).Object);

            // Act
            var result = _service.GetPatientHistory(null, 1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Patients retrieved successfully.", result.message);
            var patients = result.data.ToList();
            Assert.AreEqual(2, patients.Count);
            Assert.IsTrue(patients.Any(p => p.PatientID == 1));
            Assert.IsTrue(patients.Any(p => p.PatientID == 2));
        }

        [TestMethod]
        public void GetPatientHistory_ShouldReturnHistory_WhenPatientIDIsProvided()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 1, firstName = "John", lastName = "Doe", email = "john@example.com", phoneNumber = "123" }
            };
            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel { appointmentID = 1, doctorID = 1, patientID = 1, status = 6, date = new DateTime(2025, 1, 10), startTime = new TimeSpan(10,0,0), endTime = new TimeSpan(11,0,0) },
                new AppointmentsTblModel { appointmentID = 2, doctorID = 1, patientID = 1, status = 5, date = new DateTime(2025, 2, 5), startTime = new TimeSpan(12,0,0), endTime = new TimeSpan(13,0,0) }
            };

            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users.AsQueryable()).Object);
            _mockContext.Setup(c => c.appointments_tbl).Returns(MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable()).Object);

            // Act
            var result = _service.GetPatientHistory(1, 1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Patient history retrieved successfully.", result.message);
            var history = result.data.ToList();
            Assert.AreEqual(2, history.Count);
            Assert.AreEqual(new DateTime(2025, 2, 5), history[0].Date); // latest first
            Assert.AreEqual("12:00:00-13:00:00", history[0].Time);
        }

        [TestMethod]
        public void GetPatientHistory_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange: create a mock DbSet that throws when enumerated
            var mockAppointmentsSet = new Mock<DbSet<AppointmentsTblModel>>();
            mockAppointmentsSet.As<IQueryable<AppointmentsTblModel>>()
                .Setup(m => m.Provider)
                .Throws(new Exception("DB error"));

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointmentsSet.Object);

            // Act
            var result = _service.GetPatientHistory(1, 1);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred while retrieving patient history");
            StringAssert.Contains(result.message, "DB error");
        }

        [TestMethod]
        public void GetDoctorHistory_ShouldReturnAllDoctors_WhenDoctorIDIsNull()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 1, firstName = "Dr. John", lastName = "Doe", email = "drjohn@example.com", phoneNumber = "123" },
                new UserTblModel { userID = 2, firstName = "Dr. Jane", lastName = "Smith", email = "drjane@example.com", phoneNumber = "456" }
            };
            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel { appointmentID = 1, doctorID = 1, patientID = 1, status = 6 },
                new AppointmentsTblModel { appointmentID = 2, doctorID = 2, patientID = 1, status = 6 },
                new AppointmentsTblModel { appointmentID = 3, doctorID = 1, patientID = 2, status = 6 } // different patient
            };

            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users.AsQueryable()).Object);
            _mockContext.Setup(c => c.appointments_tbl).Returns(MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable()).Object);

            // Act
            var result = _service.GetDoctorHistory(null, 1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Doctors retrieved successfully.", result.message);
            var doctors = result.data.ToList();
            Assert.AreEqual(2, doctors.Count);
            Assert.IsTrue(doctors.Any(d => d.DoctorID == 1));
            Assert.IsTrue(doctors.Any(d => d.DoctorID == 2));
        }

        [TestMethod]
        public void GetDoctorHistory_ShouldReturnHistory_WhenDoctorIDIsProvided()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 1, firstName = "Dr. John", lastName = "Doe", email = "drjohn@example.com", phoneNumber = "123" }
            };
            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel
                {
                    appointmentID = 1,
                    doctorID = 1,
                    patientID = 1,
                    status = 6,
                    date = new DateTime(2025, 1, 10),
                    startTime = new TimeSpan(10,0,0),
                    endTime = new TimeSpan(11,0,0)
                },
                new AppointmentsTblModel
                {
                    appointmentID = 2,
                    doctorID = 1,
                    patientID = 1,
                    status = 5,
                    date = new DateTime(2025, 2, 5),
                    startTime = new TimeSpan(12,0,0),
                    endTime = new TimeSpan(13,0,0)
                }
            };

            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users.AsQueryable()).Object);
            _mockContext.Setup(c => c.appointments_tbl).Returns(MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable()).Object);

            // Act
            var result = _service.GetDoctorHistory(1, 1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Doctor history retrieved successfully.", result.message);
            var history = result.data.ToList();
            Assert.AreEqual(2, history.Count);
            Assert.AreEqual(new DateTime(2025, 2, 5), history[0].Date); // latest first
            Assert.AreEqual("12:00:00-13:00:00", history[0].Time);
        }

        [TestMethod]
        public void GetDoctorHistory_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange: create a mock DbSet that throws when enumerated
            var mockAppointmentsSet = new Mock<DbSet<AppointmentsTblModel>>();
            mockAppointmentsSet.As<IQueryable<AppointmentsTblModel>>()
                .Setup(m => m.Provider)
                .Throws(new Exception("DB error"));

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointmentsSet.Object);

            // Act
            var result = _service.GetDoctorHistory(1, 1);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred while retrieving doctor history");
            StringAssert.Contains(result.message, "DB error");
        }

        [TestMethod]
        public void GetMissedAppointments_ShouldReturnAppointments_WhenUserIsPatient()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 1, firstName = "John", lastName = "Doe" },
                new UserTblModel { userID = 2, firstName = "Dr. Jane", lastName = "Smith" }
            };
            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel
                {
                    appointmentID = 1,
                    patientID = 1,
                    doctorID = 2,
                    status = 7, // missed
                    date = new DateTime(2025, 1, 10),
                    startTime = new TimeSpan(10,0,0),
                    endTime = new TimeSpan(11,0,0)
                },
                new AppointmentsTblModel
                {
                    appointmentID = 2,
                    patientID = 3,
                    doctorID = 2,
                    status = 7
                }
            };

            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users.AsQueryable()).Object);
            _mockContext.Setup(c => c.appointments_tbl).Returns(MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable()).Object);

            // Act
            var result = _service.GetMissedAppointments(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Missed appointments retrieved successfully.", result.message);
            var data = result.data.ToList();
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual("John Doe", data[0].patientName);
            Assert.AreEqual("Dr. Jane Smith", data[0].doctorName);
        }

        [TestMethod]
        public void GetMissedAppointments_ShouldReturnAppointments_WhenUserIsDoctor()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 1, firstName = "John", lastName = "Doe" },
                new UserTblModel { userID = 2, firstName = "Dr. Jane", lastName = "Smith" }
            };
            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel
                {
                    appointmentID = 1,
                    patientID = 1,
                    doctorID = 2,
                    status = 7, // missed
                    date = new DateTime(2025, 1, 10),
                    startTime = new TimeSpan(10,0,0),
                    endTime = new TimeSpan(11,0,0)
                }
            };

            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users.AsQueryable()).Object);
            _mockContext.Setup(c => c.appointments_tbl).Returns(MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable()).Object);

            // Act
            var result = _service.GetMissedAppointments(2);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(1, result.data.Count());
            var appointment = result.data.First();
            Assert.AreEqual("John Doe", appointment.patientName);
            Assert.AreEqual("Dr. Jane Smith", appointment.doctorName);
        }

        [TestMethod]
        public void GetMissedAppointments_ShouldReturnEmpty_WhenNoMissedAppointments()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 1, firstName = "John", lastName = "Doe" }
            };
            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel
                {
                    appointmentID = 1,
                    patientID = 1,
                    doctorID = 2,
                    status = 6 // not missed
                }
            };

            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users.AsQueryable()).Object);
            _mockContext.Setup(c => c.appointments_tbl).Returns(MockDbSetHelper.BuildMockDbSet(appointments.AsQueryable()).Object);

            // Act
            var result = _service.GetMissedAppointments(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void GetMissedAppointments_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange: create a mock DbSet that throws when enumerated
            var mockAppointmentsSet = new Mock<DbSet<AppointmentsTblModel>>();
            mockAppointmentsSet.As<IQueryable<AppointmentsTblModel>>()
                .Setup(m => m.Provider)
                .Throws(new Exception("DB error"));

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointmentsSet.Object);

            // Act
            var result = _service.GetMissedAppointments(1);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred while retrieving missed appointments");
        }

    }
}
