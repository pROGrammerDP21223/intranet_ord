using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c!.Industry)
            .Include(p => p.ProductImages)
            .OrderBy(p => p.Category != null ? p.Category.Name : "")
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c!.Industry)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async System.Threading.Tasks.Task<Product> CreateAsync(CreateProductRequest request)
    {
        // Verify category exists
        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category == null || category.IsDeleted)
        {
            throw new KeyNotFoundException("Category not found");
        }

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            MainImage = request.MainImage,
            CategoryId = request.CategoryId
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Add additional images if provided
        if (request.AdditionalImages != null && request.AdditionalImages.Any())
        {
            var productImages = request.AdditionalImages
                .Select((imageUrl, index) => new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = imageUrl,
                    SortOrder = index + 1
                })
                .ToList();

            await _context.ProductImages.AddRangeAsync(productImages);
            await _context.SaveChangesAsync();
        }

        // Reload with related data
        return await GetByIdAsync(product.Id) ?? product;
    }

    public async System.Threading.Tasks.Task<Product> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _context.Products
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (product == null)
        {
            throw new KeyNotFoundException("Product not found");
        }

        // Verify category exists
        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category == null || category.IsDeleted)
        {
            throw new KeyNotFoundException("Category not found");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.MainImage = request.MainImage;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        // Update additional images
        if (request.AdditionalImages != null)
        {
            // Remove existing images
            if (product.ProductImages != null && product.ProductImages.Any())
            {
                _context.ProductImages.RemoveRange(product.ProductImages);
            }

            // Add new images
            if (request.AdditionalImages.Any())
            {
                var productImages = request.AdditionalImages
                    .Select((imageUrl, index) => new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = imageUrl,
                        SortOrder = index + 1
                    })
                    .ToList();

                await _context.ProductImages.AddRangeAsync(productImages);
            }
        }

        await _context.SaveChangesAsync();

        // Reload with related data
        return await GetByIdAsync(product.Id) ?? product;
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.ClientProducts)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (product == null)
        {
            throw new KeyNotFoundException("Product not found");
        }

        // Check if product is attached to clients
        if (product.ClientProducts != null && product.ClientProducts.Any(cp => !cp.IsDeleted))
        {
            // Soft delete
            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Hard delete (will cascade delete ProductImages)
            _context.Products.Remove(product);
        }

        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<Product>> GetProductsByClientIdAsync(int clientId)
    {
        return await _context.ClientProducts
            .Include(cp => cp.Product)
                .ThenInclude(p => p!.Category)
                    .ThenInclude(c => c!.Industry)
            .Include(cp => cp.Product)
                .ThenInclude(p => p!.ProductImages)
            .Where(cp => cp.ClientId == clientId && !cp.IsDeleted && cp.Product != null && !cp.Product.IsDeleted)
            .Select(cp => cp.Product!)
            .ToListAsync();
    }
}

