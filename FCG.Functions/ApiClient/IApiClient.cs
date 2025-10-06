using System.Net.Http;
using System.Threading.Tasks;

namespace FCG.Functions.ApiClient
{
    public interface IApiClient
    {
        Task<HttpResponseMessage> CallApiAsync(HttpMethod httpMethod, string url, string content, string authToken);
    }
}