using System.Net;
using Azure.Messaging.ServiceBus;
using FCG.Functions.ApiClient;
using FCG.Functions.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Tests
{
    public class DebitCardFunctionTests
    {
        private readonly Mock<IApiClient> _apiClientMock;
        private readonly Mock<ILogger<DebitCardFunction>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly DebitCardFunction _function;

        public DebitCardFunctionTests()
        {
            _apiClientMock = new Mock<IApiClient>();
            _loggerMock = new Mock<ILogger<DebitCardFunction>>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["PaymentsAPI_Url"]).Returns("https://example.com/api");
            _configurationMock.Setup(c => c["API_Token"]).Returns("test-token");

            _function = new DebitCardFunction(_loggerMock.Object, _apiClientMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task Run_SuccessfulApiCall_CompletesMessage()
        {
            var messageBody = new BinaryData("test-message");
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);
            var messageActionsMock = new Mock<ServiceBusMessageActions>();

            _apiClientMock
                .Setup(client => client.CallApiAsync(HttpMethod.Post, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            await _function.Run(message, messageActionsMock.Object);

            messageActionsMock.Verify(m => m.CompleteMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Run_FailedApiCall_AbandonsMessage()
        {
            var messageBody = new BinaryData("test-message");
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);
            var messageActionsMock = new Mock<ServiceBusMessageActions>();

            _apiClientMock
                .Setup(client => client.CallApiAsync(HttpMethod.Post, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            await _function.Run(message, messageActionsMock.Object);

            messageActionsMock.Verify(m => m.AbandonMessageAsync(message, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Run_ExceptionThrown_AbandonsMessage()
        {
            var messageBody = new BinaryData("test-message");
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);
            var messageActionsMock = new Mock<ServiceBusMessageActions>();

            _apiClientMock
                .Setup(client => client.CallApiAsync(HttpMethod.Post, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            await _function.Run(message, messageActionsMock.Object);

            messageActionsMock.Verify(m => m.AbandonMessageAsync(message, null, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}