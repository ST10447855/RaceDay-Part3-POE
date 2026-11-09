using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.MVC.Services
{
    public class ApiResult
    {
        public bool Success { get; init; }
        public int StatusCode { get; init; }
        public string Error { get; init; } = string.Empty;
    }

    public class ApiResult<T> : ApiResult
    {
        public T? Data { get; init; }
    }

    public class UploadResponse
    {
        public string? Url { get; set; }
    }

    /// <summary>
    /// The only place the MVC app talks to the RaceDay API. Attaches the session's JWT and turns
    /// failures into friendly messages so raw errors are never shown to users.
    /// </summary>
    public class ApiService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _accessor;

        public ApiService(IHttpClientFactory factory, IHttpContextAccessor accessor)
        {
            _factory = factory;
            _accessor = accessor;
        }

        private HttpClient CreateClient()
        {
            var client = _factory.CreateClient("RaceDayAPI");
            var token = _accessor.HttpContext?.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public Task<ApiResult<T>> GetAsync<T>(string url) => SendAsync<T>(HttpMethod.Get, url, null, null);

        public Task<ApiResult<T>> PostAsync<T>(string url, object? body) => SendAsync<T>(HttpMethod.Post, url, body, null);

        public Task<ApiResult<object>> PostAsync(string url, object? body) => SendAsync<object>(HttpMethod.Post, url, body, null);

        public Task<ApiResult<object>> PutAsync(string url, object? body) => SendAsync<object>(HttpMethod.Put, url, body, null);

        public Task<ApiResult<object>> DeleteAsync(string url) => SendAsync<object>(HttpMethod.Delete, url, null, null);

        /// <summary>Sends the image to the API (which stores it in Azure Blob Storage) and returns its URL.</summary>
        public async Task<ApiResult<UploadResponse>> UploadImageAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            return await SendAsync<UploadResponse>(HttpMethod.Post, "api/Uploads/image", null, content);
        }

        private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body, HttpContent? content)
        {
            try
            {
                using var request = new HttpRequestMessage(method, url);
                if (content != null)
                {
                    request.Content = content;
                }
                else if (body != null)
                {
                    request.Content = JsonContent.Create(body);
                }

                using var response = await CreateClient().SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    T? data = default;
                    if (typeof(T) != typeof(object) && response.Content.Headers.ContentLength != 0)
                    {
                        try { data = await response.Content.ReadFromJsonAsync<T>(); }
                        catch (JsonException) { }
                    }
                    return new ApiResult<T> { Success = true, StatusCode = (int)response.StatusCode, Data = data };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Token expired or invalid: end the session so the user is sent to the login page
                    _accessor.HttpContext?.Session.Clear();
                }

                return new ApiResult<T>
                {
                    Success = false,
                    StatusCode = (int)response.StatusCode,
                    Error = await ReadError(response)
                };
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                return new ApiResult<T>
                {
                    Success = false,
                    StatusCode = 0,
                    Error = "We could not reach the RaceDay service. Please try again in a moment."
                };
            }
        }

        private static async Task<string> ReadError(HttpResponseMessage response)
        {
            var code = (int)response.StatusCode;
            if (code == 401) return "Your session has expired. Please log in again.";
            if (code == 403) return "You do not have permission to do that.";
            if (code == 404) return "We could not find what you were looking for.";

            try
            {
                var text = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                    {
                        return message.GetString() ?? string.Empty;
                    }

                    if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var property in errors.EnumerateObject())
                        {
                            if (property.Value.ValueKind != JsonValueKind.Array) continue;
                            foreach (var item in property.Value.EnumerateArray())
                            {
                                var msg = item.GetString();
                                if (!string.IsNullOrWhiteSpace(msg)) return msg;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // fall through to the generic message
            }

            return "Something went wrong. Please check your details and try again.";
        }
    }
}
