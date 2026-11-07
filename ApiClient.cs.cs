using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDayMvc.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _accessor;

        public ApiClient(
            HttpClient http,
            IHttpContextAccessor accessor,
            IConfiguration configuration)
        {
            _http = http;
            _accessor = accessor;

            var baseUrl = configuration["ApiSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException(
                    "API BaseUrl is missing.");

            _http.BaseAddress = new Uri(baseUrl);
        }

        public async Task<HttpResponseMessage> SendAsync(
            HttpMethod method,
            string endpoint,
            object? data = null)
        {
            var context = _accessor.HttpContext
                ?? throw new InvalidOperationException(
                    "HTTP context unavailable.");

            using var request = new HttpRequestMessage(
                method, endpoint);

            // Forward the API session cookie.
            var apiCookie = context.Session.GetString("ApiCookie");

            if (!string.IsNullOrWhiteSpace(apiCookie))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie", apiCookie);
            }

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);

                request.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");
            }

            var response = await _http.SendAsync(request);

            // Save API session cookie after login.
            if (response.Headers.TryGetValues(
                "Set-Cookie", out var cookies))
            {
                foreach (var cookie in cookies)
                {
                    var cookiePair = cookie.Split(';')[0];

                    if (cookiePair.Contains('='))
                    {
                        context.Session.SetString(
                            "ApiCookie", cookiePair);
                    }
                }
            }

            return response;
        }

        public Task<HttpResponseMessage> GetAsync(
            string endpoint)
        {
            return SendAsync(HttpMethod.Get, endpoint);
        }

        public Task<HttpResponseMessage> PostJsonAsync(
            string endpoint, object data)
        {
            return SendAsync(HttpMethod.Post, endpoint, data);
        }

        public Task<HttpResponseMessage> PutJsonAsync(
            string endpoint, object data)
        {
            return SendAsync(HttpMethod.Put, endpoint, data);
        }

        public Task<HttpResponseMessage> DeleteAsync(
            string endpoint)
        {
            return SendAsync(HttpMethod.Delete, endpoint);
        }

        public async Task<T?> GetJsonAsync<T>(
            string endpoint)
        {
            using var response = await GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public void ClearApiCookie()
        {
            _accessor.HttpContext?
                .Session.Remove("ApiCookie");
        }
    }
}


