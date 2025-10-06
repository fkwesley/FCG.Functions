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

        public async Task<HttpResponseMessage> CallApiAsync(HttpMethod httpMethod, string url, string content, string authToken)
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(httpMethod, url)
            {
                Content = content != null ? new StringContent(content, Encoding.UTF8, "application/json") : null
            };
            request.Headers.Add("Authorization", $"Bearer {authToken}");
            
            return await client.SendAsync(request);
        }
    }
}