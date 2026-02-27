using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WorkflowsController : BaseController
{
    private readonly IWorkflowService _workflowService;

    public WorkflowsController(IWorkflowService workflowService)
    {
        _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
    }

    /// <summary>
    /// Get all workflows
    /// Only Admin and Owner can view workflows
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllWorkflows()
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view workflows", 403);
            }

            var workflows = await _workflowService.GetAllWorkflowsAsync();
            return HandleSuccess("Workflows retrieved successfully", workflows);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get workflow by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkflowById(int id)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can view workflows", 403);
            }

            var workflow = await _workflowService.GetWorkflowByIdAsync(id);
            if (workflow == null)
            {
                return HandleError("Workflow not found", 404);
            }
            return HandleSuccess("Workflow retrieved successfully", workflow);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Create new workflow
    /// Only Admin and Owner can create workflows
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWorkflow([FromBody] CreateWorkflowRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can create workflows", 403);
            }

            var userId = GetCurrentUserId();
            var workflow = await _workflowService.CreateWorkflowAsync(request, userId);
            return StatusCode(201, new { message = "Workflow created successfully", data = workflow, success = true });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Update workflow
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkflow(int id, [FromBody] UpdateWorkflowRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can update workflows", 403);
            }

            var workflow = await _workflowService.UpdateWorkflowAsync(id, request);
            return HandleSuccess("Workflow updated successfully", workflow);
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

    /// <summary>
    /// Delete workflow
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkflow(int id)
    {
        try
        {
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete workflows", 403);
            }

            var result = await _workflowService.DeleteWorkflowAsync(id);
            if (!result)
            {
                return HandleError("Workflow not found", 404);
            }
            return HandleSuccess("Workflow deleted successfully", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

