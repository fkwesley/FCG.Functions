using FCG.Functions.ApiClient;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Functions.ApiClient
{
    public class ApiClient : IApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HttpResponseMessage> PostAsync(string url, string content, string authToken)
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {authToken}");

            return await client.SendAsync(request);
        }
    }
}