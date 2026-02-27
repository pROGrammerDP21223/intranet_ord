using backend_net.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Data.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    
    // Client entities
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<ClientService> ClientServices { get; set; } = null!;
    public DbSet<ClientEmailService> ClientEmailServices { get; set; } = null!;
    public DbSet<ClientSeoDetail> ClientSeoDetails { get; set; } = null!;
    public DbSet<ClientAdwordsDetail> ClientAdwordsDetails { get; set; } = null!;
    
    // Service entities
    public DbSet<Service> Services { get; set; } = null!;
    
    // Transaction entities
    public DbSet<Transaction> Transactions { get; set; } = null!;
    
    // Product entities
    public DbSet<Industry> Industries { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductImage> ProductImages { get; set; } = null!;
    public DbSet<ClientProduct> ClientProducts { get; set; } = null!;
    
    // User-Client relationships
    public DbSet<UserClient> UserClients { get; set; } = null!;
    public DbSet<SalesPersonClient> SalesPersonClients { get; set; } = null!;
    public DbSet<SalesManagerSalesPerson> SalesManagerSalesPersons { get; set; } = null!;
    public DbSet<SalesManagerClient> SalesManagerClients { get; set; } = null!;
    public DbSet<OwnerClient> OwnerClients { get; set; } = null!;
    
    // Logging
    public DbSet<Log> Logs { get; set; } = null!;
    
    // Enquiry
    public DbSet<Enquiry> Enquiries { get; set; } = null!;
    
    // API Keys
    public DbSet<ApiKey> ApiKeys { get; set; } = null!;
    
    // Tickets
    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<TicketComment> TicketComments { get; set; } = null!;
    
    // Email Templates
    public DbSet<EmailTemplate> EmailTemplates { get; set; } = null!;
    
    // Audit Logs
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    
    // Documents
    public DbSet<Document> Documents { get; set; } = null!;
    
    // Workflows
    public DbSet<Workflow> Workflows { get; set; } = null!;
    
    // Tasks
    public DbSet<Domain.Entities.Task> Tasks { get; set; } = null!;
    
    // Dashboard Widgets
    public DbSet<DashboardWidget> DashboardWidgets { get; set; } = null!;
    
    // Webhooks
    public DbSet<Webhook> Webhooks { get; set; } = null!;
    public DbSet<WebhookLog> WebhookLogs { get; set; } = null!;
    
    // Internal Messaging
    public DbSet<Message> Messages { get; set; } = null!;
    
    // Calendar & Events
    public DbSet<Event> Events { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
                    .MakeGenericMethod(entityType.ClrType);
                method?.Invoke(null, new object[] { modelBuilder });
            }
        }

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired();
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure PasswordResetToken entity
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Token).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Client entity
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasIndex(e => e.CustomerNo);
            entity.HasIndex(e => e.CreatedByUserId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.CustomerNo).IsRequired();
            entity.Property(e => e.FormDate).IsRequired();
            entity.Property(e => e.CompanyName).IsRequired();
            entity.Property(e => e.ContactPerson).IsRequired();
            
            entity.HasMany(e => e.ClientServices)
                .WithOne(cs => cs.Client)
                .HasForeignKey(cs => cs.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.ClientEmailServices)
                .WithOne(ces => ces.Client)
                .HasForeignKey(ces => ces.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ClientSeoDetail)
                .WithOne(csd => csd.Client)
                .HasForeignKey<ClientSeoDetail>(csd => csd.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ClientAdwordsDetail)
                .WithOne(cad => cad.Client)
                .HasForeignKey<ClientAdwordsDetail>(cad => cad.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Foreign key for CreatedByUserId
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ClientService entity
        modelBuilder.Entity<ClientService>(entity =>
        {
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.ServiceId);
            entity.Property(e => e.ServiceId).IsRequired();
            entity.HasOne(e => e.Client)
                .WithMany(c => c.ClientServices)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Service)
                .WithMany()
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ClientEmailService entity
        modelBuilder.Entity<ClientEmailService>(entity =>
        {
            entity.HasIndex(e => e.ClientId);
            entity.Property(e => e.EmailServiceType).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.HasOne(e => e.Client)
                .WithMany(c => c.ClientEmailServices)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ClientSeoDetail entity
        modelBuilder.Entity<ClientSeoDetail>(entity =>
        {
            entity.HasIndex(e => e.ClientId).IsUnique();
            entity.HasOne(e => e.Client)
                .WithOne(c => c.ClientSeoDetail)
                .HasForeignKey<ClientSeoDetail>(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ClientAdwordsDetail entity
        modelBuilder.Entity<ClientAdwordsDetail>(entity =>
        {
            entity.HasIndex(e => e.ClientId).IsUnique();
            entity.HasOne(e => e.Client)
                .WithOne(c => c.ClientAdwordsDetail)
                .HasForeignKey<ClientAdwordsDetail>(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Service entity
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasIndex(e => e.ServiceType).IsUnique();
            entity.Property(e => e.ServiceType).IsRequired();
            entity.Property(e => e.ServiceName).IsRequired();
        });

        // Configure Transaction entity
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.TransactionNumber).IsUnique();
            entity.Property(e => e.TransactionNumber).IsRequired();
            entity.Property(e => e.TransactionType).IsRequired();
            entity.Property(e => e.TransactionDate).IsRequired();
            entity.Property(e => e.Amount).IsRequired();
            entity.HasOne(e => e.Client)
                .WithMany(c => c.Transactions)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Industry entity
        modelBuilder.Entity<Industry>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.Property(e => e.Name).IsRequired();
            entity.HasMany(e => e.Categories)
                .WithOne(c => c.Industry)
                .HasForeignKey(c => c.IndustryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IndustryId);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.IndustryId).IsRequired();
            entity.HasOne(e => e.Industry)
                .WithMany(i => i.Categories)
                .HasForeignKey(e => e.IndustryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CategoryId);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.MainImage).IsRequired();
            entity.Property(e => e.CategoryId).IsRequired();
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.ProductImages)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ProductImage entity
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasIndex(e => e.ProductId);
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.ImageUrl).IsRequired();
            entity.HasOne(e => e.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ClientProduct entity (many-to-many)
        modelBuilder.Entity<ClientProduct>(entity =>
        {
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => new { e.ClientId, e.ProductId }).IsUnique();
            entity.Property(e => e.ClientId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.HasOne(e => e.Client)
                .WithMany(c => c.ClientProducts)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                .WithMany(p => p.ClientProducts)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure RolePermission entity (explicit configuration to prevent shadow properties)
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasIndex(e => e.RoleId);
            entity.HasIndex(e => e.PermissionId);
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
            entity.Property(e => e.RoleId).IsRequired();
            entity.Property(e => e.PermissionId).IsRequired();
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Permission)
                .WithMany()
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserClient entity (Client User to Client relationship)
        modelBuilder.Entity<UserClient>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => new { e.UserId, e.ClientId }).IsUnique();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.ClientId).IsRequired();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Client)
                .WithMany(c => c.UserClients)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SalesPersonClient entity (Sales Person to multiple Clients)
        modelBuilder.Entity<SalesPersonClient>(entity =>
        {
            entity.HasIndex(e => e.SalesPersonId);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => new { e.SalesPersonId, e.ClientId }).IsUnique();
            entity.Property(e => e.SalesPersonId).IsRequired();
            entity.Property(e => e.ClientId).IsRequired();
            entity.HasOne(e => e.SalesPerson)
                .WithMany()
                .HasForeignKey(e => e.SalesPersonId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Client)
                .WithMany(c => c.SalesPersonClients)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SalesManagerSalesPerson entity (Sales Manager to multiple Sales Persons)
        modelBuilder.Entity<SalesManagerSalesPerson>(entity =>
        {
            entity.HasIndex(e => e.SalesManagerId);
            entity.HasIndex(e => e.SalesPersonId);
            entity.HasIndex(e => new { e.SalesManagerId, e.SalesPersonId }).IsUnique();
            entity.Property(e => e.SalesManagerId).IsRequired();
            entity.Property(e => e.SalesPersonId).IsRequired();
            entity.HasOne(e => e.SalesManager)
                .WithMany()
                .HasForeignKey(e => e.SalesManagerId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cascade cycle
            entity.HasOne(e => e.SalesPerson)
                .WithMany()
                .HasForeignKey(e => e.SalesPersonId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cascade cycle
        });

        // Configure SalesManagerClient entity (Sales Manager to multiple Clients)
        modelBuilder.Entity<SalesManagerClient>(entity =>
        {
            entity.HasIndex(e => e.SalesManagerId);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => new { e.SalesManagerId, e.ClientId }).IsUnique();
            entity.Property(e => e.SalesManagerId).IsRequired();
            entity.Property(e => e.ClientId).IsRequired();
            entity.HasOne(e => e.SalesManager)
                .WithMany()
                .HasForeignKey(e => e.SalesManagerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Client)
                .WithMany(c => c.SalesManagerClients)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure OwnerClient entity (Owner to multiple Clients)
        modelBuilder.Entity<OwnerClient>(entity =>
        {
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => new { e.OwnerId, e.ClientId }).IsUnique();
            entity.Property(e => e.OwnerId).IsRequired();
            entity.Property(e => e.ClientId).IsRequired();
            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Client)
                .WithMany(c => c.OwnerClients)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Enquiry entity
        modelBuilder.Entity<Enquiry>(entity =>
        {
            entity.HasIndex(e => e.EmailId);
            entity.HasIndex(e => e.MobileNumber);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ClientId);
            entity.Property(e => e.FullName).IsRequired();
            entity.Property(e => e.MobileNumber).IsRequired();
            entity.Property(e => e.EmailId).IsRequired();
            entity.Property(e => e.ClientId).IsRequired();
            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ApiKey entity
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasIndex(e => e.ClientId);
            entity.Property(e => e.Key).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.ClientId).IsRequired();
            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Ticket entity
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasIndex(e => e.TicketNumber).IsUnique();
            entity.HasIndex(e => e.CreatedBy);
            entity.HasIndex(e => e.AssignedTo);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.TicketNumber).IsRequired();
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Assignee)
                .WithMany()
                .HasForeignKey(e => e.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(e => e.Comments)
                .WithOne(c => c.Ticket)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TicketComment entity
        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.HasIndex(e => e.TicketId);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.TicketId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Comment).IsRequired();
            entity.HasOne(e => e.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure EmailTemplate entity
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.TemplateType);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Subject).IsRequired();
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.TemplateType).IsRequired();
        });

        // Configure AuditLog entity
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.EntityType).IsRequired();
            entity.Property(e => e.EntityId).IsRequired();
            entity.Property(e => e.Action).IsRequired();
        });

        // Configure Document entity
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.EntityId);
            entity.HasIndex(e => e.UploadedBy);
            entity.HasIndex(e => e.ParentDocumentId);
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.FileType).IsRequired();
            entity.Property(e => e.FileSize).IsRequired();
            entity.Property(e => e.EntityType).IsRequired();
            entity.Property(e => e.EntityId).IsRequired();
            entity.HasOne(e => e.ParentDocument)
                .WithMany()
                .HasForeignKey(e => e.ParentDocumentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure Workflow entity
        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.HasIndex(e => e.TriggerEntity);
            entity.HasIndex(e => e.TriggerEvent);
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.TriggerEntity).IsRequired();
            entity.Property(e => e.TriggerEvent).IsRequired();
            entity.Property(e => e.ActionType).IsRequired();
            entity.Property(e => e.ActionConfig).IsRequired();
        });

        // Configure Task entity
        modelBuilder.Entity<Domain.Entities.Task>(entity =>
        {
            entity.HasIndex(e => e.AssignedTo);
            entity.HasIndex(e => e.CreatedBy);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Priority).IsRequired();
            entity.Property(e => e.AssignedTo).IsRequired();
            entity.HasOne(e => e.Assignee)
                .WithMany()
                .HasForeignKey(e => e.AssignedTo)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure DashboardWidget entity
        modelBuilder.Entity<DashboardWidget>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.WidgetType);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.WidgetType).IsRequired();
            entity.Property(e => e.Title).IsRequired();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Webhook entity
        modelBuilder.Entity<Webhook>(entity =>
        {
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.Url).IsRequired();
            entity.Property(e => e.EventType).IsRequired();
            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure WebhookLog entity
        modelBuilder.Entity<WebhookLog>(entity =>
        {
            entity.HasIndex(e => e.WebhookId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.WebhookId).IsRequired();
            entity.Property(e => e.Url).IsRequired();
            entity.Property(e => e.EventType).IsRequired();
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.HasOne(e => e.Webhook)
                .WithMany()
                .HasForeignKey(e => e.WebhookId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void SetSoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set UpdatedAt on update
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

