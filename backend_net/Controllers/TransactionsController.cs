using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class TransactionsController : BaseController
{
    private readonly ITransactionService _transactionService;
    private readonly IAccessControlService _accessControlService;

    public TransactionsController(ITransactionService transactionService, IAccessControlService accessControlService)
    {
        _transactionService = transactionService;
        _accessControlService = accessControlService;
    }

    /// <summary>
    /// Get all transactions for a specific client
    /// </summary>
    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetByClientId(int clientId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Employee cannot view transactions
            if (IsEmployee())
            {
                return HandleError("Unauthorized: Employees cannot view transaction information", 403);
            }

            // Check if user can access this client
            if (!IsAdmin() && !IsHOD() && !IsCallingStaff() && !await _accessControlService.CanUserAccessClientAsync(userId.Value, clientId))
            {
                return HandleError("Unauthorized: You don't have access to this client's transactions", 403);
            }

            var transactions = await _transactionService.GetByClientIdAsync(clientId);
            var transactionsDto = transactions.Select(t => new
            {
                t.Id,
                t.ClientId,
                t.TransactionType,
                t.TransactionNumber,
                t.TransactionDate,
                t.Amount,
                t.Description,
                t.PaymentMethod,
                t.ReferenceNumber,
                t.Notes,
                t.CreatedAt,
                t.UpdatedAt
            }).ToList();

            return HandleSuccess("Transactions retrieved successfully", transactionsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get a specific transaction by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var transaction = await _transactionService.GetByIdAsync(id);
            if (transaction == null)
            {
                return HandleError("Transaction not found", 404);
            }

            // Employee cannot view transactions
            if (IsEmployee())
            {
                return HandleError("Unauthorized: Employees cannot view transaction information", 403);
            }

            // Check if user can access this client
            if (!IsAdmin() && !IsHOD() && !IsCallingStaff() && !await _accessControlService.CanUserAccessClientAsync(userId.Value, transaction.ClientId))
            {
                return HandleError("Unauthorized: You don't have access to this transaction", 403);
            }

            var transactionDto = new
            {
                transaction.Id,
                transaction.ClientId,
                transaction.TransactionType,
                transaction.TransactionNumber,
                transaction.TransactionDate,
                transaction.Amount,
                transaction.Description,
                transaction.PaymentMethod,
                transaction.ReferenceNumber,
                transaction.Notes,
                transaction.CreatedAt,
                transaction.UpdatedAt
            };

            return HandleSuccess("Transaction retrieved successfully", transactionDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get client balance (sum of all transactions)
    /// </summary>
    [HttpGet("client/{clientId}/balance")]
    public async Task<IActionResult> GetClientBalance(int clientId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Employee cannot view transactions/balance
            if (IsEmployee())
            {
                return HandleError("Unauthorized: Employees cannot view transaction information", 403);
            }

            // Check if user can access this client
            if (!IsAdmin() && !IsHOD() && !IsCallingStaff() && !await _accessControlService.CanUserAccessClientAsync(userId.Value, clientId))
            {
                return HandleError("Unauthorized: You don't have access to this client's balance", 403);
            }

            var balance = await _transactionService.GetClientBalanceAsync(clientId);
            var paidAmount = await _transactionService.GetClientPaidAmountAsync(clientId);
            return HandleSuccess("Balance retrieved successfully", new { clientId, balance, paidAmount });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Create a new transaction
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        try
        {
            // Only Admin/Owner can create transactions
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can create transactions", 403);
            }

            var transaction = await _transactionService.CreateAsync(request);
            var transactionDto = new
            {
                transaction.Id,
                transaction.ClientId,
                transaction.TransactionType,
                transaction.TransactionNumber,
                transaction.TransactionDate,
                transaction.Amount,
                transaction.Description,
                transaction.PaymentMethod,
                transaction.ReferenceNumber,
                transaction.Notes,
                transaction.CreatedAt
            };

            return StatusCode(201, new { message = "Transaction created successfully", data = transactionDto, success = true });
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message, 404);
        }
        catch (InvalidOperationException ex)
        {
            return HandleError(ex.Message, 400);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Update an existing transaction
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, [FromBody] UpdateTransactionRequest request)
    {
        try
        {
            // Only Admin/Owner can update transactions
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can update transactions", 403);
            }

            var transaction = await _transactionService.UpdateAsync(id, request);
            var transactionDto = new
            {
                transaction.Id,
                transaction.ClientId,
                transaction.TransactionType,
                transaction.TransactionNumber,
                transaction.TransactionDate,
                transaction.Amount,
                transaction.Description,
                transaction.PaymentMethod,
                transaction.ReferenceNumber,
                transaction.Notes,
                transaction.UpdatedAt
            };

            return HandleSuccess("Transaction updated successfully", transactionDto);
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
    /// Delete a transaction (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        try
        {
            // Only Admin/Owner can delete transactions
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete transactions", 403);
            }

            var result = await _transactionService.DeleteAsync(id);
            if (!result)
            {
                return HandleError("Transaction not found", 404);
            }

            return HandleSuccess("Transaction deleted successfully");
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

