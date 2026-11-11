using RaceBay.MVC.Models.Dto;

namespace RaceBay.MVC.Services.Interfaces;

public interface IApiService
{
    // Generic helpers
    Task<HttpResponseMessage> GetAsync(string endpoint);
    Task<HttpResponseMessage> PostAsync<T>(string endpoint, T payload);
    Task<HttpResponseMessage> PutAsync<T>(string endpoint, T payload);
    Task<HttpResponseMessage> DeleteAsync(string endpoint);
    Task<HttpResponseMessage> PostMultipartAsync(string endpoint, MultipartFormDataContent content);
    void SetAuthToken(string? token);

    // Auth-specific (returns parsed DTO or null)
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request);
}