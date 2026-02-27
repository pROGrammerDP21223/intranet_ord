using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface IWorkflowService
{
    System.Threading.Tasks.Task<IEnumerable<Workflow>> GetAllWorkflowsAsync();
    System.Threading.Tasks.Task<Workflow?> GetWorkflowByIdAsync(int id);
    System.Threading.Tasks.Task<Workflow> CreateWorkflowAsync(CreateWorkflowRequest request, int? createdBy = null);
    System.Threading.Tasks.Task<Workflow> UpdateWorkflowAsync(int id, UpdateWorkflowRequest request);
    System.Threading.Tasks.Task<bool> DeleteWorkflowAsync(int id);
    System.Threading.Tasks.Task ExecuteWorkflowsAsync(string triggerEntity, string triggerEvent, int entityId, object? entityData = null);
}

