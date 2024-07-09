using System.Net.Http;

namespace HookInject
{
    internal class Objects
    {
        private static readonly HttpClient httpClient = new HttpClient();

        internal static HttpClient HttpClient
        {
            get
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("miguelmartins1987 at github.com");
                return httpClient;
            }
        }
    }
}