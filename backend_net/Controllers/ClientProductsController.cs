using backend_net.Controllers;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ClientProductsController : BaseController
{
    private readonly IClientProductService _clientProductService;
    private readonly IAccessControlService _accessControlService;

    public ClientProductsController(IClientProductService clientProductService, IAccessControlService accessControlService)
    {
        _clientProductService = clientProductService;
        _accessControlService = accessControlService;
    }

    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetClientProducts(int clientId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Check if user can access this client
            if (!IsAdmin() && !await _accessControlService.CanUserAccessClientAsync(userId.Value, clientId))
            {
                return HandleError("Unauthorized: You don't have access to this client", 403);
            }

            var products = await _clientProductService.GetClientProductsAsync(clientId);
            var productsDto = products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.MainImage,
                p.CategoryId,
                CategoryName = p.Category?.Name,
                IndustryName = p.Category?.Industry?.Name,
                AdditionalImages = p.ProductImages?.OrderBy(pi => pi.SortOrder).Select(pi => pi.ImageUrl).ToList() ?? new List<string>()
            }).ToList();

            return HandleSuccess("Client products retrieved successfully", productsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPost("attach")]
    public async Task<IActionResult> AttachProductToClient([FromBody] DTOs.Requests.AttachProductToClientRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Only Admin/Owner or Sales Person/Manager (with relationship) can attach products
            if (!IsAdmin())
            {
                if (!IsSalesPerson() && !IsSalesManager())
                {
                    return HandleError("Unauthorized: Only Admin, Owner, Sales Person, and Sales Manager can attach products", 403);
                }

                // Check if user can attach products to this client
                if (!await _accessControlService.CanUserAttachProductToClientAsync(userId.Value, request.ClientId))
                {
                    return HandleError("Unauthorized: You don't have access to attach products to this client", 403);
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _clientProductService.AttachProductToClientAsync(request);
            return HandleSuccess("Product attached to client successfully");
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message, 404);
        }
        catch (InvalidOperationException ex)
        {
            return HandleError(ex.Message, 400);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPost("attach-multiple")]
    public async Task<IActionResult> AttachMultipleProductsToClient([FromBody] DTOs.Requests.AttachMultipleProductsToClientRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Only Admin/Owner or Sales Person/Manager (with relationship) can attach products
            if (!IsAdmin())
            {
                if (!IsSalesPerson() && !IsSalesManager())
                {
                    return HandleError("Unauthorized: Only Admin, Owner, Sales Person, and Sales Manager can attach products", 403);
                }

                // Check if user can attach products to this client
                if (!await _accessControlService.CanUserAttachProductToClientAsync(userId.Value, request.ClientId))
                {
                    return HandleError("Unauthorized: You don't have access to attach products to this client", 403);
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _clientProductService.AttachMultipleProductsToClientAsync(request);
            return HandleSuccess($"{request.ProductIds.Count} product(s) attached to client successfully");
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message, 404);
        }
        catch (InvalidOperationException ex)
        {
            return HandleError(ex.Message, 400);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPost("detach")]
    public async Task<IActionResult> DetachProductFromClient([FromBody] DTOs.Requests.DetachProductFromClientRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Only Admin/Owner or Sales Person/Manager (with relationship) can detach products
            if (!IsAdmin())
            {
                if (!IsSalesPerson() && !IsSalesManager())
                {
                    return HandleError("Unauthorized: Only Admin, Owner, Sales Person, and Sales Manager can detach products", 403);
                }

                // Check if user can attach products to this client (same permission for detach)
                if (!await _accessControlService.CanUserAttachProductToClientAsync(userId.Value, request.ClientId))
                {
                    return HandleError("Unauthorized: You don't have access to detach products from this client", 403);
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _clientProductService.DetachProductFromClientAsync(request);
            return HandleSuccess("Product detached from client successfully");
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

    [HttpGet("check/{clientId}/{productId}")]
    public async Task<IActionResult> CheckProductAttached(int clientId, int productId)
    {
        try
        {
            var isAttached = await _clientProductService.IsProductAttachedToClientAsync(clientId, productId);
            return HandleSuccess("Check completed", new { isAttached });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

