// Copyright (c) 2024 Miguel Martins
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using System.Collections.Generic;
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