using Moq;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class DashboardServiceTests
    {
        private Mock<ISafeTalkAppContext> _mockDbContext = null!;
        private DashboardService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockDbContext = new Mock<ISafeTalkAppContext>();
            _service = new DashboardService(_mockDbContext.Object);
        }

        [TestMethod]
        public void GetDashboardStats_ShouldReturnCorrectCounts_WhenDataExists()
        {
            // Arrange
            int userId = 1;
            string role = "Doctor";

            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel { appointmentID = 1, doctorID = 1, status = AppointmentStatus.Pending },
                new AppointmentsTblModel { appointmentID = 2, doctorID = 1, status = AppointmentStatus.Approved },
                new AppointmentsTblModel { appointmentID = 3, doctorID = 1, status = AppointmentStatus.Paid },
                new AppointmentsTblModel { appointmentID = 4, doctorID = 1, status = AppointmentStatus.Completed, dateUpdated = DateTime.Today.AddDays(-1) },
                new AppointmentsTblModel { appointmentID = 5, doctorID = 1, status = AppointmentStatus.Missed }
            }.AsQueryable();

            var resources = new List<ResourceTblModel>
            {
                new ResourceTblModel { resourceID = 1 },
                new ResourceTblModel { resourceID = 2 }
            }.AsQueryable();

            var mockAppointments = MockDbSetHelper.BuildMockDbSet(appointments);
            var mockResources = MockDbSetHelper.BuildMockDbSet(resources);

            _mockDbContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);
            _mockDbContext.Setup(c => c.resource_tbl).Returns(mockResources.Object);

            // Act
            var result = _service.GetDashboardStats(userId, role);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(2, result.data.Resources);
            Assert.AreEqual(2, result.data.UpcomingAppointments); // Pending + Approved
            Assert.AreEqual(1, result.data.ActiveConsultations);  // Paid
            Assert.AreEqual(1, result.data.CompletedConsultations);
            Assert.AreEqual(1, result.data.PendingCount);
            Assert.AreEqual(1, result.data.ApprovedCount);
            Assert.AreEqual(1, result.data.PaidCount);
            Assert.AreEqual(1, result.data.CompletedCount);
            Assert.AreEqual(1, result.data.MissedCount);
            Assert.IsTrue(result.data.ConsultationTrends.Any());
        }

        [TestMethod]
        public void GetDashboardStats_ShouldReturnZeroCounts_WhenNoAppointmentsFound()
        {
            // Arrange
            int userId = 99;
            string role = "Doctor";

            var mockAppointments = MockDbSetHelper.BuildMockDbSet(new List<AppointmentsTblModel>().AsQueryable());
            var mockResources = MockDbSetHelper.BuildMockDbSet(new List<ResourceTblModel>().AsQueryable());

            _mockDbContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);
            _mockDbContext.Setup(c => c.resource_tbl).Returns(mockResources.Object);

            // Act
            var result = _service.GetDashboardStats(userId, role);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(0, result.data.Resources);
            Assert.AreEqual(0, result.data.UpcomingAppointments);
            Assert.AreEqual(0, result.data.CompletedConsultations);
            Assert.AreEqual(7, result.data.ConsultationTrends.Count); // Always 7 days
            Assert.IsTrue(result.data.ConsultationTrends.All(t => t.Count == 0)); // All counts are zero

        }

        [TestMethod]
        public void GetDashboardStats_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange
            _mockDbContext.Setup(c => c.appointments_tbl).Throws(new Exception("DB error"));

            // Act
            var result = _service.GetDashboardStats(1, "Doctor");

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "An error occurred while fetching dashboard stats");
        }

        [TestMethod]
        public void GetAdminReports_ShouldReturnZeroCounts_WhenNoDataExists()
        {
            // Arrange
            var mockAppointments = MockDbSetHelper.BuildMockDbSet(new List<AppointmentsTblModel>().AsQueryable());
            var mockRoles = MockDbSetHelper.BuildMockDbSet(new List<RoleTblModel>().AsQueryable());
            var mockUsers = MockDbSetHelper.BuildMockDbSet(new List<UserTblModel>().AsQueryable());
            var mockUserRoles = MockDbSetHelper.BuildMockDbSet(new List<UserRoleTblModel>().AsQueryable());
            var mockResources = MockDbSetHelper.BuildMockDbSet(new List<ResourceTblModel>().AsQueryable());

            _mockDbContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);
            _mockDbContext.Setup(c => c.role_tbl).Returns(mockRoles.Object);
            _mockDbContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);
            _mockDbContext.Setup(c => c.user_role_tbl).Returns(mockUserRoles.Object);
            _mockDbContext.Setup(c => c.resource_tbl).Returns(mockResources.Object);

            // Act
            var result = _service.GetAdminReports();

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);

            // Status counts should all be 0
            Assert.IsTrue(result.data.StatusCounts.Values.All(v => v == 0));

            // Trends should always have 7 days
            Assert.AreEqual(7, result.data.AppointmentTrends.Count);
            Assert.IsTrue(result.data.AppointmentTrends.All(t => t.Count == 0));

            // User growth and resource uploads always have 6 months
            Assert.AreEqual(6, result.data.UserGrowth.Count);
            Assert.AreEqual(6, result.data.ResourceUploads.Count);
            Assert.IsTrue(result.data.UserGrowth.All(u => u.PatientCount == 0 && u.DoctorCount == 0));
            Assert.IsTrue(result.data.ResourceUploads.All(r => r.Count == 0));
        }

        [TestMethod]
        public void GetAdminReports_ShouldCalculateStatusCounts_Correctly()
        {
            // Arrange
            var appointments = new List<AppointmentsTblModel>
            {
                new AppointmentsTblModel { status = AppointmentStatus.Pending },
                new AppointmentsTblModel { status = AppointmentStatus.Pending },
                new AppointmentsTblModel { status = AppointmentStatus.Completed },
                new AppointmentsTblModel { status = AppointmentStatus.Paid }
            }.AsQueryable();

            var mockAppointments = MockDbSetHelper.BuildMockDbSet(appointments);
            var mockRoles = MockDbSetHelper.BuildMockDbSet(new List<RoleTblModel>().AsQueryable());
            var mockUsers = MockDbSetHelper.BuildMockDbSet(new List<UserTblModel>().AsQueryable());
            var mockUserRoles = MockDbSetHelper.BuildMockDbSet(new List<UserRoleTblModel>().AsQueryable());
            var mockResources = MockDbSetHelper.BuildMockDbSet(new List<ResourceTblModel>().AsQueryable());

            _mockDbContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);
            _mockDbContext.Setup(c => c.role_tbl).Returns(mockRoles.Object);
            _mockDbContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);
            _mockDbContext.Setup(c => c.user_role_tbl).Returns(mockUserRoles.Object);
            _mockDbContext.Setup(c => c.resource_tbl).Returns(mockResources.Object);

            // Act
            var result = _service.GetAdminReports();

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(2, result.data.StatusCounts["Pending"]);
            Assert.AreEqual(1, result.data.StatusCounts["Completed"]);
            Assert.AreEqual(1, result.data.StatusCounts["Paid"]);
        }

        [TestMethod]
        public void GetAdminReports_ShouldCalculateUserGrowth_AndResourceUploads()
        {
            // Arrange
            var today = DateTime.Today;

            var roles = new List<RoleTblModel>
            {
                new RoleTblModel { roleID = 1, roleName = "User" },
                new RoleTblModel { roleID = 2, roleName = "Doctor" }
            }.AsQueryable();

            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 10, dateCreated = today, },
                new UserTblModel { userID = 20, dateCreated = today.AddMonths(-1) },
                new UserTblModel { userID = 30, dateCreated = today.AddMonths(-2) }
            }.AsQueryable();

            var userRoles = new List<UserRoleTblModel>
            {
                new UserRoleTblModel { userID = 10, roleID = 1 },
                new UserRoleTblModel { userID = 20, roleID = 2 },
                new UserRoleTblModel { userID = 30, roleID = 1 }
            }.AsQueryable();

            var resources = new List<ResourceTblModel>
            {
                new ResourceTblModel { resourceID = 1, dateCreated = today },
                new ResourceTblModel { resourceID = 2, dateCreated = today.AddMonths(-1) }
            }.AsQueryable();

            var mockAppointments = MockDbSetHelper.BuildMockDbSet(new List<AppointmentsTblModel>().AsQueryable());
            var mockRoles = MockDbSetHelper.BuildMockDbSet(roles);
            var mockUsers = MockDbSetHelper.BuildMockDbSet(users);
            var mockUserRoles = MockDbSetHelper.BuildMockDbSet(userRoles);
            var mockResources = MockDbSetHelper.BuildMockDbSet(resources);

            _mockDbContext.Setup(c => c.appointments_tbl).Returns(mockAppointments.Object);
            _mockDbContext.Setup(c => c.role_tbl).Returns(mockRoles.Object);
            _mockDbContext.Setup(c => c.user_tbl).Returns(mockUsers.Object);
            _mockDbContext.Setup(c => c.user_role_tbl).Returns(mockUserRoles.Object);
            _mockDbContext.Setup(c => c.resource_tbl).Returns(mockResources.Object);

            // Act
            var result = _service.GetAdminReports();

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data.UserGrowth);
            Assert.IsNotNull(result.data.ResourceUploads);

            var thisMonth = result.data.UserGrowth.FirstOrDefault(u => u.Month == today.ToString("MMM yyyy"));
            Assert.IsNotNull(thisMonth);
            Assert.IsTrue(thisMonth.PatientCount > 0 || thisMonth.DoctorCount > 0);

            var resThisMonth = result.data.ResourceUploads.FirstOrDefault(r => r.Month == today.ToString("MMM yyyy"));
            Assert.IsNotNull(resThisMonth);
            Assert.AreEqual(1, resThisMonth.Count);
        }


    }
}
