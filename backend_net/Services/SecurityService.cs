using backend_net.Services.Interfaces;
using System.Text.Json;

namespace backend_net.Services;

public class SecurityService : ISecurityService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public SecurityService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
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
}

