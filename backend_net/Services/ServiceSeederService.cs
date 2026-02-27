using backend_net.Data.Context;
using backend_net.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public static class ServiceSeederService
{
    public static async System.Threading.Tasks.Task SeedServicesAsync(ApplicationDbContext context)
    {
        if (await context.Services.AnyAsync())
        {
            Console.WriteLine("[SeedServices] Services already exist. Skipping seeding.");
            return;
        }

        var services = new List<Service>
        {
            // Domain & Hosting
            new Service { ServiceType = "domain-hosting", ServiceName = "Domain & Hosting", Category = "Domain & Hosting", SortOrder = 1, IsActive = true },
            
            // Web Design
            new Service { ServiceType = "website-design-development", ServiceName = "Website Design / Development", Category = "Web Design", SortOrder = 2, IsActive = true },
            new Service { ServiceType = "website-maintenance", ServiceName = "Website Maintenance", Category = "Web Design", SortOrder = 3, IsActive = true },
            new Service { ServiceType = "app-development", ServiceName = "App Development", Category = "Web Design", SortOrder = 4, IsActive = true },
            
            // SEO
            new Service { ServiceType = "seo", ServiceName = "Search Engine Optimization", Category = "SEO", SortOrder = 5, IsActive = true },
            new Service { ServiceType = "google-ads", ServiceName = "Google Ads / PPC", Category = "SEO", SortOrder = 6, IsActive = true },
            new Service { ServiceType = "google-my-business", ServiceName = "Google My Business (Local)", Category = "SEO", SortOrder = 7, IsActive = true },
            
            // Additional Services
            new Service { ServiceType = "ai-chatbot", ServiceName = "AI Chatbot", Category = "Additional Services", SortOrder = 8, IsActive = true },
            new Service { ServiceType = "youtube-promotion", ServiceName = "YouTube Promotion", Category = "Additional Services", SortOrder = 9, IsActive = true },
            new Service { ServiceType = "email-marketing", ServiceName = "Email Marketing", Category = "Additional Services", SortOrder = 10, IsActive = true },
        };

        await context.Services.AddRangeAsync(services);
        await context.SaveChangesAsync();
        
        Console.WriteLine($"[SeedServices] SUCCESS: {services.Count} services seeded successfully.");
    }
}

