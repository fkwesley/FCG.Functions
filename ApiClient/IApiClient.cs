using System.Net.Http;
using System.Threading.Tasks;

namespace FCG.Functions.ApiClient
{
    public interface IApiClient
    {
        Task<HttpResponseMessage> PostAsync(string url, string content, string authToken);
    }
}