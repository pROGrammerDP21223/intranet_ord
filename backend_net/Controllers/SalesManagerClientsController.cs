using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/SalesManagerClients")]
[Authorize]
public class SalesManagerClientsController : BaseController
{
    private readonly ISalesManagerClientService _salesManagerClientService;

    public SalesManagerClientsController(ISalesManagerClientService salesManagerClientService)
    {
        _salesManagerClientService = salesManagerClientService;
    }

    [HttpPost("attach")]
    public async Task<IActionResult> AttachSalesManagerToClient([FromBody] DTOs.Requests.AttachSalesManagerToClientRequest request)
    {
        try
        {
            if (!IsAdmin())
                return HandleError("Unauthorized: Only Admin and Owner can manage sales manager-client relationships", 403);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _salesManagerClientService.AttachSalesManagerToClientAsync(request);
            return HandleSuccess("Sales manager attached to client successfully");
        }
        catch (KeyNotFoundException ex) { return HandleError(ex.Message, 404); }
        catch (InvalidOperationException ex) { return HandleError(ex.Message, 400); }
        catch (Exception ex) { return HandleError($"An error occurred: {ex.Message}", 500); }
    }

    [HttpPost("detach")]
    public async Task<IActionResult> DetachSalesManagerFromClient([FromBody] DTOs.Requests.AttachSalesManagerToClientRequest request)
    {
        try
        {
            if (!IsAdmin())
                return HandleError("Unauthorized: Only Admin and Owner can manage sales manager-client relationships", 403);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _salesManagerClientService.DetachSalesManagerFromClientAsync(request);
            return HandleSuccess("Sales manager detached from client successfully");
        }
        catch (KeyNotFoundException ex) { return HandleError(ex.Message, 404); }
        catch (Exception ex) { return HandleError($"An error occurred: {ex.Message}", 500); }
    }

    [HttpGet("salesmanager/{salesManagerId}")]
    public async Task<IActionResult> GetSalesManagerClients(int salesManagerId)
    {
        try
        {
            if (!IsAdmin())
                return HandleError("Unauthorized: Only Admin and Owner can view sales manager-client relationships", 403);

            var clients = await _salesManagerClientService.GetSalesManagerClientsAsync(salesManagerId);
            var dto = clients.Select(c => new { c.Id, c.CustomerNo, c.CompanyName, c.ContactPerson, c.Email, c.Phone });
            return HandleSuccess("Sales manager clients retrieved successfully", dto);
        }
        catch (Exception ex) { return HandleError($"An error occurred: {ex.Message}", 500); }
    }

    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetClientSalesManagers(int clientId)
    {
        try
        {
            if (!IsAdmin())
                return HandleError("Unauthorized: Only Admin and Owner can view sales manager-client relationships", 403);

            var salesManagers = await _salesManagerClientService.GetClientSalesManagersAsync(clientId);
            var dto = salesManagers.Select(sm => new { sm.Id, sm.Name, sm.Email, RoleName = sm.Role?.Name });
            return HandleSuccess("Client sales managers retrieved successfully", dto);
        }
        catch (Exception ex) { return HandleError($"An error occurred: {ex.Message}", 500); }
    }
}
