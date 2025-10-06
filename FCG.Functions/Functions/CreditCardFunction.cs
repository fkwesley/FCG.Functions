using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using FCG.Functions.ApiClient;

namespace FCG.Functions.Functions
{
    public class CreditCardFunction
    {
        private readonly ILogger<CreditCardFunction> _logger;
        private readonly IApiClient _apiClient;
        private readonly string _apiUrl;
        private readonly string _authToken;

        public CreditCardFunction(ILogger<CreditCardFunction> logger, IApiClient apiClient, IConfiguration configuration)
        {
            _logger = logger;
            _apiClient = apiClient;
            _apiUrl = configuration["PaymentsAPI_Url"];
            _authToken = configuration["API_Token"];
        }

        [Function(nameof(CreditCardFunction))]
        public async Task Run(
            [ServiceBusTrigger("fcg.paymentstopic", "FCG.Payments.CreditCard", Connection = "ServiceBusConnection")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("---------------------------------------------------------------------------------------------------------");
            _logger.LogInformation("Processing message from CreditCard subscription. Message ID: {id}", message.MessageId);

            try
            {
                var response = await _apiClient.CallApiAsync(HttpMethod.Post, _apiUrl, message.Body.ToString(), _authToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Message successfully posted to API.");
                    await messageActions.CompleteMessageAsync(message);
                }
                else
                {
                    _logger.LogError("Failed to post message to API. Status Code: {statusCode}", response.StatusCode);
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
