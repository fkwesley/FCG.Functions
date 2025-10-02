using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FCG.Functions.ApiClient;

namespace FCG.Functions.Functions
{
    public class DebitCardFunction // Renamed class
    {
        private readonly ILogger<DebitCardFunction> _logger; // Updated logger type
        private readonly IApiClient _apiClient;
        private readonly string _apiUrl;
        private readonly string _authToken;

        public DebitCardFunction(ILogger<DebitCardFunction> logger, IApiClient apiClient, IConfiguration configuration)
        {
            _logger = logger;
            _apiClient = apiClient;
            _apiUrl = configuration["PaymentsAPI_Url"];
            _authToken = configuration["PaymentsAPI_Token"];
        }

        [Function(nameof(DebitCardFunction))] // Updated function name
        public async Task Run(
            [ServiceBusTrigger("fcg.paymentstopic", "FCG.Payments.DebitCard", Connection = "ServiceBusConnection")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Processing message from DebitCard subscription. Message ID: {id}", message.MessageId);

            try
            {
                var response = await _apiClient.PostAsync(_apiUrl, message.Body.ToString(), _authToken);

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