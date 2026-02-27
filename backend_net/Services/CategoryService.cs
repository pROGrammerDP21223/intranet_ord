using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.Industry)
            .Include(c => c.Products)
            .OrderBy(c => c.Industry != null ? c.Industry.Name : "")
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetByIndustryIdAsync(int industryId)
    {
        return await _context.Categories
            .Include(c => c.Industry)
            .Where(c => c.IndustryId == industryId && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Industry)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<Category> CreateAsync(CreateCategoryRequest request)
    {
        // Verify industry exists
        var industry = await _context.Industries.FindAsync(request.IndustryId);
        if (industry == null || industry.IsDeleted)
        {
            throw new KeyNotFoundException("Industry not found");
        }

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            Image = request.Image,
            IndustryId = request.IndustryId
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        return category;
    }

    public async Task<Category> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null || category.IsDeleted)
        {
            throw new KeyNotFoundException("Category not found");
        }

        // Verify industry exists
        var industry = await _context.Industries.FindAsync(request.IndustryId);
        if (industry == null || industry.IsDeleted)
        {
            throw new KeyNotFoundException("Industry not found");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.Image = request.Image;
        category.IndustryId = request.IndustryId;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return category;
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        // Check if category has products
        if (category.Products != null && category.Products.Any(p => !p.IsDeleted))
        {
            // Soft delete
            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Hard delete
            _context.Categories.Remove(category);
        }

        await _context.SaveChangesAsync();
    }
}

