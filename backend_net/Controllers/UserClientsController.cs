using backend_net.Controllers;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class UserClientsController : BaseController
{
    private readonly IUserClientService _userClientService;

    public UserClientsController(IUserClientService userClientService)
    {
        _userClientService = userClientService;
    }

    [HttpPost("attach")]
    public async Task<IActionResult> AttachUserToClient([FromBody] DTOs.Requests.AttachUserToClientRequest request)
    {
        try
        {
            // Only Admin/Owner can manage user-client relationships
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can manage user-client relationships", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userClientService.AttachUserToClientAsync(request);
            return HandleSuccess("User attached to client successfully");
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
    public async Task<IActionResult> DetachUserFromClient([FromBody] DTOs.Requests.AttachUserToClientRequest request)
    {
        try
        {
            // Only Admin/Owner can manage user-client relationships
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can manage user-client relationships", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userClientService.DetachUserFromClientAsync(request);
            return HandleSuccess("User detached from client successfully");
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

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserClients(int userId)
    {
        try
        {
            // Only Admin/Owner can view user-client relationships
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view user-client relationships", 403);
            }

            var clients = await _userClientService.GetUserClientsAsync(userId);
            var clientsDto = clients.Select(c => new
            {
                c.Id,
                c.CustomerNo,
                c.CompanyName,
                c.ContactPerson,
                c.Email,
                c.Phone
            }).ToList();

            return HandleSuccess("User clients retrieved successfully", clientsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetClientUsers(int clientId)
    {
        try
        {
            // Only Admin/Owner can view user-client relationships
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view user-client relationships", 403);
            }

            var users = await _userClientService.GetClientUsersAsync(clientId);
            var usersDto = users.Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                RoleName = u.Role?.Name
            }).ToList();

            return HandleSuccess("Client users retrieved successfully", usersDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

