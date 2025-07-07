using System;
using System.Net.Http;
using System.Text;
using GSDev.Singleton;
using Newtonsoft.Json.Linq;

namespace App.Network
{
    public class HttpManager : MonoSingleton<HttpManager>
    {
        private HttpClient _client;
        public HttpClient Client => _client;
        private StringBuilder _stringBuilder = new (256);

        public void Setup(TimeSpan timeout)
        {
            _client = new HttpClient(new HttpClientHandler{})
            {
                Timeout = timeout
            };
        }
    }
}