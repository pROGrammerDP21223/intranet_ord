using backend_net.Controllers;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class SalesManagerSalesPersonsController : BaseController
{
    private readonly ISalesManagerSalesPersonService _salesManagerSalesPersonService;

    public SalesManagerSalesPersonsController(ISalesManagerSalesPersonService salesManagerSalesPersonService)
    {
        _salesManagerSalesPersonService = salesManagerSalesPersonService;
    }

    [HttpPost("attach")]
    public async Task<IActionResult> AttachSalesManagerToSalesPerson([FromBody] DTOs.Requests.AttachSalesManagerToSalesPersonRequest request)
    {
        try
        {
            // Only Admin/Owner can manage sales manager-sales person relationships
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can manage sales manager-sales person relationships", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _salesManagerSalesPersonService.AttachSalesManagerToSalesPersonAsync(request);
            return HandleSuccess("Sales manager attached to sales person successfully");
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
    public async Task<IActionResult> DetachSalesManagerFromSalesPerson([FromBody] DTOs.Requests.AttachSalesManagerToSalesPersonRequest request)
    {
        try
        {
            // Only Admin/Owner can manage sales manager-sales person relationships
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can manage sales manager-sales person relationships", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _salesManagerSalesPersonService.DetachSalesManagerFromSalesPersonAsync(request);
            return HandleSuccess("Sales manager detached from sales person successfully");
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

    [HttpGet("salesmanager/{salesManagerId}")]
    public async Task<IActionResult> GetSalesManagerSalesPersons(int salesManagerId)
    {
        try
        {
            // Only Admin/Owner can view sales manager-sales person relationships
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view sales manager-sales person relationships", 403);
            }

            var salesPersons = await _salesManagerSalesPersonService.GetSalesManagerSalesPersonsAsync(salesManagerId);
            var salesPersonsDto = salesPersons.Select(sp => new
            {
                sp.Id,
                sp.Name,
                sp.Email,
                RoleName = sp.Role?.Name
            }).ToList();

            return HandleSuccess("Sales manager sales persons retrieved successfully", salesPersonsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("salesperson/{salesPersonId}")]
    public async Task<IActionResult> GetSalesPersonManagers(int salesPersonId)
    {
        try
        {
            // Only Admin/Owner can view sales manager-sales person relationships
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view sales manager-sales person relationships", 403);
            }

            var salesManagers = await _salesManagerSalesPersonService.GetSalesPersonManagersAsync(salesPersonId);
            var salesManagersDto = salesManagers.Select(sm => new
            {
                sm.Id,
                sm.Name,
                sm.Email,
                RoleName = sm.Role?.Name
            }).ToList();

            return HandleSuccess("Sales person managers retrieved successfully", salesManagersDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

