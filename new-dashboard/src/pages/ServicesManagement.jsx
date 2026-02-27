import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import { serviceAPI } from '../services/api';
import { showToast } from '../utils/toast';
import { useLoading } from '../contexts/LoadingContext';

const ServicesManagement = () => {
  const [services, setServices] = useState([]);
  const [loading, setLoading] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [editingService, setEditingService] = useState(null);
  const [formData, setFormData] = useState({
    serviceType: '',
    serviceName: '',
    category: '',
    isActive: true,
    sortOrder: '',
  });
  const [errors, setErrors] = useState({});
  const [success, setSuccess] = useState('');
  const { startLoading, stopLoading } = useLoading();

  useEffect(() => {
    loadServices();
  }, []);

  const loadServices = async () => {
    try {
      setLoading(true);
      startLoading('Loading services...');
      const response = await serviceAPI.getServices(true); // Include inactive services
      if (response.success && response.data) {
        setServices(response.data);
      } else {
        showToast.error('Failed to load services');
      }
    } catch (error) {
      showToast.error('Failed to load services');
      console.error('Failed to load services:', error);
    } finally {
      setLoading(false);
      stopLoading();
    }
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
    // Clear error for this field
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
    setSuccess('');
  };

  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.serviceType.trim()) {
      newErrors.serviceType = 'Service Type is required';
    }
    if (!formData.serviceName.trim()) {
      newErrors.serviceName = 'Service Name is required';
    }
    if (!formData.category.trim()) {
      newErrors.category = 'Category is required';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    try {
      setLoading(true);
      startLoading(editingService ? 'Updating service...' : 'Creating service...');
      const payload = {
        serviceType: formData.serviceType,
        serviceName: formData.serviceName,
        category: formData.category,
        isActive: formData.isActive,
        sortOrder: formData.sortOrder ? parseInt(formData.sortOrder) : null,
      };

      let response;
      if (editingService) {
        response = await serviceAPI.updateService(editingService.id, payload);
      } else {
        response = await serviceAPI.createService(payload);
      }

      if (response.success) {
        const successMsg = editingService ? 'Service updated successfully!' : 'Service created successfully!';
        setSuccess(successMsg);
        showToast.success(successMsg);
        setShowModal(false);
        resetForm();
        loadServices();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        const errorMsg = response.message || 'Failed to save service';
        setErrors({ submit: errorMsg });
        showToast.error(errorMsg);
      }
    } catch (error) {
      const errorMsg = error.response?.data?.message || 'An error occurred';
      setErrors({ submit: errorMsg });
      showToast.error(errorMsg);
    } finally {
      setLoading(false);
      stopLoading();
    }
  };

  const handleEdit = (service) => {
    setEditingService(service);
    setFormData({
      serviceType: service.serviceType,
      serviceName: service.serviceName,
      category: service.category || '',
      isActive: service.isActive,
      sortOrder: service.sortOrder?.toString() || '',
    });
    setShowModal(true);
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this service?')) {
      return;
    }

    try {
      setLoading(true);
      startLoading('Deleting service...');
      const response = await serviceAPI.deleteService(id);
      if (response.success) {
        const successMsg = 'Service deleted successfully!';
        setSuccess(successMsg);
        showToast.success(successMsg);
        loadServices();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        const errorMsg = response.message || 'Failed to delete service';
        showToast.error(errorMsg);
      }
    } catch (error) {
      const errorMsg = error.response?.data?.message || 'An error occurred';
      showToast.error(errorMsg);
    } finally {
      setLoading(false);
      stopLoading();
    }
  };

  const resetForm = () => {
    setFormData({
      serviceType: '',
      serviceName: '',
      category: '',
      isActive: true,
      sortOrder: '',
    });
    setEditingService(null);
    setErrors({});
  };

  const handleCloseModal = () => {
    setShowModal(false);
    resetForm();
  };

  // Group services by category
  const groupedServices = services.reduce((acc, service) => {
    const category = service.category || 'Other';
    if (!acc[category]) {
      acc[category] = [];
    }
    acc[category].push(service);
    return acc;
  }, {});

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0 font-size-18">Services Management</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <a href="#dashboard">Dashboard</a>
                </li>
                <li className="breadcrumb-item active">Services</li>
              </ol>
            </div>
          </div>
        </div>
      </div>

      {success && (
        <div className="alert alert-success alert-dismissible fade show" role="alert">
          {success}
          <button type="button" className="btn-close" onClick={() => setSuccess('')}></button>
        </div>
      )}

      <div className="row">
        <div className="col-12">
          <div className="card">
            <div className="card-header">
              <div className="row align-items-center">
                <div className="col">
                  <h4 className="card-title mb-0">Services List</h4>
                  <p className="text-muted mb-0">Manage your services and categories</p>
                </div>
                <div className="col-auto">
                  <button
                    className="btn btn-primary btn-sm"
                    onClick={() => {
                      resetForm();
                      setShowModal(true);
                    }}
                  >
                    <i className="fas fa-plus me-1"></i>Add New Service
                  </button>
                </div>
              </div>
            </div>
            <div className="card-body">
              {loading && services.length === 0 ? (
                <div className="text-center py-5">
                  <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading...</span>
                  </div>
                </div>
              ) : services.length === 0 ? (
                <div className="text-center py-5">
                  <i className="fas fa-inbox fa-3x text-muted mb-3 d-block"></i>
                  <p className="text-muted mb-3">No services found.</p>
                  <button
                    className="btn btn-primary btn-sm"
                    onClick={() => {
                      resetForm();
                      setShowModal(true);
                    }}
                  >
                    <i className="fas fa-plus me-1"></i>Add New Service
                  </button>
                </div>
              ) : (
                <div className="table-responsive">
                  <table className="table table-hover table-centered mb-0">
                    <thead className="thead-light">
                      <tr>
                        <th style={{ width: '60px' }}>ID</th>
                        <th>Service Type</th>
                        <th>Service Name</th>
                        <th>Category</th>
                        <th style={{ width: '100px' }}>Sort Order</th>
                        <th style={{ width: '100px' }}>Status</th>
                        <th style={{ width: '120px' }} className="text-end">Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {services.map((service) => (
                        <tr key={service.id}>
                          <td>{service.id}</td>
                          <td>
                            <span className="badge bg-soft-primary">{service.serviceType}</span>
                          </td>
                          <td>
                            <strong>{service.serviceName}</strong>
                          </td>
                          <td>{service.category || 'Other'}</td>
                          <td className="text-center">{service.sortOrder || '-'}</td>
                          <td>
                            <span className={`badge ${service.isActive ? 'bg-success' : 'bg-secondary'}`}>
                              {service.isActive ? 'Active' : 'Inactive'}
                            </span>
                          </td>
                          <td className="text-end">
                            <button
                              className="btn btn-sm btn-soft-primary me-1"
                              onClick={() => handleEdit(service)}
                              title="Edit"
                            >
                              <i className="fas fa-edit"></i>
                            </button>
                            <button
                              className="btn btn-sm btn-soft-danger"
                              onClick={() => handleDelete(service.id)}
                              title="Delete"
                            >
                              <i className="fas fa-trash"></i>
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Add/Edit Modal */}
      {showModal && (
        <div className="modal fade show" style={{ display: 'block', backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  {editingService ? 'Edit Service' : 'Add New Service'}
                </h5>
                <button type="button" className="btn-close" onClick={handleCloseModal}></button>
              </div>
              <form onSubmit={handleSubmit}>
                <fieldset disabled={loading}>
                <div className="modal-body">
                  {errors.submit && (
                    <div className="alert alert-danger">{errors.submit}</div>
                  )}

                  <div className="mb-3">
                    <label htmlFor="serviceType" className="form-label">
                      Service Type <span className="text-danger">*</span>
                    </label>
                    <input
                      type="text"
                      className={`form-control ${errors.serviceType ? 'is-invalid' : ''}`}
                      id="serviceType"
                      name="serviceType"
                      placeholder="e.g., domain-hosting"
                      value={formData.serviceType}
                      onChange={handleChange}
                      required
                    />
                    {errors.serviceType && (
                      <div className="invalid-feedback">{errors.serviceType}</div>
                    )}
                    <small className="form-text text-muted">
                      Unique identifier for the service (lowercase with hyphens)
                    </small>
                  </div>

                  <div className="mb-3">
                    <label htmlFor="serviceName" className="form-label">
                      Service Name <span className="text-danger">*</span>
                    </label>
                    <input
                      type="text"
                      className={`form-control ${errors.serviceName ? 'is-invalid' : ''}`}
                      id="serviceName"
                      name="serviceName"
                      placeholder="e.g., Domain & Hosting"
                      value={formData.serviceName}
                      onChange={handleChange}
                      required
                    />
                    {errors.serviceName && (
                      <div className="invalid-feedback">{errors.serviceName}</div>
                    )}
                  </div>

                  <div className="mb-3">
                    <label htmlFor="category" className="form-label">
                      Category <span className="text-danger">*</span>
                    </label>
                    <select
                      className={`form-control ${errors.category ? 'is-invalid' : ''}`}
                      id="category"
                      name="category"
                      value={formData.category}
                      onChange={handleChange}
                      required
                    >
                      <option value="">Select Category</option>
                      <option value="Domain & Hosting">Domain & Hosting</option>
                      <option value="Web Design">Web Design</option>
                      <option value="SEO">SEO</option>
                      <option value="Additional Services">Additional Services</option>
                      <option value="Other">Other</option>
                    </select>
                    {errors.category && (
                      <div className="invalid-feedback">{errors.category}</div>
                    )}
                  </div>

                  <div className="mb-3">
                    <label htmlFor="sortOrder" className="form-label">
                      Sort Order
                    </label>
                    <input
                      type="number"
                      className="form-control"
                      id="sortOrder"
                      name="sortOrder"
                      placeholder="e.g., 1, 2, 3..."
                      value={formData.sortOrder}
                      onChange={handleChange}
                    />
                    <small className="form-text text-muted">
                      Lower numbers appear first (optional)
                    </small>
                  </div>

                  <div className="mb-3">
                    <div className="form-check">
                      <input
                        className="form-check-input"
                        type="checkbox"
                        id="isActive"
                        name="isActive"
                        checked={formData.isActive}
                        onChange={handleChange}
                      />
                      <label className="form-check-label" htmlFor="isActive">
                        Active
                      </label>
                    </div>
                  </div>
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-secondary btn-sm" onClick={handleCloseModal}>
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary btn-sm" disabled={loading}>
                    {loading ? (
                      <>
                        <span className="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                        Saving...
                      </>
                    ) : (
                      editingService ? 'Update' : 'Create'
                    )}
                  </button>
                </div>
                </fieldset>
              </form>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
};

export default ServicesManagement;

