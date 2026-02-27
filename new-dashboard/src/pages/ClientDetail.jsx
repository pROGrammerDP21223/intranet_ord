import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import Layout from '../components/Layout';
import ClientTransactions from '../components/ClientTransactions';
import ClientProducts from '../components/ClientProducts';
import { clientAPI } from '../services/api';
import { useRole } from '../hooks/useRole';
import { showToast } from '../utils/toast';
import { useLoading } from '../contexts/LoadingContext';

const ClientDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const role = useRole();
  const { startLoading, stopLoading } = useLoading();
  const [client, setClient] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadClient();
  }, [id]);

  const loadClient = async () => {
    try {
      setLoading(true);
      setError('');
      startLoading('Loading client details...');
      const response = await clientAPI.getClientById(id);
      if (response.success && response.data) {
        setClient(response.data);
      } else {
        const errorMsg = response.message || 'Client not found';
        setError(errorMsg);
        showToast.error(errorMsg);
      }
    } catch (err) {
      const errorMsg = 'An error occurred while loading client';
      setError(errorMsg);
      showToast.error(errorMsg);
      console.error('Failed to load client:', err);
    } finally {
      setLoading(false);
      stopLoading();
    }
  };

  const handleDelete = async () => {
    if (!window.confirm('Are you sure you want to delete this client? This action cannot be undone.')) {
      return;
    }

    try {
      startLoading('Deleting client...');
      const response = await clientAPI.deleteClient(id);
      if (response.success) {
        showToast.success('Client deleted successfully');
        navigate('/clients');
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
      maximumFractionDigits: 2
    }).format(amount);
  };

  if (loading) {
    return (
      <Layout>
        <div className="text-center py-5">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      </Layout>
    );
  }

  if (error || !client) {
    return (
      <Layout>
        <div className="alert alert-danger">{error || 'Client not found'}</div>
        <Link to="/clients" className="btn btn-primary">Back to Clients</Link>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0 font-size-18">Client Details</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <Link to="/dashboard">Dashboard</Link>
                </li>
                <li className="breadcrumb-item">
                  <Link to="/clients">Clients</Link>
                </li>
                <li className="breadcrumb-item active">Details</li>
              </ol>
            </div>
          </div>
        </div>
      </div>

      <div className="row mb-3">
        <div className="col-12">
          <div className="d-flex gap-2 align-items-center">
            <Link to="/clients" className="btn btn-secondary btn-sm">
              <i className="fas fa-arrow-left me-1"></i>Back
            </Link>
            <Link to={`/clients/${id}/edit`} className="btn btn-primary btn-sm">
              <i className="fas fa-edit me-1"></i>Edit
            </Link>
            <button onClick={handleDelete} className="btn btn-danger btn-sm">
              <i className="fas fa-trash me-1"></i>Delete
            </button>
          </div>
        </div>
      </div>

      {/* Client Information */}
      <div className="row">
        <div className="col-md-6">
          <div className="card mb-4">
            <div className="card-header">
              <h5 className="mb-0">Client Information</h5>
            </div>
            <div className="card-body">
              <table className="table table-borderless">
                <tbody>
                  <tr>
                    <th width="40%">Customer No:</th>
                    <td><strong>{client.customerNo}</strong></td>
                  </tr>
                  <tr>
                    <th>Form Date:</th>
                    <td>{formatDate(client.formDate)}</td>
                  </tr>
                  <tr>
                    <th>Company Name:</th>
                    <td>{client.companyName}</td>
                  </tr>
                  <tr>
                    <th>Contact Person:</th>
                    <td>{client.contactPerson}</td>
                  </tr>
                  <tr>
                    <th>Designation:</th>
                    <td>{client.designation || '-'}</td>
                  </tr>
                  <tr>
                    <th>Address:</th>
                    <td>{client.address || '-'}</td>
                  </tr>
                  <tr>
                    <th>Phone:</th>
                    <td>{client.phone || '-'}</td>
                  </tr>
                  <tr>
                    <th>Email:</th>
                    <td>{client.email || '-'}</td>
                  </tr>
                  {(client.whatsAppNumber || client.enquiryEmail) && (
                    <>
                      {client.whatsAppNumber && (
                        <tr>
                          <th>WhatsApp Number:</th>
                          <td>
                            {client.whatsAppNumber}
                            {client.useWhatsAppService && (
                              <span className="badge bg-success ms-2">
                                <i className="fab fa-whatsapp me-1"></i>Enabled
                              </span>
                            )}
                          </td>
                        </tr>
                      )}
                      {client.enquiryEmail && (
                        <tr>
                          <th>Enquiry Email:</th>
                          <td>{client.enquiryEmail}</td>
                        </tr>
                      )}
                    </>
                  )}
                  <tr>
                    <th>Domain Name:</th>
                    <td>{client.domainName || '-'}</td>
                  </tr>
                  <tr>
                    <th>GST No:</th>
                    <td>{client.gstNo || '-'}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div className="col-md-6">
          <div className="card mb-4">
            <div className="card-header">
              <h5 className="mb-0">Package Details</h5>
            </div>
            <div className="card-body">
              <table className="table table-borderless">
                <tbody>
                  <tr>
                    <th width="40%">Amount Without GST:</th>
                    <td>{formatCurrency(client.amountWithoutGst)}</td>
                  </tr>
                  <tr>
                    <th>GST Percentage:</th>
                    <td>{client.gstPercentage ? `${client.gstPercentage}%` : '-'}</td>
                  </tr>
                  <tr>
                    <th>GST Amount:</th>
                    <td>{formatCurrency(client.gstAmount)}</td>
                  </tr>
                  <tr>
                    <th>Total Package:</th>
                    <td><strong>{formatCurrency(client.totalPackage)}</strong></td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      {/* Services */}
      {client.clientServices && client.clientServices.length > 0 && (
        <div className="row">
          <div className="col-12">
            <div className="card mb-4">
              <div className="card-header">
                <h5 className="mb-0">Services</h5>
              </div>
              <div className="card-body">
                <ul className="list-unstyled mb-0">
                  {client.clientServices.map((cs, index) => (
                    <li key={index} className="mb-2">
                      <i className="fas fa-check-circle text-success me-2"></i>
                      {cs.service?.serviceName || `Service ID: ${cs.serviceId}`}
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Notification Settings */}
      {(client.useWhatsAppService !== undefined || client.whatsAppNumber || client.enquiryEmail) && (
        <div className="row">
          <div className="col-12">
            <div className="card mb-4">
              <div className="card-header bg-info text-white">
                <h5 className="mb-0">
                  <i className="fas fa-bell me-2"></i>
                  Notification Settings
                </h5>
              </div>
              <div className="card-body">
                <div className="row">
                  <div className="col-md-6">
                    <h6 className="border-bottom pb-2 mb-3">
                      <i className="fas fa-envelope me-2 text-primary"></i>
                      Email Notifications
                    </h6>
                    <table className="table table-sm table-borderless">
                      <tbody>
                        <tr>
                          <th width="50%">Use Same Email:</th>
                          <td>
                            {client.useSameEmailForEnquiries !== undefined ? (
                              client.useSameEmailForEnquiries ? (
                                <span className="badge bg-success">Yes</span>
                              ) : (
                                <span className="badge bg-warning">No</span>
                              )
                            ) : (
                              <span className="badge bg-secondary">Not Set</span>
                            )}
                          </td>
                        </tr>
                        {!client.useSameEmailForEnquiries && client.enquiryEmail && (
                          <tr>
                            <th>Enquiry Email:</th>
                            <td>{client.enquiryEmail}</td>
                          </tr>
                        )}
                        <tr>
                          <th>Status:</th>
                          <td>
                            <span className="badge bg-success">
                              <i className="fas fa-check-circle me-1"></i>Enabled (Default)
                            </span>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                  <div className="col-md-6">
                    <h6 className="border-bottom pb-2 mb-3">
                      <i className="fab fa-whatsapp me-2 text-success"></i>
                      WhatsApp Notifications
                    </h6>
                    <table className="table table-sm table-borderless">
                      <tbody>
                        <tr>
                          <th width="50%">Service Enabled:</th>
                          <td>
                            {client.useWhatsAppService ? (
                              <span className="badge bg-success">
                                <i className="fab fa-whatsapp me-1"></i>Enabled
                              </span>
                            ) : (
                              <span className="badge bg-secondary">Disabled</span>
                            )}
                          </td>
                        </tr>
                        {client.useWhatsAppService && (
                          <>
                            <tr>
                              <th>Same as Mobile:</th>
                              <td>
                                {client.whatsAppSameAsMobile !== undefined ? (
                                  client.whatsAppSameAsMobile ? (
                                    <span className="badge bg-info">Yes</span>
                                  ) : (
                                    <span className="badge bg-warning">No</span>
                                  )
                                ) : (
                                  <span className="badge bg-secondary">Not Set</span>
                                )}
                              </td>
                            </tr>
                            {!client.whatsAppSameAsMobile && client.whatsAppNumber && (
                              <tr>
                                <th>WhatsApp Number:</th>
                                <td>{client.whatsAppNumber}</td>
                              </tr>
                            )}
                            {client.whatsAppSameAsMobile && (
                              <tr>
                                <th>WhatsApp Number:</th>
                                <td>{client.phone || 'N/A'}</td>
                              </tr>
                            )}
                          </>
                        )}
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Email Services */}
      {client.clientEmailServices && client.clientEmailServices.length > 0 && (
        <div className="row">
          <div className="col-12">
            <div className="card mb-4">
              <div className="card-header">
                <h5 className="mb-0">Email Services</h5>
              </div>
              <div className="card-body">
                <table className="table table-sm">
                  <thead>
                    <tr>
                      <th>Service Type</th>
                      <th>Quantity</th>
                    </tr>
                  </thead>
                  <tbody>
                    {client.clientEmailServices.map((es, index) => (
                      <tr key={index}>
                        <td>{es.emailServiceType}</td>
                        <td>{es.quantity}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* SEO Details */}
      {client.clientSeoDetail && (
        <div className="row">
          <div className="col-12">
            <div className="card mb-4">
              <div className="card-header">
                <h5 className="mb-0">SEO Details</h5>
              </div>
              <div className="card-body">
                <table className="table table-borderless">
                  <tbody>
                    <tr>
                      <th width="30%">Keyword Range:</th>
                      <td>{client.clientSeoDetail.keywordRange || '-'}</td>
                    </tr>
                    <tr>
                      <th>Location:</th>
                      <td>{client.clientSeoDetail.location || '-'}</td>
                    </tr>
                    <tr>
                      <th>Keywords List:</th>
                      <td>
                        <pre className="mb-0" style={{ whiteSpace: 'pre-wrap' }}>
                          {client.clientSeoDetail.keywordsList || '-'}
                        </pre>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* AdWords Details */}
      {client.clientAdwordsDetail && (
        <div className="row">
          <div className="col-12">
            <div className="card mb-4">
              <div className="card-header">
                <h5 className="mb-0">AdWords Details</h5>
              </div>
              <div className="card-body">
                <table className="table table-borderless">
                  <tbody>
                    <tr>
                      <th width="30%">Number of Keywords:</th>
                      <td>{client.clientAdwordsDetail.numberOfKeywords || '-'}</td>
                    </tr>
                    <tr>
                      <th>Period:</th>
                      <td>{client.clientAdwordsDetail.period || '-'}</td>
                    </tr>
                    <tr>
                      <th>Location:</th>
                      <td>{client.clientAdwordsDetail.location || '-'}</td>
                    </tr>
                    <tr>
                      <th>Keywords List:</th>
                      <td>
                        <pre className="mb-0" style={{ whiteSpace: 'pre-wrap' }}>
                          {client.clientAdwordsDetail.keywordsList || '-'}
                        </pre>
                      </td>
                    </tr>
                    <tr>
                      <th>Special Guidelines:</th>
                      <td>
                        <pre className="mb-0" style={{ whiteSpace: 'pre-wrap' }}>
                          {client.clientAdwordsDetail.specialGuidelines || '-'}
                        </pre>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Guidelines */}
      {client.specificGuidelines && (
        <div className="row">
          <div className="col-12">
            <div className="card mb-4">
              <div className="card-header">
                <h5 className="mb-0">Specific Guidelines</h5>
              </div>
              <div className="card-body">
                <pre style={{ whiteSpace: 'pre-wrap' }}>{client.specificGuidelines}</pre>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Client Products Section */}
      <div className="row">
        <div className="col-12">
          <ClientProducts clientId={id} />
        </div>
      </div>

      {/* Transactions Section - Hidden for Employees */}
      {role.canViewTransactions && (
        <div className="row">
          <div className="col-12">
            <ClientTransactions clientId={id} totalPackage={client.totalPackage || 0} />
          </div>
        </div>
      )}

    </Layout>
  );
};

export default ClientDetail;

