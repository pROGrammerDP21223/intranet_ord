using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace backend_net.Services;

public class WorkflowService : IWorkflowService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ISignalRNotificationService? _signalRNotificationService;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(
        ApplicationDbContext context,
        IEmailService emailService,
        ISignalRNotificationService? signalRNotificationService,
        ILogger<WorkflowService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _signalRNotificationService = signalRNotificationService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<IEnumerable<Workflow>> GetAllWorkflowsAsync()
    {
        return await _context.Workflows
            .Where(w => !w.IsDeleted)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<Workflow?> GetWorkflowByIdAsync(int id)
    {
        return await _context.Workflows
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
    }

    public async System.Threading.Tasks.Task<Workflow> CreateWorkflowAsync(CreateWorkflowRequest request, int? createdBy = null)
    {
        var workflow = new Workflow
        {
            Name = request.Name,
            Description = request.Description,
            TriggerEntity = request.TriggerEntity,
            TriggerEvent = request.TriggerEvent,
            Conditions = request.Conditions,
            ActionType = request.ActionType,
            ActionConfig = request.ActionConfig,
            IsActive = request.IsActive,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Workflows.AddAsync(workflow);
        await _context.SaveChangesAsync();

        return workflow;
    }

    public async System.Threading.Tasks.Task<Workflow> UpdateWorkflowAsync(int id, UpdateWorkflowRequest request)
    {
        var workflow = await GetWorkflowByIdAsync(id);
        if (workflow == null)
        {
            throw new KeyNotFoundException($"Workflow with ID {id} not found");
        }

        if (!string.IsNullOrEmpty(request.Name))
            workflow.Name = request.Name;

        if (request.Description != null)
            workflow.Description = request.Description;

        if (!string.IsNullOrEmpty(request.TriggerEntity))
            workflow.TriggerEntity = request.TriggerEntity;

        if (!string.IsNullOrEmpty(request.TriggerEvent))
            workflow.TriggerEvent = request.TriggerEvent;

        if (request.Conditions != null)
            workflow.Conditions = request.Conditions;

        if (!string.IsNullOrEmpty(request.ActionType))
            workflow.ActionType = request.ActionType;

        if (request.ActionConfig != null)
            workflow.ActionConfig = request.ActionConfig;

        if (request.IsActive.HasValue)
            workflow.IsActive = request.IsActive.Value;

        workflow.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return workflow;
    }

    public async System.Threading.Tasks.Task<bool> DeleteWorkflowAsync(int id)
    {
        var workflow = await GetWorkflowByIdAsync(id);
        if (workflow == null)
        {
            return false;
        }

        workflow.IsDeleted = true;
        workflow.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async System.Threading.Tasks.Task ExecuteWorkflowsAsync(string triggerEntity, string triggerEvent, int entityId, object? entityData = null)
    {
        try
        {
            var workflows = await _context.Workflows
                .Where(w => !w.IsDeleted && w.IsActive &&
                    w.TriggerEntity == triggerEntity &&
                    w.TriggerEvent == triggerEvent)
                .ToListAsync();

            foreach (var workflow in workflows)
            {
                try
                {
                    // Check conditions if any
                    if (!string.IsNullOrEmpty(workflow.Conditions))
                    {
                        if (!EvaluateConditions(workflow.Conditions, entityData))
                        {
                            continue;
                        }
                    }

                    // Execute action
                    await ExecuteActionAsync(workflow, entityId, entityData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to execute workflow {WorkflowId}", workflow.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflows for {TriggerEntity} {TriggerEvent} {EntityId}", 
                triggerEntity, triggerEvent, entityId);
        }
    }

    private bool EvaluateConditions(string conditionsJson, object? entityData)
    {
        // Simple condition evaluation - can be enhanced
        // For now, return true (no conditions means always execute)
        if (string.IsNullOrEmpty(conditionsJson))
            return true;

        // TODO: Implement condition evaluation logic
        // This would parse the JSON and evaluate against entityData
        return true;
    }

    private async System.Threading.Tasks.Task ExecuteActionAsync(Workflow workflow, int entityId, object? entityData)
    {
        try
        {
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(workflow.ActionConfig) 
                ?? new Dictionary<string, object>();

            switch (workflow.ActionType.ToLower())
            {
                case "createtask":
                    _logger.LogWarning("CreateTask workflow action is no longer supported");
                    break;

                case "sendemail":
                    await ExecuteSendEmailActionAsync(config, entityId, workflow.TriggerEntity);
                    break;

                case "sendnotification":
                    await ExecuteSendNotificationActionAsync(config, entityId, workflow.TriggerEntity);
                    break;

                default:
                    _logger.LogWarning("Unknown action type: {ActionType}", workflow.ActionType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action for workflow {WorkflowId}", workflow.Id);
        }
    }

    private async System.Threading.Tasks.Task ExecuteSendEmailActionAsync(Dictionary<string, object> config, int entityId, string entityType)
    {
        // Email sending logic would go here
        // This would use IEmailService to send emails based on config
        _logger.LogInformation("Email action executed for {EntityType} {EntityId}", entityType, entityId);
    }

    private async System.Threading.Tasks.Task ExecuteSendNotificationActionAsync(Dictionary<string, object> config, int entityId, string entityType)
    {
        if (_signalRNotificationService != null && config.ContainsKey("userId"))
        {
            var userId = int.Parse(config["userId"].ToString() ?? "0");
            var title = config.ContainsKey("title") ? config["title"].ToString() ?? "Notification" : "Notification";
            var message = config.ContainsKey("message") ? config["message"].ToString() ?? "" : "";
            var type = config.ContainsKey("type") ? config["type"].ToString() ?? "info" : "info";

            await _signalRNotificationService.NotifyUserAsync(userId, title, message, type);
        }
    }
}

