using Moq;
using SafeTalkApp.DTOs.Resources;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;
using System.Data.Entity;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class ResourceServiceTests
    {
        private Mock<ISafeTalkAppContext> _mockContext = null!;
        private ResourceService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<ISafeTalkAppContext>();
            _service = new ResourceService(_mockContext.Object);
        }

        [TestMethod]
        public void GetResources_ShouldReturnResourcesList()
        {
            // Arrange
            var resourcesData = new List<ResourceTblModel>
        {
            new ResourceTblModel
            {
                resourceID = 1,
                title = "Resource 1",
                content = "Content 1",
                category = "Cat 1",
                type = "Type 1",
                url = "http://example.com/1",
                source = "Source 1",
                publishedDate = DateTime.Now.AddDays(-1),
                dateCreated = DateTime.Now.AddDays(-2),
                dateUpdated = DateTime.Now
            },
            new ResourceTblModel
            {
                resourceID = 2,
                title = "Resource 2",
                content = "Content 2",
                category = "Cat 2",
                type = "Type 2",
                url = "http://example.com/2",
                source = "Source 2",
                publishedDate = DateTime.Now.AddDays(-3),
                dateCreated = DateTime.Now.AddDays(-4),
                dateUpdated = DateTime.Now
            }
        };

            var mockSet = MockDbSetHelper.BuildMockDbSet(resourcesData.AsQueryable());
            _mockContext.Setup(c => c.resource_tbl).Returns(mockSet.Object);

            // Act
            var result = _service.GetResources();

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(2, result.data.Count());
            Assert.AreEqual("Resource 1", result.data.First().title);
        }

        [TestMethod]
        public void GetResources_ShouldReturnEmptyList_WhenNoResourcesExist()
        {
            // Arrange
            var emptyData = new List<ResourceTblModel>().AsQueryable();
            var mockSet = MockDbSetHelper.BuildMockDbSet(emptyData);
            _mockContext.Setup(c => c.resource_tbl).Returns(mockSet.Object);

            // Act
            var result = _service.GetResources();

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(0, result.data.Count());
        }

        [TestMethod]
        public void GetResources_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange: create a mock DbSet that throws when enumerated
            var mockSet = new Mock<DbSet<ResourceTblModel>>();
            mockSet.As<IQueryable<ResourceTblModel>>()
                .Setup(m => m.Provider)
                .Throws(new Exception("DB error"));

            _mockContext.Setup(c => c.resource_tbl).Returns(mockSet.Object);

            // Act
            var result = _service.GetResources();

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "Error getting resources");
            StringAssert.Contains(result.message, "DB error");
        }

        [TestMethod]
        public void AddResource_ShouldReturnOk_WhenResourceIsAdded()
        {
            // Arrange
            var resources = new List<ResourceTblModel>();
            var mockSet = MockDbSetHelper.BuildMockDbSet(resources.AsQueryable(),
                find: args => resources.FirstOrDefault(r => r.resourceID == (int)args[0]));

            mockSet.Setup(m => m.Add(It.IsAny<ResourceTblModel>())).Callback<ResourceTblModel>(r =>
            {
                r.resourceID = resources.Count + 1; // simulate DB-generated ID
                resources.Add(r);
            });

            _mockContext.Setup(c => c.resource_tbl).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var dto = new ResourcesDTO
            {
                title = "Test Title",
                content = "Test Content",
                category = "Test Category",
                type = "PDF",
                url = "http://example.com/resource.pdf",
                source = "Test Source",
                publishedDate = DateTime.Today
            };

            // Act
            var result = _service.AddResource(dto);

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotNull(result.data);
            Assert.AreEqual(dto.title, result.data.title);
            Assert.AreEqual(dto.content, result.data.content);
            Assert.IsTrue(result.data.resourceID > 0); // Generated ID assigned
            Assert.AreEqual(1, resources.Count); // One entity added
        }

        [TestMethod]
        public void AddResource_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange: mock DbSet.Add to throw an exception
            var mockSet = new Mock<DbSet<ResourceTblModel>>();
            mockSet.Setup(m => m.Add(It.IsAny<ResourceTblModel>())).Throws(new Exception("DB error"));
            _mockContext.Setup(c => c.resource_tbl).Returns(mockSet.Object);

            var dto = new ResourcesDTO
            {
                title = "Test Title",
                content = "Test Content",
                category = "Test Category",
                type = "PDF",
                url = "http://example.com/resource.pdf",
                source = "Test Source",
                publishedDate = DateTime.Today
            };

            // Act
            var result = _service.AddResource(dto);

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "Error adding resource");
            StringAssert.Contains(result.message, "DB error");
        }

        [TestMethod]
        public void EditResource_ShouldUpdateResource_WhenResourceExists()
        {
            // Arrange
            var resource = new ResourceTblModel
            {
                resourceID = 1,
                title = "Old Title",
                content = "Old Content",
                category = "Old Category",
                type = "Old Type",
                url = "http://old.com",
                publishedDate = DateTime.Now.AddDays(-5),
                dateCreated = DateTime.Now.AddDays(-10),
                dateUpdated = DateTime.Now.AddDays(-5)
            };

            var mockSet = MockDbSetHelper.BuildMockDbSet(new List<ResourceTblModel> { resource }.AsQueryable());
            _mockContext.Setup(c => c.resource_tbl).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChanges()).Verifiable();

            var updatedDto = new ResourcesDTO
            {
                resourceID = 1,
                title = "New Title",
                content = "New Content",
                category = "New Category",
                type = "New Type",
                url = "http://new.com",
                publishedDate = DateTime.Now
            };

            // Act
            var result = _service.EditResource(updatedDto);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("New Title", resource.title);
            Assert.AreEqual("New Content", resource.content);
            Assert.AreEqual("New Category", resource.category);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void EditResource_ShouldReturnFail_WhenResourceDoesNotExist()
        {
            // Arrange
            var mockSet = MockDbSetHelper.BuildMockDbSet(new List<ResourceTblModel>().AsQueryable());
            _mockContext.Setup(c => c.resource_tbl).Returns(mockSet.Object);

            var dto = new ResourcesDTO
            {
                resourceID = 99, // non-existent
                title = "Title"
            };

            // Act
            var result = _service.EditResource(dto);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Resource not found", result.message);
        }

        [TestMethod]
        public void EditResource_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange: force exception on Find
            var mockSet = new Mock<DbSet<ResourceTblModel>>();
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Throws(new Exception("DB error"));
            _mockContext.Setup(c => c.resource_tbl).Returns(mockSet.Object);

            var dto = new ResourcesDTO { resourceID = 1, title = "Title" };

            // Act
            var result = _service.EditResource(dto);

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsTrue(result.message.Contains("Error editing resource"));
        }

        [TestMethod]
        public void DeleteResource_ShouldDeleteResource_WhenResourceExists()
        {
            // Arrange
            var resource = new ResourceTblModel
            {
                resourceID = 1,
                title = "Resource To Delete"
            };

            var mockSet = MockDbSetHelper.BuildMockDbSet(new List<ResourceTblModel> { resource }.AsQueryable());
            _mockContext.Setup(c => c.resource_tbl).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChanges()).Verifiable();

            // Act
            var result = _service.DeleteResource(1);

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsTrue(result.data);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsFalse(mockSet.Object.Contains(resource));
        }

        [TestMethod]
        public void DeleteResource_ShouldReturnFail_WhenResourceDoesNotExist()
        {
            // Arrange
            var mockSet = MockDbSetHelper.BuildMockDbSet(new List<ResourceTblModel>().AsQueryable());
            _mockContext.Setup(c => c.resource_tbl).Returns(mockSet.Object);

            // Act
            var result = _service.DeleteResource(99); // Non-existent ID

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Resource not found", result.message);
        }

        [TestMethod]
        public void DeleteResource_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange: force exception on Find
            var mockSet = new Mock<DbSet<ResourceTblModel>>();
            mockSet.Setup(m => m.Find(It.IsAny<object[]>())).Throws(new Exception("DB error"));
            _mockContext.Setup(c => c.resource_tbl).Returns(mockSet.Object);

            // Act
            var result = _service.DeleteResource(1);

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsTrue(result.message.Contains("Error deleting resource"));
        }
    }
}
