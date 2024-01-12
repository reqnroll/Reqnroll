using System.Net.Http;

namespace Reqnroll.Analytics
{
    public class HttpClientWrapper
    {
        private HttpClient httpClient;

        public HttpClient HttpClient => httpClient ??= new HttpClient();
    }
}