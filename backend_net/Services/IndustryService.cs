using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public class IndustryService : IIndustryService
{
    private readonly ApplicationDbContext _context;

    public IndustryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Industry>> GetAllAsync()
    {
        return await _context.Industries
            .Include(i => i.Categories)
            .OrderBy(i => i.Name)
            .ToListAsync();
    }

    public async Task<Industry?> GetByIdAsync(int id)
    {
        return await _context.Industries
            .Include(i => i.Categories)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
    }

    public async Task<Industry> CreateAsync(CreateIndustryRequest request)
    {
        var industry = new Industry
        {
            Name = request.Name,
            Description = request.Description,
            Image = request.Image,
            TopIndustry = request.TopIndustry,
            BannerIndustry = request.BannerIndustry
        };

        await _context.Industries.AddAsync(industry);
        await _context.SaveChangesAsync();

        return industry;
    }

    public async Task<Industry> UpdateAsync(int id, UpdateIndustryRequest request)
    {
        var industry = await _context.Industries.FindAsync(id);
        if (industry == null || industry.IsDeleted)
        {
            throw new KeyNotFoundException("Industry not found");
        }

        industry.Name = request.Name;
        industry.Description = request.Description;
        industry.Image = request.Image;
        industry.TopIndustry = request.TopIndustry;
        industry.BannerIndustry = request.BannerIndustry;
        industry.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return industry;
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        var industry = await _context.Industries
            .Include(i => i.Categories)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

        if (industry == null)
        {
            throw new KeyNotFoundException("Industry not found");
        }

        // Check if industry has categories
        if (industry.Categories != null && industry.Categories.Any(c => !c.IsDeleted))
        {
            // Soft delete
            industry.IsDeleted = true;
            industry.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Hard delete
            _context.Industries.Remove(industry);
        }

        await _context.SaveChangesAsync();
    }
}

