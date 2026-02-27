import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/Layout';
import DataTable from '../components/DataTable';
import { industryAPI, imageUploadAPI } from '../services/api';

const IndustriesManagement = () => {
  const [industries, setIndustries] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingIndustry, setEditingIndustry] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    image: '',
    topIndustry: false,
    bannerIndustry: false,
  });
  const [imageFile, setImageFile] = useState(null);
  const [imagePreview, setImagePreview] = useState('');

  useEffect(() => {
    loadIndustries();
  }, []);

  const loadIndustries = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await industryAPI.getIndustries();
      if (response.success && response.data) {
        setIndustries(response.data);
      } else {
        setError(response.message || 'Failed to load industries');
      }
    } catch (err) {
      setError('An error occurred while loading industries');
      console.error('Failed to load industries:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      setError('');
      
      // Upload image if file is selected
      let imageUrl = formData.image;
      if (imageFile) {
        const uploadResponse = await imageUploadAPI.uploadImage(imageFile, 'industries');
        if (uploadResponse.success && uploadResponse.data) {
          imageUrl = uploadResponse.data.url;
        } else {
          setError('Failed to upload image');
          setLoading(false);
          return;
        }
      }

      const submitData = {
        ...formData,
        image: imageUrl,
      };

      const response = editingIndustry
        ? await industryAPI.updateIndustry(editingIndustry.id, submitData)
        : await industryAPI.createIndustry(submitData);

      if (response.success) {
        await loadIndustries();
        setShowModal(false);
        resetForm();
        alert(editingIndustry ? 'Industry updated successfully' : 'Industry created successfully');
      } else {
        setError(response.message || 'Failed to save industry');
      }
    } catch (err) {
      setError('An error occurred while saving industry');
      console.error('Failed to save industry:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (industry) => {
    setEditingIndustry(industry);
    setFormData({
      name: industry.name || '',
      description: industry.description || '',
      image: industry.image || '',
      topIndustry: industry.topIndustry || false,
      bannerIndustry: industry.bannerIndustry || false,
    });
    setImageFile(null);
    setImagePreview(industry.image || '');
    setShowModal(true);
  };

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setImageFile(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreview(reader.result);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this industry? This will also delete all associated categories and products.')) {
      return;
    }

    try {
      setLoading(true);
      const response = await industryAPI.deleteIndustry(id);
      if (response.success) {
        await loadIndustries();
        alert('Industry deleted successfully');
      } else {
        alert(response.message || 'Failed to delete industry');
      }
    } catch (err) {
      alert('An error occurred while deleting industry');
      console.error('Failed to delete industry:', err);
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      description: '',
      image: '',
      topIndustry: false,
      bannerIndustry: false,
    });
    setImageFile(null);
    setImagePreview('');
    setEditingIndustry(null);
  };

  const openModal = () => {
    resetForm();
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    resetForm();
  };

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0 font-size-18">Industries Management</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <Link to="/dashboard">Dashboard</Link>
                </li>
                <li className="breadcrumb-item active">Industries</li>
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
          {loading && industries.length === 0 ? (
            <div className="card">
              <div className="card-body">
                <div className="text-center py-5">
                  <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading...</span>
                  </div>
                </div>
              </div>
            </div>
          ) : (
            <DataTable
                  data={industries}
                  columns={[
                    {
                      key: 'name',
                      header: 'Name',
                      render: (value) => <strong>{value}</strong>
                    },
                    {
                      key: 'description',
                      header: 'Description',
                      render: (value) => (
                        <div style={{ maxWidth: '300px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                          {value || '-'}
                        </div>
                      )
                    },
                    {
                      key: 'image',
                      header: 'Image',
                      render: (value, row) => (
                        value ? (
                          <img src={value} alt={row.name} style={{ width: '50px', height: '50px', objectFit: 'cover' }} />
                        ) : (
                          '-'
                        )
                      )
                    },
                    {
                      key: 'topIndustry',
                      header: 'Top Industry',
                      render: (value) => (
                        value ? (
                          <span className="badge bg-success">Yes</span>
                        ) : (
                          <span className="badge bg-secondary">No</span>
                        )
                      )
                    },
                    {
                      key: 'bannerIndustry',
                      header: 'Banner Industry',
                      render: (value) => (
                        value ? (
                          <span className="badge bg-success">Yes</span>
                        ) : (
                          <span className="badge bg-secondary">No</span>
                        )
                      )
                    },
                    {
                      key: 'categoriesCount',
                      header: 'Categories',
                      render: (value) => <span className="badge bg-info">{value || 0}</span>
                    },
                    {
                      key: 'actions',
                      header: 'Actions',
                      headerStyle: { textAlign: 'center', width: '120px' },
                      cellStyle: { textAlign: 'center' },
                      render: (value, row) => (
                        <div className="d-flex gap-1 justify-content-center">
                          <button
                            className="btn btn-sm btn-info"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleEdit(row);
                            }}
                            title="Edit"
                          >
                            <i className="fas fa-edit"></i>
                          </button>
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
                        </div>
                      )
                    }
                  ]}
                  pageSize={10}
                  showPagination={true}
                  showSearch={true}
                  searchPlaceholder="Search industries..."
                  emptyMessage="No industries found"
                  actions={
                    <button className="btn btn-primary btn-sm" onClick={openModal}>
                      <i className="fas fa-plus me-1"></i>Add New Industry
                    </button>
                  }
                />
          )}
        </div>
      </div>

      {/* Modal */}
      {showModal && (
        <div className="modal fade show" style={{ display: 'block', backgroundColor: 'rgba(0,0,0,0.5)' }} tabIndex="-1">
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">{editingIndustry ? 'Edit Industry' : 'Add New Industry'}</h5>
                <button type="button" className="btn-close" onClick={closeModal}></button>
              </div>
              <form onSubmit={handleSubmit}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">Name <span className="text-danger">*</span></label>
                    <input
                      type="text"
                      className="form-control"
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Description</label>
                    <textarea
                      className="form-control"
                      rows="3"
                      value={formData.description}
                      onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Image</label>
                    <input
                      type="file"
                      className="form-control"
                      accept="image/*"
                      onChange={handleImageChange}
                    />
                    {(imagePreview || formData.image) && (
                      <div className="mt-2">
                        <img 
                          src={imagePreview || formData.image} 
                          alt="Preview" 
                          style={{ width: '150px', height: '150px', objectFit: 'cover', borderRadius: '4px' }} 
                          onError={(e) => e.target.style.display = 'none'} 
                        />
                        {formData.image && !imageFile && (
                          <div className="mt-2">
                            <small className="text-muted">Current image will be kept if no new file is selected</small>
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                  <div className="mb-3">
                    <div className="form-check form-switch">
                      <input
                        className="form-check-input"
                        type="checkbox"
                        id="topIndustry"
                        checked={formData.topIndustry}
                        onChange={(e) => setFormData({ ...formData, topIndustry: e.target.checked })}
                      />
                      <label className="form-check-label" htmlFor="topIndustry">
                        Top Industry
                      </label>
                    </div>
                  </div>
                  <div className="mb-3">
                    <div className="form-check form-switch">
                      <input
                        className="form-check-input"
                        type="checkbox"
                        id="bannerIndustry"
                        checked={formData.bannerIndustry}
                        onChange={(e) => setFormData({ ...formData, bannerIndustry: e.target.checked })}
                      />
                      <label className="form-check-label" htmlFor="bannerIndustry">
                        Banner Industry
                      </label>
                    </div>
                  </div>
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-secondary" onClick={closeModal}>Cancel</button>
                  <button type="submit" className="btn btn-primary" disabled={loading}>
                    {loading ? 'Saving...' : editingIndustry ? 'Update' : 'Create'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
};

export default IndustriesManagement;

