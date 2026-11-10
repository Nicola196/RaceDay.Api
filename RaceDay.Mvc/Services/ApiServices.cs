using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RaceDay.Mvc.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CookieContainer _cookieContainer = new CookieContainer();

        public ApiService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;

            var handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                UseCookies = true,
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            // Safely get the URL
            var configUrl = configuration["ApiSettings:BaseUrl"];
            if (string.IsNullOrWhiteSpace(configUrl))
            {
                configUrl = "http://localhost:5269";
            }

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(configUrl)
            };
        }

        private void LoadCookiesFromSession()
        {
            var apiCookie = _httpContextAccessor.HttpContext?.Session.GetString("ApiSessionCookie");
            if (!string.IsNullOrEmpty(apiCookie) && _httpClient.BaseAddress != null)
            {
                _cookieContainer.SetCookies(_httpClient.BaseAddress, apiCookie);
            }
        }

        private void SaveCookiesToSession()
        {
            if (_httpClient.BaseAddress == null) return;

            var cookies = _cookieContainer.GetCookies(_httpClient.BaseAddress);
            var cookieString = string.Join("; ", cookies.Cast<Cookie>().Select(c => $"{c.Name}={c.Value}"));
            _httpContextAccessor.HttpContext?.Session.SetString("ApiSessionCookie", cookieString);
        }

        public async Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            LoadCookiesFromSession();
            var response = await _httpClient.GetAsync(endpoint);
            SaveCookiesToSession();
            return response;
        }

        public async Task<HttpResponseMessage> PostAsync(string endpoint, object data)
        {
            LoadCookiesFromSession();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            SaveCookiesToSession();
            return response;
        }

        public async Task<HttpResponseMessage> PutAsync(string endpoint, object data)
        {
            LoadCookiesFromSession();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            SaveCookiesToSession();
            return response;
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            LoadCookiesFromSession();
            var response = await _httpClient.DeleteAsync(endpoint);
            SaveCookiesToSession();
            return response;
        }

        public async Task<HttpResponseMessage> PostFileAsync(string endpoint, IFormFile file)
        {
            LoadCookiesFromSession();
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync(endpoint, content);
            SaveCookiesToSession();
            return response;
        }
    }
}