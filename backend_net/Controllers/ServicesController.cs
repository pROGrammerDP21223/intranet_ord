using backend_net.Attributes;
using backend_net.Controllers;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ServicesController : BaseController
{
    private readonly IServiceService _serviceService;

    public ServicesController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpGet]
    [RequirePermission(PermissionNames.ViewServices)]
    public async Task<IActionResult> GetAllServices([FromQuery] bool includeInactive = false)
    {
        try
        {
            var services = await _serviceService.GetAllAsync(includeInactive);
            var servicesDto = services.Select(s => new
            {
                s.Id,
                s.ServiceType,
                s.ServiceName,
                s.Category,
                s.SortOrder,
                s.IsActive
            }).ToList();

            return HandleSuccess("Services retrieved successfully", servicesDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("by-category")]
    [RequirePermission(PermissionNames.ViewServices)]
    public async Task<IActionResult> GetServicesByCategory()
    {
        try
        {
            var services = await _serviceService.GetByCategoryAsync();
            return HandleSuccess("Services retrieved successfully", services);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPost]
    [RequirePermission(PermissionNames.CreateServices)]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var service = await _serviceService.CreateAsync(request);

            return HandleSuccess("Service created successfully", new
            {
                service.Id,
                service.ServiceType,
                service.ServiceName,
                service.Category,
                service.IsActive,
                service.SortOrder
            });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPut("{id}")]
    [RequirePermission(PermissionNames.EditServices)]
    public async Task<IActionResult> UpdateService(int id, [FromBody] UpdateServiceRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var service = await _serviceService.UpdateAsync(id, request);

            return HandleSuccess("Service updated successfully", new
            {
                service.Id,
                service.ServiceType,
                service.ServiceName,
                service.Category,
                service.IsActive,
                service.SortOrder
            });
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

    [HttpDelete("{id}")]
    [RequirePermission(PermissionNames.DeleteServices)]
    public async Task<IActionResult> DeleteService(int id)
    {
        try
        {
            await _serviceService.DeleteAsync(id);
            return HandleSuccess("Service deleted successfully");
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
}

