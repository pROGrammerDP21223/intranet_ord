using backend_net.Controllers;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ProductsController : BaseController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var products = await _productService.GetAllAsync();
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

            return HandleSuccess("Products retrieved successfully", productsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("by-category/{categoryId}")]
    public async Task<IActionResult> GetProductsByCategory(int categoryId)
    {
        try
        {
            var products = await _productService.GetByCategoryIdAsync(categoryId);
            var productsDto = products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.MainImage,
                p.CategoryId,
                CategoryName = p.Category?.Name,
                AdditionalImages = p.ProductImages?.OrderBy(pi => pi.SortOrder).Select(pi => pi.ImageUrl).ToList() ?? new List<string>()
            }).ToList();

            return HandleSuccess("Products retrieved successfully", productsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("by-client/{clientId}")]
    public async Task<IActionResult> GetProductsByClient(int clientId)
    {
        try
        {
            var products = await _productService.GetProductsByClientIdAsync(clientId);
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

            return HandleSuccess("Products retrieved successfully", productsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return HandleError("Product not found", 404);
            }

            var productDto = new
            {
                product.Id,
                product.Name,
                product.Description,
                product.MainImage,
                product.CategoryId,
                Category = product.Category != null ? new
                {
                    product.Category.Id,
                    product.Category.Name,
                    product.Category.Description,
                    product.Category.Image,
                    Industry = product.Category.Industry != null ? new
                    {
                        product.Category.Industry.Id,
                        product.Category.Industry.Name
                    } : null
                } : null,
                AdditionalImages = product.ProductImages?.OrderBy(pi => pi.SortOrder).Select(pi => (object)new
                {
                    pi.Id,
                    pi.ImageUrl,
                    pi.SortOrder
                }).ToList() ?? new List<object>()
            };

            return HandleSuccess("Product retrieved successfully", productDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] DTOs.Requests.CreateProductRequest request)
    {
        try
        {
            // Only Admin/Owner can create products
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can create products", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _productService.CreateAsync(request);

            return HandleSuccess("Product created successfully", new
            {
                product.Id,
                product.Name,
                product.Description,
                product.MainImage,
                product.CategoryId,
                AdditionalImages = product.ProductImages?.OrderBy(pi => pi.SortOrder).Select(pi => pi.ImageUrl).ToList() ?? new List<string>()
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
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] DTOs.Requests.UpdateProductRequest request)
    {
        try
        {
            // Only Admin/Owner can update products
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can update products", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _productService.UpdateAsync(id, request);

            return HandleSuccess("Product updated successfully", new
            {
                product.Id,
                product.Name,
                product.Description,
                product.MainImage,
                product.CategoryId,
                AdditionalImages = product.ProductImages?.OrderBy(pi => pi.SortOrder).Select(pi => pi.ImageUrl).ToList() ?? new List<string>()
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
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            // Only Admin/Owner can delete products
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete products", 403);
            }

            await _productService.DeleteAsync(id);
            return HandleSuccess("Product deleted successfully");
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

