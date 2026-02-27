import { useState, useEffect } from 'react';
import { transactionAPI } from '../services/api';

const ClientTransactions = ({ clientId, totalPackage = 0 }) => {
  const [transactions, setTransactions] = useState([]);
  const [balance, setBalance] = useState(0);
  const [paidAmount, setPaidAmount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [showTransactions, setShowTransactions] = useState(false);
  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedTransaction, setSelectedTransaction] = useState(null);
  const [formData, setFormData] = useState({
    transactionType: 'Payment',
    transactionDate: new Date().toISOString().split('T')[0],
    amount: '',
    description: '',
    paymentMethod: '',
    referenceNumber: '',
    notes: '',
  });

  useEffect(() => {
    loadTransactions();
    loadBalance();
  }, [clientId]);

  const loadTransactions = async () => {
    try {
      setLoading(true);
      const response = await transactionAPI.getTransactionsByClient(clientId);
      if (response.success && response.data) {
        setTransactions(response.data);
      }
    } catch (err) {
      alert('Failed to load transactions');
      console.error('Failed to load transactions:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadBalance = async () => {
    try {
      const response = await transactionAPI.getClientBalance(clientId);
      if (response.success && response.data) {
        setBalance(response.data.balance || 0);
        setPaidAmount(response.data.paidAmount || 0);
      }
    } catch (err) {
      console.error('Failed to load balance:', err);
    }
  };

  const handleAddTransaction = () => {
    setFormData({
      transactionType: 'Payment',
      transactionDate: new Date().toISOString().split('T')[0],
      amount: '',
      description: '',
      paymentMethod: '',
      referenceNumber: '',
      notes: '',
    });
    setSelectedTransaction(null);
    setShowAddModal(true);
  };

  const handleEditTransaction = (transaction) => {
    setSelectedTransaction(transaction);
    setFormData({
      transactionType: transaction.transactionType,
      transactionDate: transaction.transactionDate.split('T')[0],
      amount: Math.abs(transaction.amount).toString(),
      description: transaction.description || '',
      paymentMethod: transaction.paymentMethod || '',
      referenceNumber: transaction.referenceNumber || '',
      notes: transaction.notes || '',
    });
    setShowAddModal(false);
    setShowEditModal(true);
  };

  const handleDeleteTransaction = async (id) => {
    if (!window.confirm('Are you sure you want to delete this transaction?')) {
      return;
    }

    try {
      const response = await transactionAPI.deleteTransaction(id);
      if (response.success) {
        alert('Transaction deleted successfully');
        loadTransactions();
        loadBalance();
      } else {
        alert(response.message || 'Failed to delete transaction');
      }
    } catch (err) {
      alert('An error occurred while deleting transaction');
      console.error('Failed to delete transaction:', err);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const transactionData = {
        clientId: parseInt(clientId),
        ...formData,
        amount: parseFloat(formData.amount),
        transactionDate: new Date(formData.transactionDate).toISOString(),
      };

      let response;
      if (selectedTransaction) {
        response = await transactionAPI.updateTransaction(selectedTransaction.id, transactionData);
      } else {
        response = await transactionAPI.createTransaction(transactionData);
      }

      if (response.success) {
        alert(`Transaction ${selectedTransaction ? 'updated' : 'created'} successfully`);
        setShowAddModal(false);
        setShowEditModal(false);
        setSelectedTransaction(null);
        loadTransactions();
        loadBalance();
      } else {
        alert(response.message || `Failed to ${selectedTransaction ? 'update' : 'create'} transaction`);
      }
    } catch (err) {
      alert(`An error occurred while ${selectedTransaction ? 'updating' : 'creating'} transaction`);
      console.error('Transaction error:', err);
    }
  };

  const formatDate = (dateString) => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB');
  };

  const formatCurrency = (amount) => {
    if (amount === null || amount === undefined) return '₹0.00';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 2
    }).format(amount);
  };

  const getAmountClass = (amount) => {
    if (amount > 0) return 'text-success';
    if (amount < 0) return 'text-danger';
    return '';
  };

  const pendingAmount = (totalPackage || 0) - paidAmount;

  return (
    <>
      {/* Payment Summary Cards */}
      <div className="row mb-3">
        <div className="col-md-4">
          <div className="card border-success">
            <div className="card-body">
              <h6 className="text-muted mb-2">Paid Amount</h6>
              <h3 className="mb-0 text-success">
                {formatCurrency(paidAmount)}
              </h3>
            </div>
          </div>
        </div>
        <div className="col-md-4">
          <div className="card border-warning">
            <div className="card-body">
              <h6 className="text-muted mb-2">Pending Amount</h6>
              <h3 className={`mb-0 ${pendingAmount > 0 ? 'text-warning' : 'text-success'}`}>
                {formatCurrency(pendingAmount)}
              </h3>
            </div>
          </div>
        </div>
        <div className="col-md-4">
          <div className="card border-primary">
            <div className="card-body">
              <h6 className="text-muted mb-2">Total Package</h6>
              <h3 className="mb-0 text-primary">
                {formatCurrency(totalPackage)}
              </h3>
            </div>
          </div>
        </div>
      </div>

      {/* Transactions Toggle Button */}
      <div className="row mb-3">
        <div className="col-12">
          <button 
            className="btn btn-primary"
            onClick={() => setShowTransactions(!showTransactions)}
          >
            <i className={`fas fa-${showTransactions ? 'chevron-up' : 'chevron-down'} me-2`}></i>
            {showTransactions ? 'Hide' : 'Show'} Transaction List
            {transactions.length > 0 && (
              <span className="badge bg-light text-dark ms-2">{transactions.length}</span>
            )}
          </button>
        </div>
      </div>

      {/* Transactions Card */}
      {showTransactions && (
        <div className="card">
          <div className="card-header">
            <div className="row align-items-center">
              <div className="col">
                <h4 className="card-title mb-0">Transactions</h4>
              </div>
              <div className="col-auto">
                <button className="btn btn-primary btn-sm" onClick={handleAddTransaction}>
                  <i className="fas fa-plus me-1"></i>Add Transaction
                </button>
              </div>
            </div>
          </div>
          <div className="card-body">
          {loading ? (
            <div className="text-center py-4">
              <div className="spinner-border text-primary" role="status">
                <span className="visually-hidden">Loading...</span>
              </div>
            </div>
          ) : transactions.length === 0 ? (
            <div className="text-center py-5">
              <i className="fas fa-receipt fa-4x text-muted mb-3 d-block" style={{ opacity: 0.5 }}></i>
              <p className="text-muted mb-0" style={{ fontSize: '1.1rem' }}>No transactions found</p>
            </div>
          ) : (
            <div className="table-responsive">
              <table className="table table-bordered table-hover table-nowrap">
                <thead className="table-light">
                  <tr>
                    <th>Date</th>
                    <th>Transaction #</th>
                    <th>Type</th>
                    <th>Description</th>
                    <th>Payment Method</th>
                    <th className="text-end">Amount</th>
                    <th className="text-center" style={{ width: '120px' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {transactions.map((transaction) => (
                    <tr key={transaction.id}>
                      <td>{formatDate(transaction.transactionDate)}</td>
                      <td>
                        <code>{transaction.transactionNumber}</code>
                      </td>
                      <td>
                        <span className={`badge ${
                          transaction.transactionType === 'Payment' || transaction.transactionType === 'Credit'
                            ? 'bg-success'
                            : transaction.transactionType === 'Refund' || transaction.transactionType === 'Debit'
                            ? 'bg-danger'
                            : 'bg-warning'
                        }`}>
                          {transaction.transactionType}
                        </span>
                      </td>
                      <td>{transaction.description || '-'}</td>
                      <td>{transaction.paymentMethod || '-'}</td>
                      <td className={`text-end ${getAmountClass(transaction.amount)}`}>
                        <strong>{formatCurrency(transaction.amount)}</strong>
                      </td>
                      <td className="text-center">
                        <div className="d-flex gap-1 justify-content-center">
                          <button
                            className="btn btn-info btn-sm"
                            onClick={() => handleEditTransaction(transaction)}
                            title="Edit"
                          >
                            <i className="fas fa-edit"></i>
                          </button>
                          <button
                            className="btn btn-danger btn-sm"
                            onClick={() => handleDeleteTransaction(transaction.id)}
                            title="Delete"
                          >
                            <i className="fas fa-trash"></i>
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
      )}

      {/* Add Transaction Modal */}
      {showAddModal && (
        <div className="modal show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Add Transaction</h5>
                <button type="button" className="btn-close" onClick={() => setShowAddModal(false)}></button>
              </div>
              <form onSubmit={handleSubmit}>
                <div className="modal-body">
                  <div className="row">
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Transaction Type <span className="text-danger">*</span></label>
                      <select
                        className="form-select"
                        value={formData.transactionType}
                        onChange={(e) => setFormData({ ...formData, transactionType: e.target.value })}
                        required
                      >
                        <option value="Payment">Payment</option>
                        <option value="Refund">Refund</option>
                        <option value="Credit">Credit</option>
                        <option value="Debit">Debit</option>
                        <option value="Adjustment">Adjustment</option>
                      </select>
                    </div>
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Transaction Date <span className="text-danger">*</span></label>
                      <input
                        type="date"
                        className="form-control"
                        value={formData.transactionDate}
                        onChange={(e) => setFormData({ ...formData, transactionDate: e.target.value })}
                        required
                      />
                    </div>
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Amount <span className="text-danger">*</span></label>
                      <input
                        type="number"
                        className="form-control"
                        step="0.01"
                        min="0.01"
                        value={formData.amount}
                        onChange={(e) => setFormData({ ...formData, amount: e.target.value })}
                        required
                      />
                    </div>
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Payment Method</label>
                      <input
                        type="text"
                        className="form-control"
                        value={formData.paymentMethod}
                        onChange={(e) => setFormData({ ...formData, paymentMethod: e.target.value })}
                        placeholder="e.g., Cash, Bank Transfer, Credit Card"
                      />
                    </div>
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Reference Number</label>
                      <input
                        type="text"
                        className="form-control"
                        value={formData.referenceNumber}
                        onChange={(e) => setFormData({ ...formData, referenceNumber: e.target.value })}
                        placeholder="Bank reference, cheque number, etc."
                      />
                    </div>
                    <div className="col-12 mb-3">
                      <label className="form-label">Description</label>
                      <textarea
                        className="form-control"
                        rows="2"
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                        placeholder="Transaction description"
                      />
                    </div>
                    <div className="col-12 mb-3">
                      <label className="form-label">Notes</label>
                      <textarea
                        className="form-control"
                        rows="2"
                        value={formData.notes}
                        onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                        placeholder="Additional notes"
                      />
                    </div>
                  </div>
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-secondary" onClick={() => setShowAddModal(false)}>
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary">
                    Add Transaction
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Edit Transaction Modal */}
      {showEditModal && (
        <div className="modal show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Edit Transaction</h5>
                <button type="button" className="btn-close" onClick={() => setShowEditModal(false)}></button>
              </div>
              <form onSubmit={handleSubmit}>
                <div className="modal-body">
                  <div className="row">
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Transaction Type <span className="text-danger">*</span></label>
                      <select
                        className="form-select"
                        value={formData.transactionType}
                        onChange={(e) => setFormData({ ...formData, transactionType: e.target.value })}
                        required
                      >
                        <option value="Payment">Payment</option>
                        <option value="Refund">Refund</option>
                        <option value="Credit">Credit</option>
                        <option value="Debit">Debit</option>
                        <option value="Adjustment">Adjustment</option>
                      </select>
                    </div>
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Transaction Date <span className="text-danger">*</span></label>
                      <input
                        type="date"
                        className="form-control"
                        value={formData.transactionDate}
                        onChange={(e) => setFormData({ ...formData, transactionDate: e.target.value })}
                        required
                      />
                    </div>
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Amount <span className="text-danger">*</span></label>
                      <input
                        type="number"
                        className="form-control"
                        step="0.01"
                        min="0.01"
                        value={formData.amount}
                        onChange={(e) => setFormData({ ...formData, amount: e.target.value })}
                        required
                      />
                    </div>
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Payment Method</label>
                      <input
                        type="text"
                        className="form-control"
                        value={formData.paymentMethod}
                        onChange={(e) => setFormData({ ...formData, paymentMethod: e.target.value })}
                        placeholder="e.g., Cash, Bank Transfer, Credit Card"
                      />
                    </div>
                    <div className="col-md-6 mb-3">
                      <label className="form-label">Reference Number</label>
                      <input
                        type="text"
                        className="form-control"
                        value={formData.referenceNumber}
                        onChange={(e) => setFormData({ ...formData, referenceNumber: e.target.value })}
                        placeholder="Bank reference, cheque number, etc."
                      />
                    </div>
                    <div className="col-12 mb-3">
                      <label className="form-label">Description</label>
                      <textarea
                        className="form-control"
                        rows="2"
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                        placeholder="Transaction description"
                      />
                    </div>
                    <div className="col-12 mb-3">
                      <label className="form-label">Notes</label>
                      <textarea
                        className="form-control"
                        rows="2"
                        value={formData.notes}
                        onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                        placeholder="Additional notes"
                      />
                    </div>
                  </div>
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-secondary" onClick={() => setShowEditModal(false)}>
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary">
                    Update Transaction
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export default ClientTransactions;

