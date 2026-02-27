import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import Loader from '../components/Loader';
import { emailTemplatesAPI } from '../services/api';
import { useRole } from '../hooks/useRole';
import { toast } from 'react-toastify';

const EmailTemplatesManagement = () => {
  const role = useRole();
  const [templates, setTemplates] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [selectedTemplate, setSelectedTemplate] = useState(null);
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    templateType: '',
    subject: '',
    body: '',
    variables: '',
    isActive: true,
  });

  useEffect(() => {
    loadTemplates();
  }, []);

  const loadTemplates = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await emailTemplatesAPI.getAllTemplates();
      if (response.success && response.data) {
        setTemplates(response.data);
      } else {
        setError(response.message || 'Failed to load templates');
      }
    } catch (err) {
      setError('An error occurred while loading templates');
      console.error('Failed to load templates:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setFormData({
      name: '',
      templateType: '',
      subject: '',
      body: '',
      variables: '',
      isActive: true,
    });
    setIsEditing(false);
    setSelectedTemplate(null);
    setShowModal(true);
  };

  const handleEdit = (template) => {
    setFormData({
      name: template.name,
      templateType: template.templateType,
      subject: template.subject,
      body: template.body,
      variables: template.variables || '',
      isActive: template.isActive,
    });
    setIsEditing(true);
    setSelectedTemplate(template);
    setShowModal(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      let response;
      if (isEditing) {
        response = await emailTemplatesAPI.updateTemplate(selectedTemplate.id, formData);
      } else {
        response = await emailTemplatesAPI.createTemplate(formData);
      }

      if (response.success) {
        toast.success(response.message || 'Template saved successfully');
        setShowModal(false);
        loadTemplates();
      } else {
        toast.error(response.message || 'Failed to save template');
      }
    } catch (err) {
      toast.error('An error occurred while saving template');
      console.error('Failed to save template:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this template?')) {
      return;
    }

    try {
      setLoading(true);
      const response = await emailTemplatesAPI.deleteTemplate(id);
      if (response.success) {
        toast.success('Template deleted successfully');
        loadTemplates();
      } else {
        toast.error(response.message || 'Failed to delete template');
      }
    } catch (err) {
      toast.error('An error occurred while deleting template');
      console.error('Failed to delete template:', err);
    } finally {
      setLoading(false);
    }
  };

  if (!role.isAdmin && !role.isOwner) {
    return (
      <Layout>
        <div className="container-fluid">
          <div className="alert alert-danger">You don't have permission to access this page.</div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="container-fluid">
        <div className="row">
          <div className="col-12">
            <div className="card">
              <div className="card-header">
                <div className="row align-items-center">
                  <div className="col">
                    <h4 className="card-title mb-0">Email Templates Management</h4>
                  </div>
                  <div className="col-auto">
                    <button type="button" className="btn btn-primary" onClick={handleCreate}>
                      <i className="fas fa-plus-circle me-1"></i> Create Template
                    </button>
                  </div>
                </div>
              </div>
              <div className="card-body">
                {error && <div className="alert alert-danger">{error}</div>}
                {loading && !templates.length ? (
                  <Loader />
                ) : (
                  <div className="table-responsive">
                    <table className="table table-striped">
                      <thead>
                        <tr>
                          <th>Name</th>
                          <th>Type</th>
                          <th>Subject</th>
                          <th>Variables</th>
                          <th>Status</th>
                          <th>Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {templates.length === 0 ? (
                          <tr>
                            <td colSpan="6" className="text-center">
                              No templates found
                            </td>
                          </tr>
                        ) : (
                          templates.map((template) => (
                            <tr key={template.id}>
                              <td>{template.name}</td>
                              <td>
                                <span className="badge bg-info">{template.templateType}</span>
                              </td>
                              <td>{template.subject}</td>
                              <td>
                                <small className="text-muted">
                                  {template.variables || 'None'}
                                </small>
                              </td>
                              <td>
                                <span
                                  className={`badge ${template.isActive ? 'bg-success' : 'bg-secondary'}`}
                                >
                                  {template.isActive ? 'Active' : 'Inactive'}
                                </span>
                              </td>
                              <td>
                                <button
                                  className="btn btn-sm btn-primary me-2"
                                  onClick={() => handleEdit(template)}
                                >
                                  <i className="fas fa-edit"></i>
                                </button>
                                <button
                                  className="btn btn-sm btn-danger"
                                  onClick={() => handleDelete(template.id)}
                                >
                                  <i className="fas fa-trash"></i>
                                </button>
                              </td>
                            </tr>
                          ))
                        )}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Create/Edit Modal */}
        {showModal && (
          <div
            className="modal show d-block"
            tabIndex="-1"
            style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}
          >
            <div className="modal-dialog modal-lg">
              <div className="modal-content">
                <div className="modal-header">
                  <h5 className="modal-title">
                    {isEditing ? 'Edit Template' : 'Create Template'}
                  </h5>
                  <button
                    type="button"
                    className="btn-close"
                    onClick={() => setShowModal(false)}
                  ></button>
                </div>
                <form onSubmit={handleSubmit}>
                  <div className="modal-body">
                    <div className="mb-3">
                      <label className="form-label">Name *</label>
                      <input
                        type="text"
                        className="form-control"
                        value={formData.name}
                        onChange={(e) =>
                          setFormData({ ...formData, name: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Template Type *</label>
                      <input
                        type="text"
                        className="form-control"
                        value={formData.templateType}
                        onChange={(e) =>
                          setFormData({ ...formData, templateType: e.target.value })
                        }
                        placeholder="e.g., WelcomeEmail, PasswordReset"
                        required
                      />
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Subject *</label>
                      <input
                        type="text"
                        className="form-control"
                        value={formData.subject}
                        onChange={(e) =>
                          setFormData({ ...formData, subject: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Body *</label>
                      <textarea
                        className="form-control"
                        rows="10"
                        value={formData.body}
                        onChange={(e) =>
                          setFormData({ ...formData, body: e.target.value })
                        }
                        required
                      />
                      <small className="text-muted">
                        Use variables like {'{'}Name{'}'}, {'{'}Email{'}'} in the body
                      </small>
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Variables (comma-separated)</label>
                      <input
                        type="text"
                        className="form-control"
                        value={formData.variables}
                        onChange={(e) =>
                          setFormData({ ...formData, variables: e.target.value })
                        }
                        placeholder="e.g., Name, Email, CompanyName"
                      />
                    </div>
                    <div className="mb-3">
                      <div className="form-check">
                        <input
                          className="form-check-input"
                          type="checkbox"
                          checked={formData.isActive}
                          onChange={(e) =>
                            setFormData({ ...formData, isActive: e.target.checked })
                          }
                        />
                        <label className="form-check-label">Active</label>
                      </div>
                    </div>
                  </div>
                  <div className="modal-footer">
                    <button
                      type="button"
                      className="btn btn-secondary"
                      onClick={() => setShowModal(false)}
                    >
                      Cancel
                    </button>
                    <button type="submit" className="btn btn-primary" disabled={loading}>
                      {loading ? 'Saving...' : isEditing ? 'Update' : 'Create'}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
};

export default EmailTemplatesManagement;

