using System.Security.Claims;
using System.Text;
using backend_net.Data.Context;
using backend_net.Data.Interfaces;
using backend_net.Data.Repositories;
using backend_net.Domain.Entities;
using backend_net.Services;
using backend_net.Services.Interfaces;
using backend_net.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using AspNetCoreRateLimit;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http.Features;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;


namespace backend_net
{
    public class Program
    {
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            // Configure Serilog
            Serilog.Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build())
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .CreateLogger();

            try
            {
                Serilog.Log.Information("Starting web application");

                var builder = WebApplication.CreateBuilder(args);

                // Use Serilog for logging
                builder.Host.UseSerilog();

                // Add services to the container.

                // Database Configuration
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                    ?? "Server=sqlserver;Database=backend_net_db;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;";

                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                        sqlServerOptions.MigrationsAssembly("backend_net");
                    }));

                // Health Checks
                builder.Services.AddHealthChecks()
                    .AddSqlServer(connectionString, name: "database", tags: new[] { "db", "sqlserver" });

                // Repository Pattern - Unit of Work
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

                // Generic Repository Registration (if needed directly)
                builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

                // Generic Service Registration (if needed directly)
                builder.Services.AddScoped(typeof(IService<>), typeof(Services.Service<>));

                // Authentication Services
                builder.Services.AddScoped<IJwtService, JwtService>();
                builder.Services.AddScoped<IEmailService, EmailService>();
                builder.Services.AddScoped<IAuthService, AuthService>();
                
                // Business Services
                builder.Services.AddScoped<Services.Interfaces.IClientService, Services.ClientService>();
                builder.Services.AddScoped<Services.Interfaces.IServiceService, Services.ServiceService>();
                builder.Services.AddScoped<Services.Interfaces.IRoleService, Services.RoleService>();
                builder.Services.AddScoped<Services.Interfaces.IPermissionService, Services.PermissionService>();
                builder.Services.AddScoped<Services.Interfaces.ITransactionService, Services.TransactionService>();
                builder.Services.AddScoped<Services.Interfaces.IIndustryService, Services.IndustryService>();
                builder.Services.AddScoped<Services.Interfaces.ICategoryService, Services.CategoryService>();
                builder.Services.AddScoped<Services.Interfaces.IProductService, Services.ProductService>();
                builder.Services.AddScoped<Services.Interfaces.IClientProductService, Services.ClientProductService>();
                builder.Services.AddScoped<Services.Interfaces.ILogService, Services.LogService>();
                builder.Services.AddScoped<Services.Interfaces.IUserClientService, Services.UserClientService>();
                builder.Services.AddScoped<Services.Interfaces.ISalesPersonClientService, Services.SalesPersonClientService>();
                builder.Services.AddScoped<Services.Interfaces.ISalesManagerSalesPersonService, Services.SalesManagerSalesPersonService>();
                builder.Services.AddScoped<Services.Interfaces.ISalesManagerClientService, Services.SalesManagerClientService>();
                builder.Services.AddScoped<Services.Interfaces.IOwnerClientService, Services.OwnerClientService>();
                builder.Services.AddScoped<Services.Interfaces.IAccessControlService, Services.AccessControlService>();
                
                // Repository Pattern - Enquiry Repository
                builder.Services.AddScoped<Data.Repositories.IEnquiryRepository, Data.Repositories.EnquiryRepository>();
                
                // Repository Pattern - Analytics Repository
                builder.Services.AddScoped<Data.Repositories.IAnalyticsRepository, Data.Repositories.AnalyticsRepository>();
                
                // Notification System - Strategy Pattern
                builder.Services.AddScoped<Services.Notifications.Interfaces.INotificationHandler, Services.Notifications.Handlers.EmailNotificationHandler>();
                builder.Services.AddScoped<Services.Notifications.Interfaces.INotificationHandler, Services.Notifications.Handlers.WhatsAppNotificationHandler>();
                builder.Services.AddScoped<Services.Notifications.Interfaces.INotificationService, Services.Notifications.NotificationService>();
                
                // Services
                builder.Services.AddScoped<Services.Interfaces.IEnquiryService, Services.EnquiryService>();
                builder.Services.AddScoped<Services.Interfaces.IApiKeyService, Services.ApiKeyService>();
                builder.Services.AddScoped<Services.Interfaces.ISecurityService, Services.SecurityService>();
                builder.Services.AddScoped<Services.Interfaces.ITwilioWhatsAppService, Services.TwilioWhatsAppService>();
                builder.Services.AddScoped<Services.Interfaces.IAnalyticsService, Services.AnalyticsService>();
                builder.Services.AddScoped<Services.Interfaces.IExportService, Services.ExportService>();
                builder.Services.AddScoped<Services.Interfaces.ITicketService, Services.TicketService>();
                builder.Services.AddHttpClient();

                // FluentValidation
                builder.Services.AddFluentValidationAutoValidation();
                builder.Services.AddFluentValidationClientsideAdapters();
                builder.Services.AddValidatorsFromAssemblyContaining<Program>();

                // Rate Limiting
                builder.Services.AddMemoryCache();
                builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
                builder.Services.AddInMemoryRateLimiting();
                builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

                // API Versioning
                builder.Services.AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                });

                builder.Services.AddVersionedApiExplorer(setup =>
                {
                    setup.GroupNameFormat = "'v'VVV";
                    setup.SubstituteApiVersionInUrl = true;
                });

                // JWT Authentication Configuration
                var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!";
                var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "backend_net";
                var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "backend_net";

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                        ValidateIssuer = true,
                        ValidIssuer = jwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = jwtAudience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        RoleClaimType = ClaimTypes.Role
                    };
                });

                // Authorization Policies
                builder.Services.AddAuthorization(options =>
                {
                    // Policies are now permission-based, checked dynamically via RequirePermissionAttribute
                    // Role-based checks are done via AuthorizeRoleAttribute which reads from database
                });

                // CORS Configuration - Environment-based
                var frontendUrl = builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
                
                builder.Services.AddCors(options =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        // More permissive CORS for development - allow any localhost origin
                        options.AddPolicy("AllowFrontend", policy =>
                        {
                            policy.SetIsOriginAllowed(origin => 
                            {
                                // Allow any localhost or 127.0.0.1 origin in development
                                if (string.IsNullOrEmpty(origin)) return false;
                                try
                                {
                                    var uri = new Uri(origin);
                                    var isLocalhost = uri.Scheme == "http" || uri.Scheme == "https" &&
                                                     (uri.Host == "localhost" || 
                                                      uri.Host == "127.0.0.1" || 
                                                      uri.Host == "0.0.0.0" ||
                                                      uri.Host == "[::1]" ||
                                                      uri.Host.Contains("localhost"));
                                    return isLocalhost;
                                }
                                catch
                                {
                                    return false;
                                }
                            })
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .WithExposedHeaders("Content-Disposition", "Content-Length", "Content-Type", "Authorization")
                            .SetPreflightMaxAge(TimeSpan.FromHours(1));
                        });
                    }
                    else
                    {
                        // Strict CORS for production
                        options.AddPolicy("AllowFrontend", policy =>
                        {
                            policy.WithOrigins(frontendUrl)
                                  .AllowAnyHeader()
                                  .AllowAnyMethod()
                                  .AllowCredentials()
                                  .WithExposedHeaders("Content-Disposition", "Content-Length", "Content-Type", "Authorization");
                        });
                    }
                });

                // Hangfire (Background Jobs)
                builder.Services.AddHangfire(config => config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new Hangfire.SqlServer.SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    }));

                builder.Services.AddHangfireServer();

                // SignalR - Configure with CORS support
                builder.Services.AddSignalR(options =>
                {
                    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
                });

                // SignalR Notification Service
                builder.Services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();

                // Export Service

                // Email Template Service
                builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();

                // Audit Service
                builder.Services.AddScoped<IAuditService, AuditService>();

                // Document Service
                builder.Services.AddScoped<IDocumentService, DocumentService>();

                // Workflow Service
                builder.Services.AddScoped<IWorkflowService, WorkflowService>();

                // Message Service (Internal Messaging)
                builder.Services.AddScoped<IMessageService, MessageService>();

                // Cache Service - Use Redis if available and connectable, otherwise use Memory Cache
                var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
                var useRedis = false;
                if (!string.IsNullOrEmpty(redisConnectionString))
                {
                    try
                    {
                        using var testConnection = ConnectionMultiplexer.Connect(redisConnectionString);
                        if (testConnection.IsConnected)
                        {
                            useRedis = true;
                        }
                    }
                    catch
                    {
                        Serilog.Log.Warning("Redis not available, using MemoryCache for caching");
                    }
                }

                if (useRedis)
                {
                    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                        ConnectionMultiplexer.Connect(redisConnectionString!));
                    builder.Services.AddScoped<ICacheService, RedisCacheService>();
                }
                else
                {
                    builder.Services.AddScoped<ICacheService, CacheService>();
                }

                // Background Job Service
                builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();

                // Backup Service
                builder.Services.AddScoped<IBackupService, BackupService>();

                // Archive Service
                builder.Services.AddScoped<IArchiveService, ArchiveService>();

                // Cache Statistics Service
                builder.Services.AddScoped<ICacheStatisticsService, CacheStatisticsService>();

                // Ensure IHttpClientFactory is registered for WebhookService
                builder.Services.AddHttpClient();

                // Data Protection - Configure key persistence for Docker containers
                var dataProtectionKeysPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "DataProtection-Keys");
                if (!Directory.Exists(dataProtectionKeysPath))
                {
                    Directory.CreateDirectory(dataProtectionKeysPath);
                }
                builder.Services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
                    .SetApplicationName("backend_net");

                // Controllers
                builder.Services.AddControllers();
                
                // Configure file upload limits
                builder.Services.Configure<FormOptions>(options =>
                {
                    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
                });

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Backend API",
                        Version = "v1",
                        Description = "Backend API with JWT Authentication",
                        Contact = new OpenApiContact
                        {
                            Name = "One Rank Digital",
                            Email = "support@onerankdigital.com"
                        }
                    });

                    // Add JWT Authentication to Swagger
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

                var app = builder.Build();

                // Seed roles on startup (only when application actually runs, not during migrations)
                // Skip seeding if we detect EF Core design-time operations
                var commandLineArgs = Environment.GetCommandLineArgs();
                var isEfDesignTime = commandLineArgs.Any(arg => 
                    arg.Contains("ef.dll", StringComparison.OrdinalIgnoreCase) || 
                    arg.Contains("design", StringComparison.OrdinalIgnoreCase) ||
                    arg.Contains("dotnet-ef", StringComparison.OrdinalIgnoreCase) ||
                    arg.Contains("migrations", StringComparison.OrdinalIgnoreCase) ||
                    arg.Contains("database", StringComparison.OrdinalIgnoreCase));
                
                // Also check for EF Core design-time service
                var isDesignTime = builder.Environment.EnvironmentName == "Design" ||
                                   builder.Configuration.GetValue<string>("EF_DESIGN_TIME") == "true";
                
                if (!isEfDesignTime && !isDesignTime)
                {
                    try
                    {
                        using (var scope = app.Services.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            
                            // Check if database exists and is accessible
                            if (await context.Database.CanConnectAsync())
                            {
                                // Apply pending migrations automatically
                                try
                                {
                                    await context.Database.MigrateAsync();
                                    Serilog.Log.Information("Database migrations applied successfully");
                                }
                                catch (Exception migrationEx)
                                {
                                    Serilog.Log.Warning(migrationEx, "Failed to apply migrations. This may be normal if migrations are already applied.");
                                }
                                
                                // Update permissions to match current modules (for existing databases)
                                await PermissionUpdateService.UpdatePermissionsToMatchModulesAsync(context);
                                
                                // Seed roles, services, and email templates (only if not already seeded)
                                await RoleSeederService.SeedRolesAsync(context);
                                await ServiceSeederService.SeedServicesAsync(context);
                                await EmailTemplateSeederService.SeedEmailTemplatesAsync(context);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Don't fail application startup if seeding fails
                        Serilog.Log.Warning(ex, "Failed to seed database on startup. This is normal if database doesn't exist yet or migrations are pending.");
                    }
                }

                // Configure the HTTP request pipeline.
                
                // Enable CORS FIRST - must be before any other middleware that might modify response headers
                // CORS must be before UseRouting, UseAuthentication, UseAuthorization
                app.UseCors("AllowFrontend");

                // Security Headers Middleware (early in pipeline, but after CORS)
                app.UseMiddleware<SecurityHeadersMiddleware>();

                // Request Logging Middleware
                app.UseMiddleware<RequestLoggingMiddleware>();

                // Rate Limiting (before authentication)
                var rateLimitEnabled = builder.Configuration.GetValue<bool>("RateLimiting:EnableRateLimiting", false);
                if (rateLimitEnabled && !builder.Environment.IsDevelopment())
                {
                    app.UseIpRateLimiting();
                }

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend API v1");
                        c.RoutePrefix = string.Empty; // Swagger UI at root
                    });
                }

                // Only use HTTPS redirection if HTTPS is configured
                var httpsPort = builder.Configuration["HTTPS_PORT"];
                if (!string.IsNullOrEmpty(httpsPort))
                {
                    app.UseHttpsRedirection();
                }

                app.UseAuthentication();
                app.UseAuthorization();

                // Health Checks
                app.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                app.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                app.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    Predicate = _ => false,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                // Global Exception Handler (should be after authentication/authorization)
                app.UseMiddleware<ExceptionHandlerMiddleware>();

                // Hangfire Dashboard (Admin/Owner only - will be protected by authorization)
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new HangfireAuthorizationFilter() },
                    DashboardTitle = "Background Jobs Dashboard"
                });

                // Create wwwroot directory if it doesn't exist (fixes WebRootPath warning)
                var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
                if (!Directory.Exists(wwwrootPath))
                {
                    Directory.CreateDirectory(wwwrootPath);
                }

                // Serve frontend (wwwroot) and uploaded files
                app.UseDefaultFiles();
                app.UseStaticFiles();
                
                // Serve uploaded images
                var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
                    RequestPath = "/uploads"
                });

                // SignalR Hub (must be mapped before MapControllers for proper routing)
                app.MapHub<Hubs.NotificationHub>("/notificationHub");
                
                app.MapControllers();
                app.MapFallbackToFile("index.html");

                Serilog.Log.Information("Application started successfully. Environment: {Environment}", app.Environment.EnvironmentName);

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal(ex, "Application failed to start");
                throw;
            }
            finally
            {
                Serilog.Log.CloseAndFlush();
            }
        }
    }
}
