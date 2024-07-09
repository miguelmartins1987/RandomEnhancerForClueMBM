using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HookInject
{
    public class DiceRollController
    {
        private static long Id = 1;
        public static List<long> Data { get; private set; }
        static DiceRollController()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        }

        public static Result Initialize(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return Result.Failure("random.org API key not found!");
            }

            HttpClient httpClient = Objects.HttpClient;
            object payload = new
            {
                jsonrpc = "2.0",
                method = "generateIntegers",
                @params = new
                {
                    apiKey,
                    n = 100,
                    min = 0,
                    max = 32765
                },
                id = Id++
            };
            string stringPayload = JsonConvert.SerializeObject(payload);
            HttpContent content = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> httpResponseMessageTask = httpClient.PostAsync("https://api.random.org/json-rpc/4/invoke", content);
            HttpResponseMessage httpResponseMessage = httpResponseMessageTask.Result;
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
                var definition = new { result = new { random = new { data = new List<long>() } } };
                var deserializedContent = JsonConvert.DeserializeAnonymousType(responseContent, definition);
                AddData(deserializedContent.result.random.data);
                return Result.Success();
            }
            return Result.Failure($"Unexpected status code: {httpResponseMessage.StatusCode}");
        }

        private static void AddData(List<long> data)
        {
            if (Data == null)
            {
                Data = new List<long>(data.Count);
            }
            Data.AddRange(data);
        }
    }
}