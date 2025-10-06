using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FCG.Functions.ApiClient;
using FCG.Functions.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FCG.Tests
{
    public class CreditCardFunctionTests
    {
        private readonly Mock<IApiClient> _apiClientMock;
        private readonly Mock<ILogger<CreditCardFunction>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly CreditCardFunction _function;

        public CreditCardFunctionTests()
        {
            _apiClientMock = new Mock<IApiClient>();
            _loggerMock = new Mock<ILogger<CreditCardFunction>>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["PaymentsAPI_Url"]).Returns("https://example.com/api");
            _configurationMock.Setup(c => c["PaymentsAPI_Token"]).Returns("test-token");

            _function = new CreditCardFunction(_loggerMock.Object, _apiClientMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task Run_SuccessfulApiCall_CompletesMessage()
        {
            // Arrange
            var messageBody = new BinaryData("test-message");
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);
            var messageActionsMock = new Mock<ServiceBusMessageActions>();

            _apiClientMock
                .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            await _function.Run(message, messageActionsMock.Object);

            // Assert
            messageActionsMock.Verify(m => m.CompleteMessageAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Run_FailedApiCall_AbandonsMessage()
        {
            // Arrange
            var messageBody = new BinaryData("test-message");
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);
            var messageActionsMock = new Mock<ServiceBusMessageActions>();

            _apiClientMock
                .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            await _function.Run(message, messageActionsMock.Object);

            // Assert
            messageActionsMock.Verify(m => m.AbandonMessageAsync(message, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Run_ExceptionThrown_AbandonsMessage()
        {
            // Arrange
            var messageBody = new BinaryData("test-message");
            var message = ServiceBusModelFactory.ServiceBusReceivedMessage(body: messageBody);
            var messageActionsMock = new Mock<ServiceBusMessageActions>();

            _apiClientMock
                .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await _function.Run(message, messageActionsMock.Object);

            // Assert
            messageActionsMock.Verify(m => m.AbandonMessageAsync(message, null, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}