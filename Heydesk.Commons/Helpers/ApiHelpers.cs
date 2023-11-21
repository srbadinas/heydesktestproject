using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Heydesk.Commons.Helpers
{
    public static class ApiHelpers
    {
        private static string BaseUrl = "https://localhost:7062/api";

        public static async Task<HttpResponseMessage> PostRequest(string url, object? data) {
            try
            {
                var client = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + url);
                var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
                jsonContent.Headers.ContentLength = Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(data));
                request.Content = jsonContent;

                return await client.SendAsync(request);
            } 
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<HttpResponseMessage> GetRequest(string url)
        {
            try
            {
                var client = new HttpClient();
                return await client.GetAsync(BaseUrl + url);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
