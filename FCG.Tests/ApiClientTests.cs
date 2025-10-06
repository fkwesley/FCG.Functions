using System.Net;
using FCG.Functions.ApiClient;
using Moq;
using Moq.Protected;

namespace FCG.Tests
{
    public class ApiClientTests
    {
        private readonly ApiClient _apiClient;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public ApiClientTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            _apiClient = new ApiClient(_httpClientFactoryMock.Object);
        }

        [Fact]
        public async Task PostAsync_SendsCorrectRequest()
        {
            // Arrange
            var url = "https://example.com/api";
            var content = "{\"key\":\"value\"}";
            var authToken = "test-token";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await _apiClient.PostAsync(url, content, authToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString() == url &&
                    req.Headers.Authorization.ToString() == $"Bearer {authToken}" &&
                    req.Content.ReadAsStringAsync().Result == content),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task PostAsync_HandlesErrorResponse()
        {
            // Arrange
            var url = "https://example.com/api";
            var content = "{\"key\":\"value\"}";
            var authToken = "test-token";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var response = await _apiClient.PostAsync(url, content, authToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}