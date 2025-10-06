using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FCG.Functions.ApiClient;

namespace FCG.Functions.Functions
{
    public class PaymentCompletedFunction
    {
        private readonly ILogger<PaymentCompletedFunction> _logger;
        private readonly IApiClient _apiClient;
        private readonly string _apiUrl;
        private readonly string _authToken;

        public PaymentCompletedFunction(ILogger<PaymentCompletedFunction> logger, IApiClient apiClient, IConfiguration configuration)
        {
            _logger = logger;
            _apiClient = apiClient;
            _apiUrl = configuration["OrdersAPI_Url"];
            _authToken = configuration["API_Token"];
        }

        [Function(nameof(PaymentCompletedFunction))]
        public async Task Run(
            [ServiceBusTrigger("fcg.paymentstopic", "FCG.Payments.Completed", Connection = "ServiceBusConnection")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("---------------------------------------------------------------------------------------------------------");
            _logger.LogInformation("Processing message from PaymentCompleted subscription. Message ID: {id}", message.MessageId);

            try
            {
                var orderId = message.ApplicationProperties.ContainsKey("OrderId") ? 
                                message.ApplicationProperties["OrderId"].ToString() : 
                                throw new InvalidOperationException("OrderId not received");
                _logger.LogInformation("Order ID: {orderId}", orderId);

                var paymentStatus = message.ApplicationProperties.ContainsKey("PaymentStatus") ?
                                        message.ApplicationProperties["PaymentStatus"].ToString() :
                                        throw new InvalidOperationException("PaymentStatus not received");
                _logger.LogInformation("PaymentStatus: {paymentStatus}", paymentStatus);

                if (paymentStatus == "Completed")
                    paymentStatus = "Paid";

                var response = await _apiClient.CallApiAsync(HttpMethod.Put, 
                                                    $"{_apiUrl}/{orderId}?orderStatus={paymentStatus}", 
                                                    null, 
                                                    _authToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Message successfully put to API.");
                    await messageActions.CompleteMessageAsync(message);
                }
                else
                {
                    _logger.LogError("Failed to put message to API. Status Code: {statusCode}", response.StatusCode);
                    await messageActions.AbandonMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the message.");
                await messageActions.AbandonMessageAsync(message);
            }
        }
    }
}
