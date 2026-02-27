using backend_net.Controllers;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ApiKeysController : BaseController
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeysController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    /// <summary>
    /// Get all API keys - Only Admin/Owner can view all API keys
    /// This must come before other GET routes to avoid routing conflicts
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllApiKeys()
    {
        try
        {
            // Only Admin/Owner can view all API keys
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view all API keys", 403);
            }

            var apiKeys = await _apiKeyService.GetAllApiKeysAsync();
            var apiKeysDto = apiKeys.Select(ak => new
            {
                ak.Id,
                ak.Key, // Note: In production, you might want to mask this
                ak.Name,
                ak.Description,
                ak.ClientId,
                ClientName = ak.Client?.CompanyName,
                ak.IsActive,
                ak.ExpiresAt,
                ak.AllowedOrigins,
                ak.CreatedAt,
                ak.UpdatedAt
            }).ToList();

            return HandleSuccess("API keys retrieved successfully", apiKeysDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get all API keys for a client
    /// </summary>
    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetApiKeysByClient(int clientId)
    {
        try
        {
            // Only Admin/Owner can view API keys
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view API keys", 403);
            }

            var apiKeys = await _apiKeyService.GetApiKeysByClientAsync(clientId);
            var apiKeysDto = apiKeys.Select(ak => new
            {
                ak.Id,
                ak.Key, // Note: In production, you might want to mask this
                ak.Name,
                ak.Description,
                ak.ClientId,
                ak.IsActive,
                ak.ExpiresAt,
                ak.AllowedOrigins,
                ak.CreatedAt,
                ak.UpdatedAt
            }).ToList();

            return HandleSuccess("API keys retrieved successfully", apiKeysDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get API key by ID - Only Admin/Owner can view
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetApiKeyById(int id)
    {
        try
        {
            // Only Admin/Owner can view API keys
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view API keys", 403);
            }

            var apiKey = await _apiKeyService.GetApiKeyByIdAsync(id);
            if (apiKey == null)
            {
                return HandleError("API key not found", 404);
            }

            var apiKeyDto = new
            {
                apiKey.Id,
                apiKey.Key,
                apiKey.Name,
                apiKey.Description,
                apiKey.ClientId,
                ClientName = apiKey.Client?.CompanyName,
                apiKey.IsActive,
                apiKey.ExpiresAt,
                apiKey.AllowedOrigins,
                apiKey.CreatedAt,
                apiKey.UpdatedAt
            };

            return HandleSuccess("API key retrieved successfully", apiKeyDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Create a new API key for a client - Only Admin/Owner can create
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        try
        {
            // Only Admin/Owner can create API keys
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can create API keys", 403);
            }

            var apiKey = await _apiKeyService.CreateApiKeyAsync(
                request.ClientId,
                request.Name,
                request.Description,
                request.ExpiresAt,
                request.AllowedOrigins
            );

            var apiKeyDto = new
            {
                apiKey.Id,
                apiKey.Key,
                apiKey.Name,
                apiKey.Description,
                apiKey.ClientId,
                apiKey.IsActive,
                apiKey.ExpiresAt,
                apiKey.AllowedOrigins,
                apiKey.CreatedAt
            };

            return StatusCode(201, new { message = "API key created successfully", data = apiKeyDto, success = true });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Update an API key - Only Admin/Owner can update
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateApiKey(int id, [FromBody] UpdateApiKeyRequest request)
    {
        try
        {
            // Only Admin/Owner can update API keys
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can update API keys", 403);
            }

            var apiKey = await _apiKeyService.UpdateApiKeyAsync(
                id,
                request.Name,
                request.Description,
                request.ExpiresAt,
                request.AllowedOrigins,
                request.IsActive
            );

            var apiKeyDto = new
            {
                apiKey.Id,
                apiKey.Key,
                apiKey.Name,
                apiKey.Description,
                apiKey.ClientId,
                apiKey.IsActive,
                apiKey.ExpiresAt,
                apiKey.AllowedOrigins,
                apiKey.UpdatedAt
            };

            return HandleSuccess("API key updated successfully", apiKeyDto);
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message, 404);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Revoke an API key - Only Admin/Owner can revoke
    /// </summary>
    [HttpPost("{apiKeyId}/revoke")]
    public async Task<IActionResult> RevokeApiKey(int apiKeyId)
    {
        try
        {
            // Only Admin/Owner can revoke API keys
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can revoke API keys", 403);
            }

            var result = await _apiKeyService.RevokeApiKeyAsync(apiKeyId);
            if (!result)
            {
                return HandleError("API key not found or already revoked", 404);
            }

            return HandleSuccess("API key revoked successfully", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Reactivate an API key - Only Admin/Owner can reactivate
    /// </summary>
    [HttpPost("{apiKeyId}/reactivate")]
    public async Task<IActionResult> ReactivateApiKey(int apiKeyId)
    {
        try
        {
            // Only Admin/Owner can reactivate API keys
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can reactivate API keys", 403);
            }

            var result = await _apiKeyService.ReactivateApiKeyAsync(apiKeyId);
            if (!result)
            {
                return HandleError("API key not found, already active, or expired", 404);
            }

            return HandleSuccess("API key reactivated successfully", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Delete an API key (soft delete) - Only Admin/Owner can delete
    /// Note: This revokes the API key. For permanent deletion, use revoke endpoint.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApiKey(int id)
    {
        try
        {
            // Only Admin/Owner can delete API keys
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete API keys", 403);
            }

            var apiKey = await _apiKeyService.GetApiKeyByIdAsync(id);
            if (apiKey == null)
            {
                return HandleError("API key not found", 404);
            }

            // Revoke the API key (which effectively disables it)
            var result = await _apiKeyService.RevokeApiKeyAsync(id);
            if (!result)
            {
                return HandleError("API key not found or already revoked", 404);
            }

            return HandleSuccess("API key deleted successfully", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

