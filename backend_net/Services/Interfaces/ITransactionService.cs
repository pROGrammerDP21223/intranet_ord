using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface ITransactionService
{
    Task<IEnumerable<Transaction>> GetByClientIdAsync(int clientId);
    Task<Transaction?> GetByIdAsync(int id);
    Task<Transaction> CreateAsync(CreateTransactionRequest request);
    Task<Transaction> UpdateAsync(int id, UpdateTransactionRequest request);
    Task<bool> DeleteAsync(int id);
    Task<decimal> GetClientBalanceAsync(int clientId);
    Task<decimal> GetClientPaidAmountAsync(int clientId);
    Task<bool> TransactionNumberExistsAsync(string transactionNumber, int? excludeTransactionId = null);
}

