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
    public class PixFunction // Renamed class
    {
        private readonly ILogger<PixFunction> _logger; // Updated logger type
        private readonly IApiClient _apiClient;
        private readonly string _apiUrl;
        private readonly string _authToken;

        public PixFunction(ILogger<PixFunction> logger, IApiClient apiClient, IConfiguration configuration)
        {
            _logger = logger;
            _apiClient = apiClient;
            _apiUrl = configuration["PaymentsAPI_Url"];
            _authToken = configuration["API_Token"];
        }

        [Function(nameof(PixFunction))] // Updated function name    
        public async Task Run(
            [ServiceBusTrigger("fcg.paymentstopic", "FCG.Payments.Pix", Connection = "ServiceBusConnection")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("---------------------------------------------------------------------------------------------------------");
            _logger.LogInformation("Processing message from Pix subscription. Message ID: {id}", message.MessageId);

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