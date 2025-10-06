using System.Net;
using System.Text;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;
using FCG.Functions.ApiClient;
using FCG.Functions.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Tests
{
    public class PixFunctionTests
    {
        private readonly Mock<IApiClient> _apiClientMock;
        private readonly Mock<ILogger<PixFunction>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly PixFunction _function;

        public PixFunctionTests()
        {
            _apiClientMock = new Mock<IApiClient>();
            _loggerMock = new Mock<ILogger<PixFunction>>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["PaymentsAPI_Url"]).Returns("https://example.com/api");
            _configurationMock.Setup(c => c["API_Token"]).Returns("test-token");

            _function = new PixFunction(_loggerMock.Object, _apiClientMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task Run_SuccessfulApiCall_CompletesMessage()
        {
            var message = CreateTestServiceBusReceivedMessage("test-message");
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
            var message = CreateTestServiceBusReceivedMessage("test-message");
            var messageActionsMock = new Mock<ServiceBusMessageActions>();

            _apiClientMock
                .Setup(client => client.CallApiAsync(HttpMethod.Post, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            await _function.Run(message, messageActionsMock.Object);

            messageActionsMock.Verify(m => m.AbandonMessageAsync(message, null, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Run_ExceptionThrown_AbandonsMessage()
        {
            var message = CreateTestServiceBusReceivedMessage("test-message");
            var messageActionsMock = new Mock<ServiceBusMessageActions>();

            _apiClientMock
                .Setup(client => client.CallApiAsync(HttpMethod.Post, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            await _function.Run(message, messageActionsMock.Object);

            messageActionsMock.Verify(m => m.AbandonMessageAsync(message, null, CancellationToken.None), Times.Once);
        }

        private ServiceBusReceivedMessage CreateTestServiceBusReceivedMessage(string messageBody)
        {
            var bodyBytes = Encoding.UTF8.GetBytes(messageBody);
            var binaryData = new BinaryData(bodyBytes);

            // Convert byte[] to ReadOnlyMemory<byte> and wrap it in an IEnumerable
            var readOnlyMemoryData = new[] { new ReadOnlyMemory<byte>(binaryData.ToArray()) };

            var amqpAnnotatedMessage = new AmqpAnnotatedMessage(AmqpMessageBody.FromData(readOnlyMemoryData));
            return ServiceBusReceivedMessage.FromAmqpMessage(amqpAnnotatedMessage, binaryData);
        }
    }
}