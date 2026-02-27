using backend_net.Controllers;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class CategoriesController : BaseController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllAsync();
            var categoriesDto = categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.Image,
                c.IndustryId,
                IndustryName = c.Industry?.Name,
                ProductsCount = c.Products?.Count(p => !p.IsDeleted) ?? 0
            }).ToList();

            return HandleSuccess("Categories retrieved successfully", categoriesDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("by-industry/{industryId}")]
    public async Task<IActionResult> GetCategoriesByIndustry(int industryId)
    {
        try
        {
            var categories = await _categoryService.GetByIndustryIdAsync(industryId);
            var categoriesDto = categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.Image,
                c.IndustryId,
                IndustryName = c.Industry?.Name,
                ProductsCount = c.Products?.Count(p => !p.IsDeleted) ?? 0
            }).ToList();

            return HandleSuccess("Categories retrieved successfully", categoriesDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return HandleError("Category not found", 404);
            }

            var categoryDto = new
            {
                category.Id,
                category.Name,
                category.Description,
                category.Image,
                category.IndustryId,
                Industry = category.Industry != null ? new
                {
                    category.Industry.Id,
                    category.Industry.Name,
                    category.Industry.Description,
                    category.Industry.Image
                } : null,
                ProductsCount = category.Products?.Count(p => !p.IsDeleted) ?? 0
            };

            return HandleSuccess("Category retrieved successfully", categoryDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] DTOs.Requests.CreateCategoryRequest request)
    {
        try
        {
            // Only Admin/Owner can create categories
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can create categories", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _categoryService.CreateAsync(request);

            return HandleSuccess("Category created successfully", new
            {
                category.Id,
                category.Name,
                category.Description,
                category.Image,
                category.IndustryId
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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] DTOs.Requests.UpdateCategoryRequest request)
    {
        try
        {
            // Only Admin/Owner can update categories
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can update categories", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _categoryService.UpdateAsync(id, request);

            return HandleSuccess("Category updated successfully", new
            {
                category.Id,
                category.Name,
                category.Description,
                category.Image,
                category.IndustryId
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
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            // Only Admin/Owner can delete categories
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete categories", 403);
            }

            await _categoryService.DeleteAsync(id);
            return HandleSuccess("Category deleted successfully");
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

