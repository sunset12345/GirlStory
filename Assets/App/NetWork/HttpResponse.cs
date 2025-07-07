using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace App.Network
{
    public class HttpResponse : IDisposable
    {
        public bool Success { get; }
        public string Content { get; }
        public JToken Json { get; }
        public HttpResponseMessage OriginMessage { get; }
        
        public HttpStatusCode StatusCode => OriginMessage.StatusCode;
        public int Code { get; }
        
        public static HttpResponse ExceptionResponse { get; } = new(
            new HttpResponseMessage(HttpStatusCode.InternalServerError),
            null);

        internal HttpResponse(
            HttpResponseMessage originMessage,
            string content)
        {
            OriginMessage = originMessage;
            if (originMessage.IsSuccessStatusCode)
            {
                Content = content;
                Json = JToken.Parse(content);
                var code = Json["code"]?.Value<int>();
                Success = code == 0;
                Code = code ?? (int) originMessage.StatusCode;
            }
            else
            {
                Success = false;
                Content = null;
                Json = null;
                Code = (int) originMessage.StatusCode;
            }
        }

        public void Dispose()
        {
            OriginMessage?.Dispose();
        }
    }
}