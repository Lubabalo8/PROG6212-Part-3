using Microsoft.Extensions.Options;
using RaceBay.MVC.Models.Dto;
using RaceBay.MVC.Services.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceBay.MVC.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiSettings _settings;
    private readonly ILogger<ApiService> _logger;
    private string? _authToken;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService(
        HttpClient httpClient,
        IOptions<ApiSettings> settings,
        ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            throw new InvalidOperationException(
                "ApiSettings:BaseUrl is missing or empty. Configure it in appsettings.json.");
        }

        if (!Uri.TryCreate(_settings.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException(
                $"ApiSettings:BaseUrl is not a valid absolute URI: '{_settings.BaseUrl}'.");
        }

        _httpClient.BaseAddress = baseUri;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public void SetAuthToken(string? token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Authorization =
            string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, JsonOptions);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOptions);
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/auth/register", request, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                // Read the exact error message from the API
                var errorContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(errorContent))
                {
                    errorContent = $"API returned status code: {response.StatusCode}";
                }

                _logger.LogWarning(
                    "Registration API returned HTTP status {StatusCode}. Error: {ErrorContent}",
                    response.StatusCode, errorContent);

                // Throw exception so AccountController can catch and display it
                throw new Exception(errorContent);
            }

            if (response.Content.Headers.ContentLength == 0 || response.Content.Headers.ContentLength == null)
                return new AuthResponseDto(); // success but no token

            return await response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOptions);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Could not reach the registration API at {ApiUrl}.",
                _httpClient.BaseAddress);

            throw new Exception("Could not connect to the API. Is the API project running?");
        }
    }

    public async Task<HttpResponseMessage> GetAsync(string endpoint)
        => await _httpClient.GetAsync(endpoint);

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T payload)
        => await _httpClient.PostAsJsonAsync(endpoint, payload, JsonOptions);

    public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T payload)
        => await _httpClient.PutAsJsonAsync(endpoint, payload, JsonOptions);

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        => await _httpClient.DeleteAsync(endpoint);

    public async Task<HttpResponseMessage> PostMultipartAsync(string endpoint, MultipartFormDataContent content)
        => await _httpClient.PostAsync(endpoint, content);
}