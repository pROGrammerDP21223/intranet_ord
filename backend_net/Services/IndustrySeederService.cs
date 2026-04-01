using backend_net.Data.Context;
using backend_net.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public static class IndustrySeederService
{
    /// <summary>
    /// Seeds default industries from product catalog. Safe to run repeatedly: only inserts missing names.
    /// </summary>
    public static async System.Threading.Tasks.Task SeedIndustriesAsync(ApplicationDbContext context)
    {
        var existingNames = (await context.Industries
            .AsNoTracking()
            .Select(i => i.Name)
            .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var industries = new List<Industry>
        {
            new() { Name = "Agriculture", Description = "Farming equipment, irrigation systems, and agro solutions.", Image = "https://img.icons8.com/color/96/tractor.png", TopIndustry = true, BannerIndustry = false },
            new() { Name = "Electrical & Electronics", Description = "Electrical components, electronics, and automation systems.", Image = "https://img.icons8.com/color/96/electronics.png", TopIndustry = true, BannerIndustry = false },
            new() { Name = "Mechanical & Engineering", Description = "Industrial machinery and engineering components.", Image = "https://img.icons8.com/color/96/gear.png", TopIndustry = true, BannerIndustry = false },
            new() { Name = "Pharmaceuticals", Description = "Medicines, pharma manufacturing, and lab products.", Image = "https://img.icons8.com/color/96/pill.png", TopIndustry = false, BannerIndustry = false },
            new() { Name = "Rubber & Plastics", Description = "Plastic products, rubber materials, and polymer solutions.", Image = "https://img.icons8.com/color/96/plastic.png", TopIndustry = false, BannerIndustry = false },
            new() { Name = "Packaging & Labeling", Description = "Packaging machinery and labeling solutions.", Image = "https://img.icons8.com/color/96/box.png", TopIndustry = false, BannerIndustry = false },
            new() { Name = "Health Products, Drug and Medicine", Description = "Healthcare products and OTC medicines.", Image = "https://img.icons8.com/color/96/medicine.png", TopIndustry = false, BannerIndustry = false },
            new() { Name = "Chemical & Petrochemical", Description = "Chemicals, petrochemical products, and processing systems.", Image = "https://img.icons8.com/color/96/chemical-plant.png", TopIndustry = false, BannerIndustry = false },
            new() { Name = "Construction & Building Material", Description = "Construction equipment and building materials.", Image = "https://img.icons8.com/color/96/construction.png", TopIndustry = true, BannerIndustry = false },
            new() { Name = "Automobiles", Description = "Vehicles, auto parts, and automotive solutions.", Image = "https://img.icons8.com/color/96/car.png", TopIndustry = true, BannerIndustry = true },
            new() { Name = "Machine & Equipment", Description = "Industrial machines and automation equipment.", Image = "https://img.icons8.com/color/96/factory.png", TopIndustry = true, BannerIndustry = false },
            new() { Name = "Energy & Power", Description = "Power generation and renewable energy systems.", Image = "https://img.icons8.com/color/96/solar-panel.png", TopIndustry = true, BannerIndustry = true },
            new() { Name = "Heating & Thermal System", Description = "Boilers, furnaces, and heating systems.", Image = "https://img.icons8.com/color/96/heating-room.png", TopIndustry = false, BannerIndustry = false },
            new() { Name = "Environmental & Geotechnical", Description = "Environmental protection and geotechnical services.", Image = "https://img.icons8.com/color/96/earth-planet.png", TopIndustry = false, BannerIndustry = false },
            new() { Name = "Technology & Smart Industries", Description = "AI, IoT, automation, and smart technologies.", Image = "https://img.icons8.com/color/96/artificial-intelligence.png", TopIndustry = true, BannerIndustry = true },
            new() { Name = "Education", Description = "Learning institutions and education services.", Image = "https://img.icons8.com/color/96/school.png", TopIndustry = false, BannerIndustry = false },
            new() { Name = "Real Estate", Description = "Residential and commercial real estate services.", Image = "https://img.icons8.com/color/96/city-buildings.png", TopIndustry = false, BannerIndustry = false },
            new() { Name = "Travel & Tourism", Description = "Travel agencies and tourism services.", Image = "https://img.icons8.com/color/96/beach.png", TopIndustry = false, BannerIndustry = true },
            new() { Name = "Medical & Health", Description = "Hospitals, diagnostics, and healthcare services.", Image = "https://img.icons8.com/color/96/hospital.png", TopIndustry = true, BannerIndustry = true },
            new() { Name = "Logistic, Trade & Export", Description = "Logistics, supply chain, and export services.", Image = "https://img.icons8.com/color/96/delivery.png", TopIndustry = false, BannerIndustry = false },
            new() { Name = "Manufacturing & Engineering", Description = "Production, manufacturing, and industrial engineering.", Image = "https://img.icons8.com/color/96/assembly-line.png", TopIndustry = true, BannerIndustry = false },
        };

        var toAdd = industries.Where(i => !existingNames.Contains(i.Name)).ToList();
        if (toAdd.Count == 0)
        {
            Console.WriteLine("[SeedIndustries] All catalog industries already exist. Skipping.");
            return;
        }

        await context.Industries.AddRangeAsync(toAdd);
        await context.SaveChangesAsync();

        Console.WriteLine($"[SeedIndustries] SUCCESS: {toAdd.Count} industries inserted ({industries.Count - toAdd.Count} were already present).");
    }
}
