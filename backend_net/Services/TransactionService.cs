using backend_net.Data.Context;
using backend_net.Data.Interfaces;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly ISignalRNotificationService? _signalRNotificationService;

    public TransactionService(
        IUnitOfWork unitOfWork, 
        ApplicationDbContext context, 
        ISignalRNotificationService? signalRNotificationService = null)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _signalRNotificationService = signalRNotificationService;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Transaction>> GetByClientIdAsync(int clientId)
    {
        return await _context.Transactions
            .Where(t => t.ClientId == clientId && !t.IsDeleted)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<Transaction?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Repository<Transaction>()
            .GetByIdAsync(id);
    }

    public async System.Threading.Tasks.Task<Transaction> CreateAsync(CreateTransactionRequest request)
    {
        // Verify client exists
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == request.ClientId && !c.IsDeleted);
        
        if (client == null)
        {
            throw new KeyNotFoundException($"Client with ID {request.ClientId} not found");
        }

        // Generate unique transaction number
        var transactionNumber = await GenerateUniqueTransactionNumberAsync();

        // Adjust amount based on transaction type
        // Payments increase balance (positive), Refunds decrease balance (negative)
        var amount = request.Amount;
        if (request.TransactionType == TransactionTypes.Refund || 
            request.TransactionType == TransactionTypes.Debit)
        {
            amount = -Math.Abs(amount); // Ensure negative for refunds/debits
        }
        else
        {
            amount = Math.Abs(amount); // Ensure positive for payments/credits
        }

        var transaction = new Transaction
        {
            ClientId = request.ClientId,
            TransactionType = request.TransactionType,
            TransactionNumber = transactionNumber,
            TransactionDate = request.TransactionDate,
            Amount = amount,
            Description = request.Description,
            PaymentMethod = request.PaymentMethod,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Transaction>().AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        // Send SignalR notification
        if (_signalRNotificationService != null)
        {
            await _signalRNotificationService.NotifyTransactionUpdateAsync(transaction, "created");
        }

        return transaction;
    }

    public async System.Threading.Tasks.Task<Transaction> UpdateAsync(int id, UpdateTransactionRequest request)
    {
        var transaction = await _unitOfWork.Repository<Transaction>().GetByIdAsync(id);
        if (transaction == null)
        {
            throw new KeyNotFoundException($"Transaction with ID {id} not found");
        }

        // Adjust amount based on transaction type
        var amount = request.Amount;
        if (request.TransactionType == TransactionTypes.Refund || 
            request.TransactionType == TransactionTypes.Debit)
        {
            amount = -Math.Abs(amount);
        }
        else
        {
            amount = Math.Abs(amount);
        }

        transaction.TransactionType = request.TransactionType;
        transaction.TransactionDate = request.TransactionDate;
        transaction.Amount = amount;
        transaction.Description = request.Description;
        transaction.PaymentMethod = request.PaymentMethod;
        transaction.ReferenceNumber = request.ReferenceNumber;
        transaction.Notes = request.Notes;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Transaction>().UpdateAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        // Send SignalR notification
        if (_signalRNotificationService != null)
        {
            await _signalRNotificationService.NotifyTransactionUpdateAsync(transaction, "updated");
        }

        return transaction;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
    {
        var transaction = await _unitOfWork.Repository<Transaction>().GetByIdAsync(id);
        if (transaction == null)
        {
            return false;
        }

        transaction.IsDeleted = true;
        transaction.UpdatedAt = DateTime.UtcNow;
        transaction.UpdatedBy = "System"; // TODO: Get from current user context

        await _unitOfWork.Repository<Transaction>().UpdateAsync(transaction);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async System.Threading.Tasks.Task<decimal> GetClientBalanceAsync(int clientId)
    {
        var balance = await _context.Transactions
            .Where(t => t.ClientId == clientId && !t.IsDeleted)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        return balance;
    }

    public async System.Threading.Tasks.Task<decimal> GetClientPaidAmountAsync(int clientId)
    {
        // Sum of all positive transactions (payments, credits)
        var paidAmount = await _context.Transactions
            .Where(t => t.ClientId == clientId && !t.IsDeleted && t.Amount > 0)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        return paidAmount;
    }

    public async System.Threading.Tasks.Task<bool> TransactionNumberExistsAsync(string transactionNumber, int? excludeTransactionId = null)
    {
        var query = _context.Transactions
            .Where(t => t.TransactionNumber == transactionNumber && !t.IsDeleted);

        if (excludeTransactionId.HasValue)
        {
            query = query.Where(t => t.Id != excludeTransactionId.Value);
        }

        return await query.AnyAsync();
    }

    private async Task<string> GenerateUniqueTransactionNumberAsync()
    {
        string transactionNumber;
        int attempts = 0;
        const int maxAttempts = 10;

        do
        {
            // Format: TXN-YYYYMMDD-HHMMSS-XXXX (where XXXX is random 4-digit number)
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var random = new Random().Next(1000, 9999);
            transactionNumber = $"TXN-{timestamp}-{random}";

            attempts++;
            if (attempts >= maxAttempts)
            {
                throw new InvalidOperationException("Failed to generate unique transaction number after multiple attempts");
            }
        } while (await TransactionNumberExistsAsync(transactionNumber));

        return transactionNumber;
    }
}

