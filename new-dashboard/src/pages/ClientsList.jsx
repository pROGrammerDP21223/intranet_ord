import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/Layout';
import DataTable from '../components/DataTable';
import { clientAPI } from '../services/api';
import { useRole } from '../hooks/useRole';
import { showToast } from '../utils/toast';
import { useLoading } from '../contexts/LoadingContext';

const ClientsList = () => {
  const [clients, setClients] = useState([]);
  const [error, setError] = useState('');
  const role = useRole();
  const { startLoading, stopLoading } = useLoading();

  useEffect(() => {
    loadClients();
  }, []);

  const loadClients = async () => {
    try {
      setError('');
      startLoading('Loading clients...');
      const response = await clientAPI.getClients();
      if (response.success && response.data) {
        setClients(response.data);
      } else {
        const errorMsg = response.message || 'Failed to load clients';
        setError(errorMsg);
        showToast.error(errorMsg);
      }
    } catch (err) {
      const errorMsg = 'An error occurred while loading clients';
      setError(errorMsg);
      showToast.error(errorMsg);
      console.error('Failed to load clients:', err);
    } finally {
      stopLoading();
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this client? This action cannot be undone.')) {
      return;
    }

    try {
      startLoading('Deleting client...');
      const response = await clientAPI.deleteClient(id);
      if (response.success) {
        await loadClients();
        showToast.success('Client deleted successfully');
      } else {
        const errorMsg = response.message || 'Failed to delete client';
        showToast.error(errorMsg);
      }
    } catch (err) {
      showToast.error('An error occurred while deleting client');
      console.error('Failed to delete client:', err);
    } finally {
      stopLoading();
    }
  };

  const formatDate = (dateString) => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB');
  };

  const formatCurrency = (amount) => {
    if (!amount) return '-';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 0
    }).format(amount);
  };

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0 font-size-18">Clients List</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <Link to="/dashboard">Dashboard</Link>
                </li>
                <li className="breadcrumb-item active">Clients</li>
              </ol>
            </div>
          </div>
        </div>
      </div>

      {error && (
        <div className="alert alert-danger alert-dismissible fade show" role="alert">
          {error}
          <button type="button" className="btn-close" onClick={() => setError('')}></button>
        </div>
      )}

      <div className="row">
        <div className="col-12">
          <div className="card">
            <div className="card-header">
              <div className="row align-items-center">
                <div className="col">
                  <h4 className="card-title mb-0">Clients Details</h4>
                  <p className="text-muted mb-0">Manage your clients and their information</p>
                </div>
              </div>
            </div>
            <div className="card-body">
              <DataTable
                data={clients}
                columns={[
                  {
                    key: 'customerNo',
                    header: 'Customer No',
                    render: (value) => <strong className="text-primary">{value || '-'}</strong>
                  },
                  { key: 'companyName', header: 'Company Name', render: (value) => value || '-' },
                  { key: 'contactPerson', header: 'Contact Person', render: (value) => value || '-' },
                  { key: 'email', header: 'Email', render: (value) => value || '-' },
                  { key: 'phone', header: 'Phone', render: (value) => value || '-' },
                  {
                    key: 'formDate',
                    header: 'Form Date',
                    render: (value) => formatDate(value)
                  },
                  {
                    key: 'totalPackage',
                    header: 'Total Package',
                    render: (value) => <strong>{formatCurrency(value)}</strong>
                  },
                  {
                    key: 'actions',
                    header: 'Actions',
                    cellStyle: { textAlign: 'center', width: '180px' },
                    render: (value, row) => (
                      <div className="d-flex gap-1 justify-content-center">
                        <Link
                          to={`/clients/${row.id}`}
                          className="btn btn-sm btn-primary"
                          title="View Details"
                          onClick={(e) => e.stopPropagation()}
                        >
                          <i className="fas fa-eye"></i>
                        </Link>
                        {role.canEdit && (
                          <Link
                            to={`/clients/${row.id}/edit`}
                            className="btn btn-sm btn-info"
                            title="Edit"
                            onClick={(e) => e.stopPropagation()}
                          >
                            <i className="fas fa-edit"></i>
                          </Link>
                        )}
                        {role.canDelete && (
                          <button
                            className="btn btn-sm btn-danger"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleDelete(row.id);
                            }}
                            title="Delete"
                          >
                            <i className="fas fa-trash"></i>
                          </button>
                        )}
                      </div>
                    )
                  }
                ]}
                pageSize={10}
                showPagination={true}
                showSearch={true}
                searchPlaceholder="Search clients..."
                emptyMessage="No clients found"
              />
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default ClientsList;

