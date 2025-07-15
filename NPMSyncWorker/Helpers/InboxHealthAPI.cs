using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NPMSyncWorker.Helpers
{
    internal static class InboxHealthAPI
    {
        private static string _baseUrl;
        private static string _apiKey;
        private static IConfigurationRoot Configuration { get; set; }
        static InboxHealthAPI()
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            _baseUrl = Configuration.GetValue<string>("InboxHealth:BaseUrl");
            _apiKey = Configuration.GetValue<string>("InboxHealth:APIKey");
        }

        public static async Task<HttpResponseMessage> Post(dynamic data, string requestUri)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUrl);
                    client.DefaultRequestHeaders.Add("x-api-key", _apiKey);
                    var json = JsonConvert.SerializeObject(data);
                    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                    return await client.PostAsync(requestUri, stringContent);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<HttpResponseMessage> Get(string requestUri)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUrl);
                    client.DefaultRequestHeaders.Add("x-api-key", _apiKey);
                    return await client.GetAsync(requestUri);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
