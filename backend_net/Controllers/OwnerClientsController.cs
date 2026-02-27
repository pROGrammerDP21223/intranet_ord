using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/OwnerClients")]
[Authorize]
public class OwnerClientsController : BaseController
{
    private readonly IOwnerClientService _ownerClientService;

    public OwnerClientsController(IOwnerClientService ownerClientService)
    {
        _ownerClientService = ownerClientService;
    }

    [HttpPost("attach")]
    public async Task<IActionResult> AttachOwnerToClient([FromBody] DTOs.Requests.AttachOwnerToClientRequest request)
    {
        try
        {
            if (!IsAdmin())
                return HandleError("Unauthorized: Only Admin and Owner can manage owner-client relationships", 403);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _ownerClientService.AttachOwnerToClientAsync(request);
            return HandleSuccess("Owner attached to client successfully");
        }
        catch (KeyNotFoundException ex) { return HandleError(ex.Message, 404); }
        catch (InvalidOperationException ex) { return HandleError(ex.Message, 400); }
        catch (Exception ex) { return HandleError($"An error occurred: {ex.Message}", 500); }
    }

    [HttpPost("detach")]
    public async Task<IActionResult> DetachOwnerFromClient([FromBody] DTOs.Requests.AttachOwnerToClientRequest request)
    {
        try
        {
            if (!IsAdmin())
                return HandleError("Unauthorized: Only Admin and Owner can manage owner-client relationships", 403);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _ownerClientService.DetachOwnerFromClientAsync(request);
            return HandleSuccess("Owner detached from client successfully");
        }
        catch (KeyNotFoundException ex) { return HandleError(ex.Message, 404); }
        catch (Exception ex) { return HandleError($"An error occurred: {ex.Message}", 500); }
    }

    [HttpGet("owner/{ownerId}")]
    public async Task<IActionResult> GetOwnerClients(int ownerId)
    {
        try
        {
            if (!IsAdmin())
                return HandleError("Unauthorized: Only Admin and Owner can view owner-client relationships", 403);

            var clients = await _ownerClientService.GetOwnerClientsAsync(ownerId);
            var dto = clients.Select(c => new { c.Id, c.CustomerNo, c.CompanyName, c.ContactPerson, c.Email, c.Phone });
            return HandleSuccess("Owner clients retrieved successfully", dto);
        }
        catch (Exception ex) { return HandleError($"An error occurred: {ex.Message}", 500); }
    }

    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetClientOwners(int clientId)
    {
        try
        {
            if (!IsAdmin())
                return HandleError("Unauthorized: Only Admin and Owner can view owner-client relationships", 403);

            var owners = await _ownerClientService.GetClientOwnersAsync(clientId);
            var dto = owners.Select(o => new { o.Id, o.Name, o.Email, RoleName = o.Role?.Name });
            return HandleSuccess("Client owners retrieved successfully", dto);
        }
        catch (Exception ex) { return HandleError($"An error occurred: {ex.Message}", 500); }
    }
}
