using backend_net.Services.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace backend_net.Services;

public class SecurityService : ISecurityService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;
    private static readonly TimeSpan CaptchaTtl = TimeSpan.FromMinutes(5);

    public SecurityService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IMemoryCache memoryCache)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _memoryCache = memoryCache;
    }

    public async Task<bool> ValidateCaptchaAsync(string captchaToken, string? remoteIp = null)
    {
        if (string.IsNullOrWhiteSpace(captchaToken))
            return false;

        // For Google reCAPTCHA v2/v3
        var secretKey = _configuration["Security:RecaptchaSecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            // If no secret key configured, skip validation (for development)
            return true;
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaToken}&remoteip={remoteIp}",
                null
            );

            if (!response.IsSuccessStatusCode)
                return false;

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);

            return result.TryGetProperty("success", out var success) && success.GetBoolean();
        }
        catch
        {
            return false;
        }
    }

    public (string CaptchaId, string Question) CreateCustomCaptchaChallenge()
    {
        var left = Random.Shared.Next(10, 99);
        var right = Random.Shared.Next(1, 10);
        var operation = Random.Shared.Next(0, 2) == 0 ? '+' : '-';
        var answer = operation == '+' ? left + right : left - right;
        var captchaId = Guid.NewGuid().ToString("N");

        _memoryCache.Set(
            GetCaptchaCacheKey(captchaId),
            answer.ToString(),
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CaptchaTtl
            }
        );

        return (captchaId, $"{left} {operation} {right} = ?");
    }

    public bool ValidateCustomCaptchaAnswer(string captchaId, string captchaAnswer)
    {
        if (string.IsNullOrWhiteSpace(captchaId) || string.IsNullOrWhiteSpace(captchaAnswer))
            return false;

        var key = GetCaptchaCacheKey(captchaId.Trim());
        if (!_memoryCache.TryGetValue<string>(key, out var expected))
            return false;

        _memoryCache.Remove(key);
        return string.Equals(expected, captchaAnswer.Trim(), StringComparison.Ordinal);
    }

    public bool ValidateCsrfToken(string csrfToken, string? sessionToken = null)
    {
        if (string.IsNullOrWhiteSpace(csrfToken))
            return false;

        // Simple CSRF validation - in production, use proper session-based CSRF tokens
        // For now, we'll validate that the token is not empty and has minimum length
        if (csrfToken.Length < 32)
            return false;

        // If session token provided, validate match
        if (!string.IsNullOrWhiteSpace(sessionToken))
        {
            return csrfToken == sessionToken;
        }

        // Basic validation passed
        return true;
    }

    public bool ValidateHoneypot(string? honeypotValue)
    {
        // Honeypot should be empty - if it has a value, it's likely a bot
        return string.IsNullOrWhiteSpace(honeypotValue);
    }

    private static string GetCaptchaCacheKey(string captchaId) => $"custom-captcha:{captchaId}";
}

