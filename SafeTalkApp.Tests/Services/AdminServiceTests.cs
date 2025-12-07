using Moq;
using SafeTalkApp.DTOs.Admin;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class AdminServiceTests
    {
        private Mock<ISafeTalkAppContext> _mockContext = null!;
        private AdminService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<ISafeTalkAppContext>();
            var mockEmailService = new Mock<IEmailService>();
            _service = new AdminService(_mockContext.Object, mockEmailService.Object);
        }

        // ---------------- FAQs ----------------

        [TestMethod]
        public void GetFaqs_ShouldReturnFaqs()
        {
            // Arrange
            var faqs = new List<FAQsTblModel>
            {
                new FAQsTblModel { faqID = 1, question = "Q1", answer = "A1", keywords = "k1" }
            }.AsQueryable();

            var mockFaqsDbSet = MockDbSetHelper.BuildMockDbSet(faqs);
            _mockContext.Setup(c => c.faqs_tbl).Returns(mockFaqsDbSet.Object);

            // Act
            var result = _service.GetFaqs();

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(1, result.data.Count());
        }

        [TestMethod]
        public void AddFaq_ShouldAddAndReturnFaq()
        {
            // Arrange
            var dto = new FAQsDTO { question = "Q", answer = "A", keywords = "k" };
            var faqList = new List<FAQsTblModel>();
            var mockFaqsDbSet = MockDbSetHelper.BuildMockDbSet(faqList.AsQueryable());

            _mockContext.Setup(c => c.faqs_tbl).Returns(mockFaqsDbSet.Object);
            _mockContext.Setup(c => c.SaveChanges()).Callback(() => { dto.faqID = 1; }).Returns(1);

            // Act
            var result = _service.AddFaq(dto);

            // Assert
            Assert.IsTrue(result.success);
            mockFaqsDbSet.Verify(db => db.Add(It.Is<FAQsTblModel>(f => f.question == "Q")), Times.Once);
        }

        [TestMethod]
        public void UpdateFaq_ShouldUpdateExistingFaq()
        {
            // Arrange
            var faq = new FAQsTblModel { faqID = 1, question = "OldQ", answer = "OldA", keywords = "OldK" };
            var mockFaqsDbSet = MockDbSetHelper.BuildMockDbSet(new List<FAQsTblModel> { faq }.AsQueryable());

            _mockContext.Setup(c => c.faqs_tbl).Returns(mockFaqsDbSet.Object);
            _mockContext.Setup(c => c.faqs_tbl.Find(1)).Returns(faq);
            _mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var dto = new FAQsDTO { faqID = 1, question = "NewQ", answer = "NewA", keywords = "NewK" };

            // Act
            var result = _service.UpdateFaq(dto);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("NewQ", faq.question);
            Assert.AreEqual("NewA", faq.answer);
        }

        [TestMethod]
        public void UpdateFaq_ShouldReturnFail_WhenNotFound()
        {
            // Arrange
            _mockContext.Setup(c => c.faqs_tbl.Find(999)).Returns((FAQsTblModel)null);

            var dto = new FAQsDTO { faqID = 999, question = "NewQ" };

            // Act
            var result = _service.UpdateFaq(dto);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("FAQ not found.", result.message);
        }

        [TestMethod]
        public void DeleteFaq_ShouldRemoveFaq()
        {
            // Arrange
            var faq = new FAQsTblModel { faqID = 1, question = "Q" };
            var mockFaqsDbSet = MockDbSetHelper.BuildMockDbSet(new List<FAQsTblModel> { faq }.AsQueryable());

            _mockContext.Setup(c => c.faqs_tbl).Returns(mockFaqsDbSet.Object);
            _mockContext.Setup(c => c.faqs_tbl.Find(1)).Returns(faq);

            // Act
            var result = _service.DeleteFaq(1);

            // Assert
            Assert.IsTrue(result.success);
            mockFaqsDbSet.Verify(db => db.Remove(It.Is<FAQsTblModel>(f => f.faqID == 1)), Times.Once);
        }

        [TestMethod]
        public void DeleteFaq_ShouldReturnFail_WhenNotFound()
        {
            // Arrange
            _mockContext.Setup(c => c.faqs_tbl.Find(999)).Returns((FAQsTblModel)null);

            // Act
            var result = _service.DeleteFaq(999);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("FAQ not found.", result.message);
        }

        // ---------------- Prompts ----------------

        [TestMethod]
        public void GetPrompts_ShouldReturnPrompts()
        {
            // Arrange
            var prompts = new List<PromptsTblModel>
            {
                new PromptsTblModel { promptID = 1, text = "Hello" }
            }.AsQueryable();

            var mockPromptsDbSet = MockDbSetHelper.BuildMockDbSet(prompts);
            _mockContext.Setup(c => c.prompts_tbl).Returns(mockPromptsDbSet.Object);

            // Act
            var result = _service.GetPrompts();

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(1, result.data.Count());
        }

        // ---------------- Doctors ----------------

        [TestMethod]
        public void GetPendingDoctors_ShouldReturnDoctors()
        {
            // Arrange
            var users = new List<UserTblModel>
            {
                new UserTblModel { userID = 1, firstName = "Doc", lastName = "Unverified", birthDate = DateTime.Now, isVerified = false, licenseNumber = "123", specialization = "Cardio" }
            }.AsQueryable();

            var userRoles = new List<UserRoleTblModel>
            {
                new UserRoleTblModel { userID = 1, roleID = 2 }
            }.AsQueryable();

            var roles = new List<RoleTblModel>
            {
                new RoleTblModel { roleID = 2, roleName = "Doctor" }
            }.AsQueryable();

            _mockContext.Setup(c => c.user_tbl).Returns(MockDbSetHelper.BuildMockDbSet(users).Object);
            _mockContext.Setup(c => c.user_role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(userRoles).Object);
            _mockContext.Setup(c => c.role_tbl).Returns(MockDbSetHelper.BuildMockDbSet(roles).Object);

            // Act
            var result = _service.GetPendingDoctors();

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(1, result.data.Count());
        }

        [TestMethod]
        public void VerifyDoctor_ShouldApproveDoctor()
        {
            // Arrange
            var user = new UserTblModel { userID = 1, isVerified = false };
            _mockContext.Setup(c => c.user_tbl.Find(1)).Returns(user);

            // Act
            var result = _service.VerifyDoctor(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsTrue(user.isVerified);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void VerifyDoctor_ShouldReturnFail_WhenNotFound()
        {
            // Arrange
            _mockContext.Setup(c => c.user_tbl.Find(999)).Returns((UserTblModel)null);

            // Act
            var result = _service.VerifyDoctor(999);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Doctor not found.", result.message);
        }
    }

}

