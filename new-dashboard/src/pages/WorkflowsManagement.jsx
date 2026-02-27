import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import Loader from '../components/Loader';
import { workflowsAPI } from '../services/api';
import { useRole } from '../hooks/useRole';
import { toast } from 'react-toastify';

const WorkflowsManagement = () => {
  const role = useRole();
  const [workflows, setWorkflows] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [selectedWorkflow, setSelectedWorkflow] = useState(null);
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    entityType: '',
    triggerEvent: '',
    conditions: '',
    actions: '',
    isActive: true,
  });

  useEffect(() => {
    if (role.isAdmin || role.isOwner) {
      loadWorkflows();
    }
  }, [role.isAdmin, role.isOwner]);

  const loadWorkflows = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await workflowsAPI.getAllWorkflows();
      if (response.success && response.data) {
        setWorkflows(response.data);
      } else {
        setError(response.message || 'Failed to load workflows');
      }
    } catch (err) {
      setError('An error occurred while loading workflows');
      console.error('Failed to load workflows:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setFormData({
      name: '',
      description: '',
      entityType: '',
      triggerEvent: '',
      conditions: '',
      actions: '',
      isActive: true,
    });
    setIsEditing(false);
    setSelectedWorkflow(null);
    setShowModal(true);
  };

  const handleEdit = (workflow) => {
    setFormData({
      name: workflow.name,
      description: workflow.description || '',
      entityType: workflow.entityType,
      triggerEvent: workflow.triggerEvent,
      conditions: workflow.conditions || '',
      actions: workflow.actions || '',
      isActive: workflow.isActive,
    });
    setIsEditing(true);
    setSelectedWorkflow(workflow);
    setShowModal(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      let response;
      if (isEditing) {
        response = await workflowsAPI.updateWorkflow(selectedWorkflow.id, formData);
      } else {
        response = await workflowsAPI.createWorkflow(formData);
      }

      if (response.success) {
        toast.success(response.message || 'Workflow saved successfully');
        setShowModal(false);
        loadWorkflows();
      } else {
        toast.error(response.message || 'Failed to save workflow');
      }
    } catch (err) {
      toast.error('An error occurred while saving workflow');
      console.error('Failed to save workflow:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this workflow?')) {
      return;
    }

    try {
      setLoading(true);
      const response = await workflowsAPI.deleteWorkflow(id);
      if (response.success) {
        toast.success('Workflow deleted successfully');
        loadWorkflows();
      } else {
        toast.error(response.message || 'Failed to delete workflow');
      }
    } catch (err) {
      toast.error('An error occurred while deleting workflow');
      console.error('Failed to delete workflow:', err);
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
                    <h4 className="card-title mb-0">Workflows Management</h4>
                  </div>
                  <div className="col-auto">
                    <button type="button" className="btn btn-primary" onClick={handleCreate}>
                      <i className="fas fa-plus-circle me-1"></i> Create Workflow
                    </button>
                  </div>
                </div>
              </div>
              <div className="card-body">
                {error && <div className="alert alert-danger">{error}</div>}
                {loading && !workflows.length ? (
                  <Loader />
                ) : (
                  <div className="table-responsive">
                    <table className="table table-striped">
                      <thead>
                        <tr>
                          <th>Name</th>
                          <th>Entity Type</th>
                          <th>Trigger Event</th>
                          <th>Status</th>
                          <th>Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {workflows.length === 0 ? (
                          <tr>
                            <td colSpan="5" className="text-center">
                              No workflows found
                            </td>
                          </tr>
                        ) : (
                          workflows.map((workflow) => (
                            <tr key={workflow.id}>
                              <td>{workflow.name}</td>
                              <td>
                                <span className="badge bg-primary">{workflow.entityType}</span>
                              </td>
                              <td>
                                <span className="badge bg-info">{workflow.triggerEvent}</span>
                              </td>
                              <td>
                                <span
                                  className={`badge ${workflow.isActive ? 'bg-success' : 'bg-secondary'}`}
                                >
                                  {workflow.isActive ? 'Active' : 'Inactive'}
                                </span>
                              </td>
                              <td>
                                <button
                                  className="btn btn-sm btn-primary me-2"
                                  onClick={() => handleEdit(workflow)}
                                >
                                  <i className="fas fa-edit"></i>
                                </button>
                                <button
                                  className="btn btn-sm btn-danger"
                                  onClick={() => handleDelete(workflow.id)}
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
                    {isEditing ? 'Edit Workflow' : 'Create Workflow'}
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
                      <label className="form-label">Description</label>
                      <textarea
                        className="form-control"
                        rows="3"
                        value={formData.description}
                        onChange={(e) =>
                          setFormData({ ...formData, description: e.target.value })
                        }
                      />
                    </div>
                    <div className="row">
                      <div className="col-md-6">
                        <div className="mb-3">
                          <label className="form-label">Entity Type *</label>
                          <select
                            className="form-select"
                            value={formData.entityType}
                            onChange={(e) =>
                              setFormData({ ...formData, entityType: e.target.value })
                            }
                            required
                          >
                            <option value="">Select Entity Type</option>
                            <option value="Client">Client</option>
                            <option value="Ticket">Ticket</option>
                            <option value="Enquiry">Enquiry</option>
                            <option value="Transaction">Transaction</option>
                          </select>
                        </div>
                      </div>
                      <div className="col-md-6">
                        <div className="mb-3">
                          <label className="form-label">Trigger Event *</label>
                          <select
                            className="form-select"
                            value={formData.triggerEvent}
                            onChange={(e) =>
                              setFormData({ ...formData, triggerEvent: e.target.value })
                            }
                            required
                          >
                            <option value="">Select Event</option>
                            <option value="Created">Created</option>
                            <option value="Updated">Updated</option>
                            <option value="Deleted">Deleted</option>
                            <option value="StatusChanged">Status Changed</option>
                          </select>
                        </div>
                      </div>
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Conditions (JSON)</label>
                      <textarea
                        className="form-control"
                        rows="4"
                        value={formData.conditions}
                        onChange={(e) =>
                          setFormData({ ...formData, conditions: e.target.value })
                        }
                        placeholder='{"status": "Pending", "priority": "High"}'
                      />
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Actions (JSON)</label>
                      <textarea
                        className="form-control"
                        rows="4"
                        value={formData.actions}
                        onChange={(e) =>
                          setFormData({ ...formData, actions: e.target.value })
                        }
                        placeholder='{"createTask": true, "sendEmail": true}'
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

export default WorkflowsManagement;

