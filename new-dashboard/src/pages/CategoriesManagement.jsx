import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/Layout';
import DataTable from '../components/DataTable';
import { categoryAPI, industryAPI, imageUploadAPI } from '../services/api';

const CategoriesManagement = () => {
  const [categories, setCategories] = useState([]);
  const [industries, setIndustries] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingCategory, setEditingCategory] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    image: '',
    industryId: '',
  });
  const [imageFile, setImageFile] = useState(null);
  const [imagePreview, setImagePreview] = useState('');

  useEffect(() => {
    loadCategories();
    loadIndustries();
  }, []);

  const loadCategories = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await categoryAPI.getCategories();
      if (response.success && response.data) {
        setCategories(response.data);
      } else {
        setError(response.message || 'Failed to load categories');
      }
    } catch (err) {
      setError('An error occurred while loading categories');
      console.error('Failed to load categories:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadIndustries = async () => {
    try {
      const response = await industryAPI.getIndustries();
      if (response.success && response.data) {
        setIndustries(response.data);
      }
    } catch (err) {
      console.error('Failed to load industries:', err);
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
        const uploadResponse = await imageUploadAPI.uploadImage(imageFile, 'categories');
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

      const response = editingCategory
        ? await categoryAPI.updateCategory(editingCategory.id, submitData)
        : await categoryAPI.createCategory(submitData);

      if (response.success) {
        await loadCategories();
        setShowModal(false);
        resetForm();
        alert(editingCategory ? 'Category updated successfully' : 'Category created successfully');
      } else {
        setError(response.message || 'Failed to save category');
      }
    } catch (err) {
      setError('An error occurred while saving category');
      console.error('Failed to save category:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (category) => {
    setEditingCategory(category);
    setFormData({
      name: category.name || '',
      description: category.description || '',
      image: category.image || '',
      industryId: category.industryId || '',
    });
    setImageFile(null);
    setImagePreview(category.image || '');
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
    if (!window.confirm('Are you sure you want to delete this category? This will also delete all associated products.')) {
      return;
    }

    try {
      setLoading(true);
      const response = await categoryAPI.deleteCategory(id);
      if (response.success) {
        await loadCategories();
        alert('Category deleted successfully');
      } else {
        alert(response.message || 'Failed to delete category');
      }
    } catch (err) {
      alert('An error occurred while deleting category');
      console.error('Failed to delete category:', err);
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      description: '',
      image: '',
      industryId: '',
    });
    setImageFile(null);
    setImagePreview('');
    setEditingCategory(null);
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
            <h4 className="mb-sm-0 font-size-18">Categories Management</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <Link to="/dashboard">Dashboard</Link>
                </li>
                <li className="breadcrumb-item active">Categories</li>
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
          {loading && categories.length === 0 ? (
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
                  data={categories}
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
                      key: 'industryName',
                      header: 'Industry',
                      render: (value) => value || '-'
                    },
                    {
                      key: 'productsCount',
                      header: 'Products',
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
                  searchPlaceholder="Search categories..."
                  emptyMessage="No categories found"
                  actions={
                    <button className="btn btn-primary btn-sm" onClick={openModal}>
                      <i className="fas fa-plus me-1"></i>Add New Category
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
                <h5 className="modal-title">{editingCategory ? 'Edit Category' : 'Add New Category'}</h5>
                <button type="button" className="btn-close" onClick={closeModal}></button>
              </div>
              <form onSubmit={handleSubmit}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">Industry <span className="text-danger">*</span></label>
                    <select
                      className="form-select"
                      value={formData.industryId}
                      onChange={(e) => setFormData({ ...formData, industryId: e.target.value })}
                      required
                    >
                      <option value="">Select Industry</option>
                      {industries.map((industry) => (
                        <option key={industry.id} value={industry.id}>
                          {industry.name}
                        </option>
                      ))}
                    </select>
                  </div>
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
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-secondary" onClick={closeModal}>Cancel</button>
                  <button type="submit" className="btn btn-primary" disabled={loading}>
                    {loading ? 'Saving...' : editingCategory ? 'Update' : 'Create'}
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

export default CategoriesManagement;

