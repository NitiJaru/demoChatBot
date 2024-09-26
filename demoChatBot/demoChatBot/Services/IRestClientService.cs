using Flurl.Http;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace demoChatBot.Services
{
    public interface IRestClientService
    {

        Task<T> Get<T>(string endpointUrl, string? header = null);
        Task<T> Post<T>(string endpointUrl, string requestBody, string? header = null);
        Task Put(string endpointUrl, string requestBody);
        Task Post(string endpointUrl, string requestBody, string? header = null);
        Task<T> Put<T>(string endpointUrl, string requestBody);
    }
    public class RestClientService : IRestClientService
    {



        public async Task<T> Get<T>(string endpointUrl, string? header = null)
        {
            try
            {
                var rsp = string.IsNullOrEmpty(header) ?
                     await endpointUrl.GetAsync()
                     : await endpointUrl
                    .WithHeader("Line-id", header)
                    .GetAsync();
                if (rsp.StatusCode == 200)
                    return await rsp.ResponseMessage.Content.ReadFromJsonAsync<T>();
            }
            catch
            {
            }
            return default(T);
        }

        public async Task<T> Post<T>(string endpointUrl, string requestBody, string? header = null)
        {
            //var rsp = await endpointUrl
            //    .PostAsync(new StringContent(requestBody, Encoding.UTF8, "application/json"));
            //if (rsp.StatusCode == 200)
            //    return await rsp.ResponseMessage.Content.ReadFromJsonAsync<T>();
            //return default(T);
            try
            {
                var rsp = string.IsNullOrEmpty(header) ?
                     await endpointUrl
                     .PostAsync(new StringContent(requestBody, Encoding.UTF8, "application/json"))
                     : await endpointUrl
                     .WithHeader("Line-id", header)
                     .PostAsync(new StringContent(requestBody, Encoding.UTF8, "application/json"));
                if (rsp.StatusCode == 200)
                    return await rsp.ResponseMessage.Content.ReadFromJsonAsync<T>();
            }
            catch
            {
            }
            return default(T);
        }

        public async Task Put(string endpointUrl, string requestBody)
        {
            await endpointUrl.PutAsync(new StringContent(requestBody, Encoding.UTF8, "application/json"));
        }

        public async Task Post(string endpointUrl, string requestBody, string? header = null)
        {
            try
            {
                var rsp = string.IsNullOrEmpty(header) ?
                     await endpointUrl
                     .PostAsync(new StringContent(requestBody, Encoding.UTF8, "application/json"))
                     : await endpointUrl
                     .WithHeader("Line-id", header)
                     .PostAsync(new StringContent(requestBody, Encoding.UTF8, "application/json"));
                if (rsp.StatusCode == 200)
                    await endpointUrl.PostAsync(new StringContent(requestBody, Encoding.UTF8, "application/json"));
            }
            catch
            {
            }
        }

        public async Task<T> Put<T>(string endpointUrl, string requestBody)
        {
            var rsp = await endpointUrl
                .PutAsync(new StringContent(requestBody, Encoding.UTF8, "application/json"));
            if (rsp.StatusCode == 200)
                return await rsp.ResponseMessage.Content.ReadFromJsonAsync<T>();

            return default(T);
        }
    }
}
