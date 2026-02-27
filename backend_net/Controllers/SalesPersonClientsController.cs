using backend_net.Controllers;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class SalesPersonClientsController : BaseController
{
    private readonly ISalesPersonClientService _salesPersonClientService;

    public SalesPersonClientsController(ISalesPersonClientService salesPersonClientService)
    {
        _salesPersonClientService = salesPersonClientService;
    }

    [HttpPost("attach")]
    public async Task<IActionResult> AttachSalesPersonToClient([FromBody] DTOs.Requests.AttachSalesPersonToClientRequest request)
    {
        try
        {
            // Only Admin/Owner can manage sales person-client relationships
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can manage sales person-client relationships", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _salesPersonClientService.AttachSalesPersonToClientAsync(request);
            return HandleSuccess("Sales person attached to client successfully");
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
    public async Task<IActionResult> AttachMultipleSalesPersonsToClient([FromBody] DTOs.Requests.AttachMultipleSalesPersonsToClientRequest request)
    {
        try
        {
            // Only Admin/Owner can manage sales person-client relationships
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can manage sales person-client relationships", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _salesPersonClientService.AttachMultipleSalesPersonsToClientAsync(request);
            return HandleSuccess($"{request.SalesPersonIds.Count} sales person(s) attached to client successfully");
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
    public async Task<IActionResult> DetachSalesPersonFromClient([FromBody] DTOs.Requests.AttachSalesPersonToClientRequest request)
    {
        try
        {
            // Only Admin/Owner can manage sales person-client relationships
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can manage sales person-client relationships", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _salesPersonClientService.DetachSalesPersonFromClientAsync(request);
            return HandleSuccess("Sales person detached from client successfully");
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

    [HttpGet("salesperson/{salesPersonId}")]
    public async Task<IActionResult> GetSalesPersonClients(int salesPersonId)
    {
        try
        {
            // Only Admin/Owner can view sales person-client relationships
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view sales person-client relationships", 403);
            }

            var clients = await _salesPersonClientService.GetSalesPersonClientsAsync(salesPersonId);
            var clientsDto = clients.Select(c => new
            {
                c.Id,
                c.CustomerNo,
                c.CompanyName,
                c.ContactPerson,
                c.Email,
                c.Phone
            }).ToList();

            return HandleSuccess("Sales person clients retrieved successfully", clientsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetClientSalesPersons(int clientId)
    {
        try
        {
            // Only Admin/Owner can view sales person-client relationships
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view sales person-client relationships", 403);
            }

            var salesPersons = await _salesPersonClientService.GetClientSalesPersonsAsync(clientId);
            var salesPersonsDto = salesPersons.Select(sp => new
            {
                sp.Id,
                sp.Name,
                sp.Email,
                RoleName = sp.Role?.Name
            }).ToList();

            return HandleSuccess("Client sales persons retrieved successfully", salesPersonsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

