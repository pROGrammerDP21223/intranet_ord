using backend_net.Controllers;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class IndustriesController : BaseController
{
    private readonly IIndustryService _industryService;

    public IndustriesController(IIndustryService industryService)
    {
        _industryService = industryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllIndustries()
    {
        try
        {
            var industries = await _industryService.GetAllAsync();
            var industriesDto = industries.Select(i => new
            {
                i.Id,
                i.Name,
                i.Description,
                i.Image,
                i.TopIndustry,
                i.BannerIndustry,
                CategoriesCount = i.Categories?.Count(c => !c.IsDeleted) ?? 0
            }).ToList();

            return HandleSuccess("Industries retrieved successfully", industriesDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetIndustryById(int id)
    {
        try
        {
            var industry = await _industryService.GetByIdAsync(id);
            if (industry == null)
            {
                return HandleError("Industry not found", 404);
            }

            var categoriesList = industry.Categories?.Where(c => !c.IsDeleted).Select(c => (object)new
            {
                c.Id,
                c.Name,
                c.Description,
                c.Image
            }).ToList() ?? new List<object>();

            var industryDto = new
            {
                industry.Id,
                industry.Name,
                industry.Description,
                industry.Image,
                industry.TopIndustry,
                industry.BannerIndustry,
                Categories = categoriesList
            };

            return HandleSuccess("Industry retrieved successfully", industryDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateIndustry([FromBody] DTOs.Requests.CreateIndustryRequest request)
    {
        try
        {
            // Only Admin/Owner can create industries
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can create industries", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var industry = await _industryService.CreateAsync(request);

            return HandleSuccess("Industry created successfully", new
            {
                industry.Id,
                industry.Name,
                industry.Description,
                industry.Image,
                industry.TopIndustry,
                industry.BannerIndustry
            });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateIndustry(int id, [FromBody] DTOs.Requests.UpdateIndustryRequest request)
    {
        try
        {
            // Only Admin/Owner can update industries
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can update industries", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var industry = await _industryService.UpdateAsync(id, request);

            return HandleSuccess("Industry updated successfully", new
            {
                industry.Id,
                industry.Name,
                industry.Description,
                industry.Image,
                industry.TopIndustry,
                industry.BannerIndustry
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
    public async Task<IActionResult> DeleteIndustry(int id)
    {
        try
        {
            // Only Admin/Owner can delete industries
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete industries", 403);
            }

            await _industryService.DeleteAsync(id);
            return HandleSuccess("Industry deleted successfully");
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

