using Moq;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using SafeTalkApp.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SafeTalkApp.Tests.Services
{
    [TestClass]
    public class ChatBotServiceTests
    {
        private Mock<ISafeTalkAppContext> _mockContext = null!;
        private HttpClient _httpClient = null!;
        private ChatBotService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<ISafeTalkAppContext>();
        }

        [TestMethod]
        public async Task GetResponseAsync_WhenFaqMatch_ReturnsFaqAnswer()
        {
            // Arrange
            var faqData = new List<FAQsTblModel>
            {
                new FAQsTblModel { faqID = 1, question = "What is HIV?", answer = "HIV is a virus.", keywords = "hiv,virus" }
            }.AsQueryable();

            var mockFaqs = MockDbSetHelper.BuildMockDbSet(faqData);
            _mockContext.Setup(db => db.faqs_tbl).Returns(mockFaqs.Object);

            // Fake HttpClient (not used in this case)
            _httpClient = new HttpClient(new FakeHttpMessageHandler("AI not needed"));

            _service = new ChatBotService(_mockContext.Object, _httpClient, "fake-api-key");

            // Act
            var result = await _service.GetResponseAsync("Tell me about HIV virus");

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("HIV is a virus.", result.data);
        }

        [TestMethod]
        public async Task GetResponseAsync_WhenNoFaqMatch_CallsAI()
        {
            // Arrange
            var faqData = new List<FAQsTblModel>().AsQueryable();
            var mockFaqs = MockDbSetHelper.BuildMockDbSet(faqData);
            _mockContext.Setup(db => db.faqs_tbl).Returns(mockFaqs.Object);

            // Return JSON like Cohere would
            _httpClient = new HttpClient(new FakeHttpMessageHandler("AI fallback answer"));

            _service = new ChatBotService(_mockContext.Object, _httpClient, "fake-api-key");

            // Act
            var result = await _service.GetResponseAsync("Random question");

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual("Sorry, I didn’t quite get that. Could you rephrase?", result.data);
        }

        [TestMethod]
        public async Task GetResponseAsync_WhenException_ReturnsFail()
        {
            // Arrange
            var faqData = new List<FAQsTblModel>().AsQueryable();
            var mockFaqs = MockDbSetHelper.BuildMockDbSet(faqData);
            _mockContext.Setup(db => db.faqs_tbl).Returns(mockFaqs.Object);

            // Fake HttpClient that throws
            _httpClient = new HttpClient(new ThrowingHttpMessageHandler());

            _service = new ChatBotService(_mockContext.Object, _httpClient, "fake-api-key");

            // Act
            var result = await _service.GetResponseAsync("Trigger error");

            // Assert
            Assert.IsFalse(result.success);
            StringAssert.Contains(result.message, "Error getting chatbot response");
        }
    }

    // --- Helpers ---
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _response;

        public FakeHttpMessageHandler(string response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var fakeJson = $@"{{
            ""generations"": [{{ ""text"": ""{_response}"" }}]
            }}";

            var message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(fakeJson, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(message);
        }
    }

    public class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Simulated network failure");
        }
    }
}
