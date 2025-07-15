using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NPMAPI.Services
{
    internal static class InboxHealthAPI
    {
        private static string _baseUrl;
        private static string _apiKey;

        static InboxHealthAPI()
        {
            _baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            _apiKey = ConfigurationManager.AppSettings["APIKey"];
        }

        public static Task<HttpResponseMessage> Put(dynamic data, string requestUri)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUrl);
                    client.DefaultRequestHeaders.Add("x-api-key", _apiKey);
                    var json = JsonConvert.SerializeObject(data);
                    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var task = Task.Run(() => client.PutAsync(requestUri, stringContent));
                    task.Wait();
                    //return await client.PutAsync(requestUri, stringContent);
                    return task;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


 
        public static Task<HttpResponseMessage> Create(dynamic data, string requestUri)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUrl);
                    client.DefaultRequestHeaders.Add("x-api-key", _apiKey);
                    var json = JsonConvert.SerializeObject(data);
                    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var task = Task.Run(() => client.PostAsync(requestUri, stringContent));
                    task.Wait();
                    //return await client.PutAsync(requestUri, stringContent);
                    return task;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static Task<HttpResponseMessage> Get(string requestUri)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseUrl);
                    client.DefaultRequestHeaders.Add("x-api-key", _apiKey);
                    var task = Task.Run(() => client.GetAsync(requestUri));
                    task.Wait();
                    return task;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    
    public static Task<HttpResponseMessage> Delete(string requestUri)
    {
        try
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                client.DefaultRequestHeaders.Add("x-api-key", _apiKey);
                var task = Task.Run(() => client.DeleteAsync(requestUri));
                task.Wait();
                return task;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}
}