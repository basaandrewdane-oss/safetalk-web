using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using SafeTalkApp.Interfaces;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;
using System.Data.Entity;
using System.Web;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class PaymentServiceTests
    {
        private Mock<ISafeTalkAppContext> _mockContext = null!;
        private Mock<IPayPalService> _payPalService = null!;
        private Mock<IEmailService> _emailService = null!;
        private Mock<IFileStorageService> _fileStorage = null!;
        private Mock<IDateTimeProvider> _time = null!;
        private Mock<ILogger<PaymentService>> _logger = null!;
        private PaymentService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<ISafeTalkAppContext>();
            _payPalService = new Mock<IPayPalService>();
            _emailService = new Mock<IEmailService>();
            _fileStorage = new Mock<IFileStorageService>();
            _time = new Mock<IDateTimeProvider>();
            _logger = new Mock<ILogger<PaymentService>>();

            _service = new PaymentService(_mockContext.Object, _payPalService.Object, _emailService.Object,
                _fileStorage.Object, _time.Object, _logger.Object);

        }

        [TestMethod]
        public void SubmitPayment_ShouldSucceed_WhenValidInput()
        {
            // Arrange appointment
            var appointment = new AppointmentsTblModel
            {
                appointmentID = 1,
                fee = 500,
                status = AppointmentStatus.Pending
            };

            var appointments = new[] { appointment }.AsQueryable();
            var mockApptSet = MockDbSetHelper.BuildMockDbSet(appointments);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockApptSet.Object);

            // Prepare payment_tbl
            var payments = new List<PaymentTblModel>().AsQueryable();
            var mockPaySet = MockDbSetHelper.BuildMockDbSet(payments);
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPaySet.Object);

            // Mock file
            var mockFile = new Mock<HttpPostedFileBase>();
            mockFile.Setup(f => f.ContentLength).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("proof.jpg");

            var adminUser = new UserTblModel { userID = 10, email = "admin@test.com" };
            var patient = new UserTblModel { userID = 20, email = "patient@test.com" };
            var doctor = new UserTblModel { userID = 30, email = "doctor@test.com" };

            var users = new List<UserTblModel> { adminUser, patient, doctor }.AsQueryable();
            var mockUserSet = MockDbSetHelper.BuildMockDbSet(users);
            _mockContext.Setup(c => c.user_tbl).Returns(mockUserSet.Object);

            var roles = new List<UserRoleTblModel>
            {
                new UserRoleTblModel { userID = 10, roleID = 3 } // admin role
            }.AsQueryable();

            var mockRoleSet = MockDbSetHelper.BuildMockDbSet(roles);
            _mockContext.Setup(c => c.user_role_tbl).Returns(mockRoleSet.Object);

            // FileStorage returns a path
            _fileStorage.Setup(f => f.SavePaymentProof(It.IsAny<HttpPostedFileBase>()))
                        .Returns("uploads/payments/proof.jpg");

            // Time provider
            var now = DateTime.UtcNow;
            _time.Setup(t => t.Now).Returns(now);

            // Act
            var result = _service.SubmitPayment(1, mockFile.Object);

            // Assert response
            Assert.IsTrue(result.success);
            Assert.AreEqual("Payment Submitted", result.message);

            // Assert payment was added
            Assert.AreEqual(1, mockPaySet.Object.Count());

            var savedPayment = mockPaySet.Object.First();
            Assert.AreEqual(1, savedPayment.appointmentID);
            Assert.AreEqual("uploads/payments/proof.jpg", savedPayment.imagePath);
            Assert.AreEqual(PaymentStatus.Pending, savedPayment.status);
            Assert.AreEqual(500, savedPayment.amount);

            // Assert appointment updated
            Assert.AreEqual(AppointmentStatus.PaymentSubmitted, appointment.status);

            // SaveChanges must have been called
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);

            // Email should have been sent
            _emailService.Verify(e =>
                e.SendPaymentSubmittedEmail(
                    It.IsAny<UserTblModel>(),          // admin
                    It.IsAny<AppointmentsTblModel>(),  // appointment
                    It.IsAny<PaymentTblModel>(),       // payment
                    It.IsAny<UserTblModel>(),          // patient
                    It.IsAny<UserTblModel>()           // doctor
                ),
                Times.Once
            );
        }

        [TestMethod]
        public void SubmitPayment_ReturnsFail_WhenAppointmentNotFound()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>().AsQueryable();
            var mockAppointmentSet = MockDbSetHelper.BuildMockDbSet(appointments);

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointmentSet.Object);

            // Act
            var result = _service.SubmitPayment(123, Mock.Of<HttpPostedFileBase>());

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Appointment not found.", result.message);
        }

        [TestMethod]
        public void SubmitPayment_ReturnsFail_WhenNoPaymentProof()
        {
            // Arrange
            var data = new[]
            {
                new AppointmentsTblModel { appointmentID = 1, fee = 500 }
            }.AsQueryable();

            var mockAppointmentSet = MockDbSetHelper.BuildMockDbSet(data);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointmentSet.Object);

            // Act
            var result = _service.SubmitPayment(1, null);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("No payment proof uploaded.", result.message);
        }

        [TestMethod]
        public void SubmitPayment_ReturnsFail_WhenInvalidFileType()
        {
            // Arrange
            var data = new[]
            {
                new AppointmentsTblModel { appointmentID = 1, fee = 500 }
            }.AsQueryable();

            var mockAppointmentSet = MockDbSetHelper.BuildMockDbSet(data);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointmentSet.Object);

            // Mock file (.exe → invalid)
            var mockFile = new Mock<HttpPostedFileBase>();
            mockFile.Setup(f => f.ContentLength).Returns(10);
            mockFile.Setup(f => f.FileName).Returns("virus.exe");

            // Act
            var result = _service.SubmitPayment(1, mockFile.Object);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Invalid file type.", result.message);
        }

        [TestMethod]
        public void SubmitPayment_ReturnsFail_WhenExceptionThrown()
        {
            // Arrange - throw exception on .Find()
            var mockApptSet = new Mock<DbSet<AppointmentsTblModel>>();
            mockApptSet.Setup(m => m.Find(It.IsAny<object[]>())).Throws(new Exception("DB error"));

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockApptSet.Object);

            // Act
            var result = _service.SubmitPayment(1, Mock.Of<HttpPostedFileBase>());

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsTrue(result.message.Contains("Problem submitting payment"));

            // Logger should record the exception
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }

        [TestMethod]
        public void CreatePayPalOrder_ReturnsFail_WhenAppointmentNotFound()
        {
            // Arrange: Empty table
            var appointments = new List<AppointmentsTblModel>().AsQueryable();
            var mockApptSet = MockDbSetHelper.BuildMockDbSet(appointments);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockApptSet.Object);

            // Act
            var result = _service.CreatePayPalOrder(1, "return", "cancel");

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Appointment not found.", result.message);
        }

        [TestMethod]
        public void CreatePayPalOrder_ShouldReturnApprovalLink_WhenSuccessful()
        {
            // Arrange
            var appointment = new AppointmentsTblModel
            {
                appointmentID = 1,
                fee = 500
            };

            var appointments = new[] { appointment }.AsQueryable();
            var mockApptSet = MockDbSetHelper.BuildMockDbSet(appointments);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockApptSet.Object);

            // Mock PayPal response structure
            // Correct JObject response
            var paypalResponse = JObject.FromObject(new
            {
                links = new[]
                {
                    new
                    {
                        rel = "approve",
                        href = "https://paypal.com/approve123"
                    }
                }
            });

            _payPalService
                .Setup(p => p.CreateOrder(500, "returnURL", "cancelURL", 1))
                .Returns(paypalResponse);

            // Act
            var result = _service.CreatePayPalOrder(1, "returnURL", "cancelURL");

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("https://paypal.com/approve123", result.data);
        }

        [TestMethod]
        public void CreatePayPalOrder_ReturnsFail_WhenPayPalThrowsException()
        {
            // Arrange
            var appointment = new AppointmentsTblModel
            {
                appointmentID = 1,
                fee = 500
            };

            var appointments = new[] { appointment }.AsQueryable();
            var mockApptSet = MockDbSetHelper.BuildMockDbSet(appointments);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockApptSet.Object);

            _payPalService
                .Setup(p => p.CreateOrder(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Throws(new Exception("PayPal offline"));

            // Act
            var result = _service.CreatePayPalOrder(1, "ok", "cancel");

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsTrue(result.message.Contains("There was a problem in creating order"));
            Assert.IsTrue(result.message.Contains("PayPal offline"));

            // Verify logger was called
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }

        [TestMethod]
        public void CreatePayPalOrder_ReturnsFail_WhenApprovalLinkMissing()
        {
            // Arrange
            var appointment = new AppointmentsTblModel
            {
                appointmentID = 1,
                fee = 500
            };

            var appointments = new[] { appointment }.AsQueryable();
            var mockApptSet = MockDbSetHelper.BuildMockDbSet(appointments);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockApptSet.Object);

            // Correct JObject response (BUT missing "approve")
            var paypalResponse = JObject.FromObject(new
            {
                links = new[]
                {
                    new
                    {
                        rel = "self",
                        href = "https://paypal.com/self"
                    }
                }
            });

            _payPalService
                .Setup(p => p.CreateOrder(500, "ret", "cancel", 1))
                .Returns(paypalResponse);

            // Act
            var result = _service.CreatePayPalOrder(1, "ret", "cancel");

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsTrue(result.message.Contains("There was a problem"));
        }

        [TestMethod]
        public void ReviewPayPalOrder_ReturnsFail_WhenAppointmentNotFound()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>().AsQueryable();
            var mockApptSet = MockDbSetHelper.BuildMockDbSet(appointments);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockApptSet.Object);

            var users = new List<UserTblModel>().AsQueryable();
            var mockUserSet = MockDbSetHelper.BuildMockDbSet(users);
            _mockContext.Setup(c => c.user_tbl).Returns(mockUserSet.Object);

            // Act
            var result = _service.ReviewPayPalOrder("tok123", 99);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Appointment not found.", result.message);
        }

        [TestMethod]
        public void ReviewPayPalOrder_ShouldReturnReviewDTO_WhenValid()
        {
            // Arrange appointment
            var appointment = new AppointmentsTblModel
            {
                appointmentID = 1,
                doctorID = 10,
                fee = 800,
                date = new DateTime(2025, 1, 10),
                startTime = new TimeSpan(10, 0, 0),
                endTime = new TimeSpan(11, 0, 0)
            };

            var appointments = new[] { appointment }.AsQueryable();
            var mockApptSet = MockDbSetHelper.BuildMockDbSet(appointments);
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockApptSet.Object);

            // Arrange doctor
            var doctor = new UserTblModel
            {
                userID = 10,
                firstName = "John",
                lastName = "Doe"
            };

            var users = new[] { doctor }.AsQueryable();
            var mockUserSet = MockDbSetHelper.BuildMockDbSet(users);
            _mockContext.Setup(c => c.user_tbl).Returns(mockUserSet.Object);

            // Act
            var result = _service.ReviewPayPalOrder("tokABC", 1);

            // Assert success
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);

            // Assert DTO values
            Assert.AreEqual("tokABC", result.data.Token);
            Assert.AreEqual(1, result.data.AppointmentID);
            Assert.AreEqual(800, result.data.Fee);
            Assert.AreEqual("John Doe", result.data.DoctorName);
            Assert.AreEqual(new DateTime(2025, 1, 10), result.data.Date);
            Assert.AreEqual("10:00:00 - 11:00:00", result.data.Time);
        }

        [TestMethod]
        public void ReviewPayPalOrder_ShouldHandleMissingDoctorName()
        {
            // Arrange appointment
            var appointment = new AppointmentsTblModel
            {
                appointmentID = 1,
                doctorID = 999,  // doctor does NOT exist
                fee = 500,
                date = new DateTime(2025, 2, 5),
                startTime = new TimeSpan(10, 0, 0),
                endTime = new TimeSpan(11, 0, 0)
            };

            var mockApptSet = MockDbSetHelper.BuildMockDbSet(new[] { appointment }.AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockApptSet.Object);

            // No users in user_tbl
            var mockUserSet = MockDbSetHelper.BuildMockDbSet(new List<UserTblModel>().AsQueryable());
            _mockContext.Setup(c => c.user_tbl).Returns(mockUserSet.Object);

            // Act
            var result = _service.ReviewPayPalOrder("tok678", 1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNull(result.data.DoctorName);
        }

        [TestMethod]
        public void ReviewPayPalOrder_ReturnsFail_WhenExceptionThrown()
        {
            // Arrange — force exception from Find()
            var mockApptSet = new Mock<DbSet<AppointmentsTblModel>>();
            mockApptSet.Setup(s => s.Find(It.IsAny<object[]>()))
                       .Throws(new Exception("DB failure"));

            _mockContext.Setup(c => c.appointments_tbl).Returns(mockApptSet.Object);

            var mockUserSet = MockDbSetHelper.BuildMockDbSet(new List<UserTblModel>().AsQueryable());
            _mockContext.Setup(c => c.user_tbl).Returns(mockUserSet.Object);

            // Act
            var result = _service.ReviewPayPalOrder("tok999", 1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsTrue(result.message.Contains("There was a problem reviewing payment order"));
            Assert.IsTrue(result.message.Contains("DB failure"));
        }

        [TestMethod]
        public void CapturePayPalOrder_ReturnsFail_WhenStatusNotCompleted()
        {
            // Arrange
            var paypalResponse = JObject.FromObject(new
            {
                status = "PENDING"
            });

            _payPalService
                .Setup(p => p.CaptureOrder("tok123"))
                .Returns(paypalResponse);

            // Act
            var result = _service.CapturePayPalOrder("tok123", 1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Payment not Completed", result.message);
        }

        [TestMethod]
        public void CapturePayPalOrder_ReturnsFail_WhenReferenceIdMismatch()
        {
            // Arrange PayPal response (COMPLETED but wrong reference_id)
            var paypalResponse = JObject.FromObject(new
            {
                status = "COMPLETED",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = "999", // wrong ID
                        payments = new
                        {
                            captures = new[]
                            {
                                new
                                {
                                    id = "TX123",
                                    amount = new
                                    {
                                        value = "800",
                                        currency_code = "USD"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            _payPalService
                .Setup(p => p.CaptureOrder("tokABC"))
                .Returns(paypalResponse);

            // Act
            var result = _service.CapturePayPalOrder("tokABC", 1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Appointment ID mismatch", result.message);
        }

        [TestMethod]
        public void CapturePayPalOrder_ReturnsFail_WhenPaymentAlreadyExists()
        {
            // Arrange PayPal response (COMPLETED)
            var paypalResponse = JObject.FromObject(new
            {
                status = "COMPLETED",
                purchase_units = new[]
                {
            new
            {
                reference_id = "1",
                payments = new
                {
                    captures = new[]
                    {
                        new
                        {
                            id = "TX999",
                            amount = new
                            {
                                value = "500",
                                currency_code = "USD"
                            }
                        }
                    }
                }
            }
        }
            });

            _payPalService
                .Setup(p => p.CaptureOrder("tok777"))
                .Returns(paypalResponse);

            // Existing payment with same transactionId
            var existingPayment = new PaymentTblModel { transactionId = "TX999" };

            var payments = new[] { existingPayment }.AsQueryable();
            var mockPaymentSet = MockDbSetHelper.BuildMockDbSet(payments);
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPaymentSet.Object);

            // Appointment table (can be empty for this test)
            var mockAppts = MockDbSetHelper.BuildMockDbSet(new List<AppointmentsTblModel>().AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppts.Object);

            // Act
            var result = _service.CapturePayPalOrder("tok777", 1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Payment already processed.", result.message);
        }

        [TestMethod]
        public void CapturePayPalOrder_ReturnsFail_WhenAppointmentNotFound()
        {
            // Arrange PayPal response (COMPLETED)
            var paypalResponse = JObject.FromObject(new
            {
                status = "COMPLETED",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = "1",
                        payments = new
                        {
                            captures = new[]
                            {
                                new
                                {
                                    id = "TX321",
                                    amount = new
                                    {
                                        value = "400",
                                        currency_code = "USD"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            _payPalService
                .Setup(p => p.CaptureOrder("tok1"))
                .Returns(paypalResponse);

            // Empty payment table
            _mockContext.Setup(c => c.payment_tbl)
                .Returns(MockDbSetHelper.BuildMockDbSet(new List<PaymentTblModel>().AsQueryable()).Object);

            // Empty appointment table
            _mockContext.Setup(c => c.appointments_tbl)
                .Returns(MockDbSetHelper.BuildMockDbSet(new List<AppointmentsTblModel>().AsQueryable()).Object);

            // Act
            var result = _service.CapturePayPalOrder("tok1", 1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Appointment not found.", result.message);
        }

        [TestMethod]
        public void CapturePayPalOrder_ReturnsSuccess_WhenValid()
        {
            // Arrange PayPal captured response
            var paypalResponse = JObject.FromObject(new
            {
                status = "COMPLETED",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = "1",
                        payments = new
                        {
                            captures = new[]
                            {
                                new
                                {
                                    id = "TX123",
                                    amount = new
                                    {
                                        value = "350",
                                        currency_code = "USD"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            _payPalService
                .Setup(p => p.CaptureOrder("tokOK"))
                .Returns(paypalResponse);

            // Appointment exists
            var appointment = new AppointmentsTblModel { appointmentID = 1, doctorID = 10, patientID = 20 };
            var mockAppts = MockDbSetHelper.BuildMockDbSet(new[] { appointment }.AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppts.Object);

            // No payments yet
            var mockPayments = MockDbSetHelper.BuildMockDbSet(new List<PaymentTblModel>().AsQueryable());
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);

            // Doctor + Patient users
            var users = new[]
            {
                new UserTblModel { userID = 10, firstName = "Doc", lastName = "Smith" },
                new UserTblModel { userID = 20, firstName = "Pat", lastName = "Jones" }
            }.AsQueryable();
            var mockUsers = MockDbSetHelper.BuildMockDbSet(users);
            _mockContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);

            // Act
            var result = _service.CapturePayPalOrder("tokOK", 1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("TX123", result.data.TransactionId);
            Assert.AreEqual("350", result.data.Amount);
            Assert.AreEqual("USD", result.data.Currency);
            Assert.AreEqual(1, result.data.AppointmentID);

            // Payment added
            Assert.AreEqual(1, mockPayments.Object.Count());

            // SaveChanges called
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void CapturePayPalOrder_ShouldStillSucceed_WhenEmailSendingFails()
        {
            // Arrange PayPal completed response
            var paypalResponse = JObject.FromObject(new
            {
                status = "COMPLETED",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = "1",
                        payments = new
                        {
                            captures = new[]
                            {
                                new
                                {
                                    id = "TX555",
                                    amount = new
                                    {
                                        value = "600",
                                        currency_code = "USD"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            _payPalService.Setup(p => p.CaptureOrder("tokERR")).Returns(paypalResponse);

            // Appointment
            var appointment = new AppointmentsTblModel { appointmentID = 1, doctorID = 10, patientID = 20 };
            _mockContext.Setup(c => c.appointments_tbl)
                .Returns(MockDbSetHelper.BuildMockDbSet(new[] { appointment }.AsQueryable()).Object);

            // Empty payments
            _mockContext.Setup(c => c.payment_tbl)
                .Returns(MockDbSetHelper.BuildMockDbSet(new List<PaymentTblModel>().AsQueryable()).Object);

            // Doctor + patient
            var users = new[]
            {
                new UserTblModel { userID = 10 },
                new UserTblModel { userID = 20 }
            }.AsQueryable();
            _mockContext.Setup(c => c.user_tbl)
                .Returns(MockDbSetHelper.BuildMockDbSet(users).Object);

            // Force email exception
            _emailService.Setup(e => e.SendPayPalPaymentConfirmationToPatient(
                It.IsAny<UserTblModel>(),
                It.IsAny<UserTblModel>(),
                It.IsAny<AppointmentsTblModel>(),
                It.IsAny<PaymentTblModel>()
            )).Throws(new Exception("SMTP DOWN"));

            // Act
            var result = _service.CapturePayPalOrder("tokERR", 1);

            // Assert — STILL SUCCESS
            Assert.IsTrue(result.success);
            Assert.AreEqual("TX555", result.data.TransactionId);
        }

        [TestMethod]
        public void CapturePayPalOrder_ReturnsFail_WhenPayPalThrowsError()
        {
            // Arrange
            _payPalService.Setup(p => p.CaptureOrder("tokX"))
                .Throws(new Exception("PayPal Server Error"));

            // Act
            var result = _service.CapturePayPalOrder("tokX", 1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsTrue(result.message.Contains("PayPal Server Error"));
        }

        [TestMethod]
        public void VerifyPayment_ReturnsFail_WhenPaymentNotFound()
        {
            // Arrange empty payment table
            var mockPayments = MockDbSetHelper.BuildMockDbSet(new List<PaymentTblModel>().AsQueryable());
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);

            // Act
            var result = _service.VerifyPayment(1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Payment not found.", result.message);
        }

        [TestMethod]
        public void VerifyPayment_ShouldMarkPaymentAndAppointmentAsPaid_WhenValid()
        {
            // Arrange payment
            var payment = new PaymentTblModel
            {
                paymentID = 1,
                appointmentID = 1,
                status = PaymentStatus.Pending
            };

            var mockPayments = MockDbSetHelper.BuildMockDbSet(new[] { payment }.AsQueryable());
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);

            // Arrange appointment
            var appointment = new AppointmentsTblModel
            {
                appointmentID = 1,
                patientID = 10,
                doctorID = 20,
                status = AppointmentStatus.PaymentSubmitted
            };
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(new[] { appointment }.AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);

            // Arrange users
            var patient = new UserTblModel { userID = 10 };
            var doctor = new UserTblModel { userID = 20 };
            var mockUsers = MockDbSetHelper.BuildMockDbSet(new[] { patient, doctor }.AsQueryable());
            _mockContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);

            // Act
            var result = _service.VerifyPayment(1);

            // Assert response
            Assert.IsTrue(result.success);
            Assert.AreEqual("Payment Verified", result.message);

            // Assert payment updated
            Assert.AreEqual(PaymentStatus.Completed, payment.status);

            // Assert appointment updated
            Assert.AreEqual(AppointmentStatus.Paid, appointment.status);

            // Verify SaveChanges called at least once
            _mockContext.Verify(c => c.SaveChanges(), Times.AtLeast(1));

            // Verify emails sent
            _emailService.Verify(e => e.SendPaymentVerifiedEmailToPatient(patient, doctor, appointment, payment), Times.Once);
            _emailService.Verify(e => e.SendPaymentVerifiedEmailToDoctor(doctor, patient, appointment, payment), Times.Once);
        }

        [TestMethod]
        public void VerifyPayment_ShouldHandleNullAppointment()
        {
            // Arrange payment
            var payment = new PaymentTblModel
            {
                appointmentID = 1,
                status = PaymentStatus.Pending
            };
            var mockPayments = MockDbSetHelper.BuildMockDbSet(new[] { payment }.AsQueryable());
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);

            // No appointment in DB
            _mockContext.Setup(c => c.appointments_tbl).Returns(MockDbSetHelper.BuildMockDbSet(new List<AppointmentsTblModel>().AsQueryable()).Object);

            // Act
            var result = _service.VerifyPayment(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(PaymentStatus.Completed, payment.status);

            // Verify emails never called
            _emailService.Verify(e => e.SendPaymentVerifiedEmailToPatient(It.IsAny<UserTblModel>(), It.IsAny<UserTblModel>(), It.IsAny<AppointmentsTblModel>(), It.IsAny<PaymentTblModel>()), Times.Never);
            _emailService.Verify(e => e.SendPaymentVerifiedEmailToDoctor(It.IsAny<UserTblModel>(), It.IsAny<UserTblModel>(), It.IsAny<AppointmentsTblModel>(), It.IsAny<PaymentTblModel>()), Times.Never);
        }

        [TestMethod]
        public void VerifyPayment_ShouldStillSucceed_WhenEmailThrowsException()
        {
            // Arrange payment
            var payment = new PaymentTblModel { appointmentID = 1, status = PaymentStatus.Pending };
            var mockPayments = MockDbSetHelper.BuildMockDbSet(new[] { payment }.AsQueryable());
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);

            // Appointment exists
            var appointment = new AppointmentsTblModel { appointmentID = 1, patientID = 10, doctorID = 20 };
            _mockContext.Setup(c => c.appointments_tbl).Returns(MockDbSetHelper.BuildMockDbSet(new[] { appointment }.AsQueryable()).Object);

            // Users exist
            var patient = new UserTblModel { userID = 10 };
            var doctor = new UserTblModel { userID = 20 };
            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet<UserTblModel>(new[] { patient, doctor }.AsQueryable()).Object);


            // Force email exception
            _emailService.Setup(e => e.SendPaymentVerifiedEmailToPatient(patient, doctor, appointment, payment))
                         .Throws(new Exception("SMTP down"));

            _emailService.Setup(e => e.SendPaymentVerifiedEmailToDoctor(doctor, patient, appointment, payment))
                         .Throws(new Exception("SMTP down"));

            // Act
            var result = _service.VerifyPayment(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(PaymentStatus.Completed, payment.status);
            Assert.AreEqual(AppointmentStatus.Paid, appointment.status);
        }

        [TestMethod]
        public void VerifyPayment_ReturnsFail_WhenExceptionThrown()
        {
            // Arrange — force exception when accessing payment_tbl
            var mockPayments = new Mock<DbSet<PaymentTblModel>>();

            // Make the IQueryable provider throw when enumerated
            mockPayments.As<IQueryable<PaymentTblModel>>()
                .Setup(m => m.Provider)
                .Throws(new Exception("DB error"));

            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);

            // Act
            var result = _service.VerifyPayment(1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsTrue(result.message.Contains("DB error"));
        }

        [TestMethod]
        public void RejectPayment_ReturnsFail_WhenPaymentNotFound()
        {
            // Arrange empty payment table
            var mockPayments = MockDbSetHelper.BuildMockDbSet(new List<PaymentTblModel>().AsQueryable());
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);

            // Act
            var result = _service.RejectPayment(1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Payment not found.", result.message);
        }

        [TestMethod]
        public void RejectPayment_ShouldMarkPaymentAndAppointmentAsRejected_WhenValid()
        {
            // Arrange payment
            var payment = new PaymentTblModel
            {
                appointmentID = 1,
                status = PaymentStatus.Pending
            };
            var mockPayments = MockDbSetHelper.BuildMockDbSet(new[] { payment }.AsQueryable());
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);

            // Arrange appointment
            var appointment = new AppointmentsTblModel
            {
                appointmentID = 1,
                patientID = 10,
                doctorID = 20,
                status = AppointmentStatus.PaymentSubmitted
            };
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(new[] { appointment }.AsQueryable());
            _mockContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);

            // Arrange users
            var patient = new UserTblModel { userID = 10 };
            var doctor = new UserTblModel { userID = 20 };
            var mockUsers = MockDbSetHelper.BuildMockDbSet(new[] { patient, doctor }.AsQueryable());
            _mockContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);

            // Act
            var result = _service.RejectPayment(1);

            // Assert response
            Assert.IsTrue(result.success);
            Assert.AreEqual("Payment Verified", result.message); // Note: your function returns "Payment Verified" even for rejection

            // Assert payment updated
            Assert.AreEqual(PaymentStatus.Failed, payment.status);

            // Assert appointment updated
            Assert.AreEqual(AppointmentStatus.Rejected, appointment.status);

            // Verify SaveChanges called at least once
            _mockContext.Verify(c => c.SaveChanges(), Times.AtLeast(1));

            // Verify emails sent
            _emailService.Verify(e => e.SendPaymentRejectedEmailToPatient(patient, doctor, appointment, payment), Times.Once);
            _emailService.Verify(e => e.SendPaymentRejectedEmailToDoctor(doctor, patient, appointment, payment), Times.Once);
        }

        [TestMethod]
        public void RejectPayment_ShouldHandleNullAppointment()
        {
            // Arrange payment
            var payment = new PaymentTblModel { appointmentID = 1, status = PaymentStatus.Pending };
            var mockPayments = MockDbSetHelper.BuildMockDbSet(new[] { payment }.AsQueryable());
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);

            // No appointment in DB
            _mockContext.Setup(c => c.appointments_tbl)
                .Returns(MockDbSetHelper.BuildMockDbSet(new List<AppointmentsTblModel>().AsQueryable()).Object);

            // Act
            var result = _service.RejectPayment(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(PaymentStatus.Failed, payment.status);

            // Verify emails never called
            _emailService.Verify(e => e.SendPaymentRejectedEmailToPatient(It.IsAny<UserTblModel>(), It.IsAny<UserTblModel>(), It.IsAny<AppointmentsTblModel>(), It.IsAny<PaymentTblModel>()), Times.Never);
            _emailService.Verify(e => e.SendPaymentRejectedEmailToDoctor(It.IsAny<UserTblModel>(), It.IsAny<UserTblModel>(), It.IsAny<AppointmentsTblModel>(), It.IsAny<PaymentTblModel>()), Times.Never);
        }

        [TestMethod]
        public void RejectPayment_ShouldStillSucceed_WhenEmailThrowsException()
        {
            // Arrange payment
            var payment = new PaymentTblModel { appointmentID = 1, status = PaymentStatus.Pending };
            var mockPayments = MockDbSetHelper.BuildMockDbSet(new[] { payment }.AsQueryable());
            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);

            // Appointment exists
            var appointment = new AppointmentsTblModel { appointmentID = 1, patientID = 10, doctorID = 20 };
            _mockContext.Setup(c => c.appointments_tbl).Returns(MockDbSetHelper.BuildMockDbSet(new[] { appointment }.AsQueryable()).Object);

            // Users exist
            var patient = new UserTblModel { userID = 10 };
            var doctor = new UserTblModel { userID = 20 };
            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet<UserTblModel>(new[] { patient, doctor }.AsQueryable()).Object);


            // Force email exception
            _emailService.Setup(e => e.SendPaymentRejectedEmailToPatient(patient, doctor, appointment, payment))
                .Throws(new Exception("SMTP down"));
            _emailService.Setup(e => e.SendPaymentRejectedEmailToDoctor(doctor, patient, appointment, payment))
                .Throws(new Exception("SMTP down"));

            // Act
            var result = _service.RejectPayment(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(PaymentStatus.Failed, payment.status);
            Assert.AreEqual(AppointmentStatus.Rejected, appointment.status);
        }

        [TestMethod]
        public void RejectPayment_ReturnsFail_WhenExceptionThrown()
        {
            // Arrange — force exception when accessing payment_tbl
            var mockPayments = new Mock<DbSet<PaymentTblModel>>();

            // Make the IQueryable provider throw when enumerated
            mockPayments.As<IQueryable<PaymentTblModel>>()
                .Setup(m => m.Provider)
                .Throws(new Exception("DB error"));

            _mockContext.Setup(c => c.payment_tbl).Returns(mockPayments.Object);

            // Act
            var result = _service.RejectPayment(1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsTrue(result.message.Contains("DB error"));
        }


    }
}
