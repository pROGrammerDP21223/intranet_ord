import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/Layout';
import DataTable from '../components/DataTable';
import RichTextEditor from '../components/RichTextEditor';
import { productAPI, categoryAPI, imageUploadAPI } from '../services/api';

const ProductsManagement = () => {
  const stripHtml = (html) => (html || '').replace(/<[^>]*>/g, ' ').replace(/\s+/g, ' ').trim();
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingProduct, setEditingProduct] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    mainImage: '',
    categoryId: '',
    additionalImages: [],
  });
  const [mainImageFile, setMainImageFile] = useState(null);
  const [mainImagePreview, setMainImagePreview] = useState('');
  const [additionalImageFiles, setAdditionalImageFiles] = useState([]);
  const [additionalImagePreviews, setAdditionalImagePreviews] = useState([]);

  useEffect(() => {
    loadProducts();
    loadCategories();
  }, []);

  const loadProducts = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await productAPI.getProducts();
      if (response.success && response.data) {
        setProducts(response.data);
      } else {
        setError(response.message || 'Failed to load products');
      }
    } catch (err) {
      setError('An error occurred while loading products');
      console.error('Failed to load products:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadCategories = async () => {
    try {
      const response = await categoryAPI.getCategories();
      if (response.success && response.data) {
        setCategories(response.data);
      }
    } catch (err) {
      console.error('Failed to load categories:', err);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      setError('');
      
      // Upload main image if file is selected
      let mainImageUrl = formData.mainImage;
      if (mainImageFile) {
        const uploadResponse = await imageUploadAPI.uploadImage(mainImageFile, 'products');
        if (uploadResponse.success && uploadResponse.data) {
          mainImageUrl = uploadResponse.data.url;
        } else {
          setError('Failed to upload main image');
          setLoading(false);
          return;
        }
      }

      // Upload additional images if files are selected
      let additionalImageUrls = [...formData.additionalImages];
      if (additionalImageFiles.length > 0) {
        const uploadResponse = await imageUploadAPI.uploadMultipleImages(additionalImageFiles, 'products');
        if (uploadResponse.success && uploadResponse.data) {
          additionalImageUrls = [...additionalImageUrls, ...uploadResponse.data.urls];
        } else {
          setError('Failed to upload some additional images');
          setLoading(false);
          return;
        }
      }

      const submitData = {
        ...formData,
        mainImage: mainImageUrl,
        additionalImages: additionalImageUrls,
      };

      const response = editingProduct
        ? await productAPI.updateProduct(editingProduct.id, submitData)
        : await productAPI.createProduct(submitData);

      if (response.success) {
        await loadProducts();
        setShowModal(false);
        resetForm();
        alert(editingProduct ? 'Product updated successfully' : 'Product created successfully');
      } else {
        setError(response.message || 'Failed to save product');
      }
    } catch (err) {
      setError('An error occurred while saving product');
      console.error('Failed to save product:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (product) => {
    setEditingProduct(product);
    setFormData({
      name: product.name || '',
      description: product.description || '',
      mainImage: product.mainImage || '',
      categoryId: product.categoryId || '',
      additionalImages: product.additionalImages || [],
    });
    setMainImageFile(null);
    setMainImagePreview(product.mainImage || '');
    setAdditionalImageFiles([]);
    setAdditionalImagePreviews(product.additionalImages || []);
    setShowModal(true);
  };

  const handleMainImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setMainImageFile(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setMainImagePreview(reader.result);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleAdditionalImagesChange = (e) => {
    const files = Array.from(e.target.files);
    if (files.length > 0) {
      const newFiles = [...additionalImageFiles, ...files];
      setAdditionalImageFiles(newFiles);
      
      // Create previews for new files
      files.forEach((file) => {
        const reader = new FileReader();
        reader.onloadend = () => {
          setAdditionalImagePreviews(prev => [...prev, reader.result]);
        };
        reader.readAsDataURL(file);
      });
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this product?')) {
      return;
    }

    try {
      setLoading(true);
      const response = await productAPI.deleteProduct(id);
      if (response.success) {
        await loadProducts();
        alert('Product deleted successfully');
      } else {
        alert(response.message || 'Failed to delete product');
      }
    } catch (err) {
      alert('An error occurred while deleting product');
      console.error('Failed to delete product:', err);
    } finally {
      setLoading(false);
    }
  };

  const removeImage = (index, isFile = false) => {
    if (isFile) {
      // Remove from file array (adjust index for files only)
      const fileIndex = index - formData.additionalImages.length;
      const newFiles = additionalImageFiles.filter((_, i) => i !== fileIndex);
      setAdditionalImageFiles(newFiles);
      // Remove preview (adjust index)
      const previewIndex = index;
      setAdditionalImagePreviews(prev => prev.filter((_, i) => i !== previewIndex));
    } else {
      // Remove existing URL
      setFormData({
        ...formData,
        additionalImages: formData.additionalImages.filter((_, i) => i !== index),
      });
      // Also remove corresponding preview
      setAdditionalImagePreviews(prev => prev.filter((_, i) => i !== index));
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      description: '',
      mainImage: '',
      categoryId: '',
      additionalImages: [],
    });
    setMainImageFile(null);
    setMainImagePreview('');
    setAdditionalImageFiles([]);
    setAdditionalImagePreviews([]);
    setEditingProduct(null);
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
            <h4 className="mb-sm-0 font-size-18">Products Management</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <Link to="/dashboard">Dashboard</Link>
                </li>
                <li className="breadcrumb-item active">Products</li>
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
          {loading && products.length === 0 ? (
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
              data={products}
              columns={[
                {
                  key: 'mainImage',
                  header: 'Image',
                  render: (value, row) => (
                    <img
                      src={value || '/placeholder.png'}
                      alt={row.name}
                      style={{ width: '50px', height: '50px', objectFit: 'cover', borderRadius: '4px' }}
                      onError={(e) => {
                        e.target.src = 'https://via.placeholder.com/50x50?text=No+Image';
                      }}
                    />
                  )
                },
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
                      {stripHtml(value) || 'No description'}
                    </div>
                  )
                },
                {
                  key: 'categoryName',
                  header: 'Category',
                  render: (value, row) => (
                    <div>
                      <span className="badge bg-primary">{value || 'Uncategorized'}</span>
                      {row.industryName && (
                        <span className="badge bg-info ms-1">{row.industryName}</span>
                      )}
                    </div>
                  )
                },
                {
                  key: 'additionalImages',
                  header: 'Additional Images',
                  render: (value) => (
                    value && value.length > 0 ? (
                      <span className="badge bg-secondary">
                        <i className="fas fa-image me-1"></i>
                        {value.length}
                      </span>
                    ) : (
                      '-'
                    )
                  )
                },
                {
                  key: 'actions',
                  header: 'Actions',
                  headerStyle: { textAlign: 'center', width: '150px' },
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
              searchPlaceholder="Search products..."
              emptyMessage="No products found"
              actions={
                <button className="btn btn-primary btn-sm" onClick={openModal}>
                  <i className="fas fa-plus me-1"></i>Add New Product
                </button>
              }
            />
          )}
        </div>
      </div>

      {/* Modal */}
      {showModal && (
        <div className="modal fade show" style={{ display: 'block', backgroundColor: 'rgba(0,0,0,0.5)' }} tabIndex="-1">
          <div className="modal-dialog modal-lg modal-dialog-scrollable">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">{editingProduct ? 'Edit Product' : 'Add New Product'}</h5>
                <button type="button" className="btn-close" onClick={closeModal}></button>
              </div>
              <form onSubmit={handleSubmit}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">Category <span className="text-danger">*</span></label>
                    <select
                      className="form-select"
                      value={formData.categoryId}
                      onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
                      required
                    >
                      <option value="">Select Category</option>
                      {categories.map((category) => (
                        <option key={category.id} value={category.id}>
                          {category.name} {category.industryName ? `(${category.industryName})` : ''}
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
                    <RichTextEditor
                      value={formData.description}
                      onChange={(content) => setFormData({ ...formData, description: content })}
                      placeholder="Enter product description..."
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Main Image <span className="text-danger">*</span></label>
                    <input
                      type="file"
                      className="form-control"
                      accept="image/*"
                      onChange={handleMainImageChange}
                      required={!editingProduct || !formData.mainImage}
                    />
                    {(mainImagePreview || formData.mainImage) && (
                      <div className="mt-2">
                        <img 
                          src={mainImagePreview || formData.mainImage} 
                          alt="Preview" 
                          style={{ width: '150px', height: '150px', objectFit: 'cover', borderRadius: '4px' }} 
                          onError={(e) => e.target.style.display = 'none'} 
                        />
                        {formData.mainImage && !mainImageFile && (
                          <div className="mt-2">
                            <small className="text-muted">Current image will be kept if no new file is selected</small>
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Additional Images</label>
                    <input
                      type="file"
                      className="form-control"
                      accept="image/*"
                      multiple
                      onChange={handleAdditionalImagesChange}
                    />
                    <small className="text-muted">You can select multiple images at once</small>
                    {(additionalImagePreviews.length > 0 || formData.additionalImages.length > 0) && (
                      <div className="d-flex flex-wrap gap-2 mt-2">
                        {/* Show existing images from formData */}
                        {formData.additionalImages.map((img, index) => (
                          <div key={`existing-${index}`} className="position-relative" style={{ width: '100px', height: '100px' }}>
                            <img
                              src={img}
                              alt={`Additional ${index + 1}`}
                              className="img-thumbnail"
                              style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                              onError={(e) => e.target.style.display = 'none'}
                            />
                            <button
                              type="button"
                              className="btn btn-sm btn-danger position-absolute top-0 end-0"
                              style={{ transform: 'translate(50%, -50%)' }}
                              onClick={() => removeImage(index, false)}
                            >
                              <i className="fas fa-times"></i>
                            </button>
                          </div>
                        ))}
                        {/* Show new file previews (only those beyond existing images) */}
                        {additionalImagePreviews.length > formData.additionalImages.length && 
                         additionalImagePreviews.slice(formData.additionalImages.length).map((preview, index) => {
                           const actualIndex = formData.additionalImages.length + index;
                           return (
                             <div key={`new-${index}`} className="position-relative" style={{ width: '100px', height: '100px' }}>
                               <img
                                 src={preview}
                                 alt={`New ${index + 1}`}
                                 className="img-thumbnail"
                                 style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                                 onError={(e) => e.target.style.display = 'none'}
                               />
                               <button
                                 type="button"
                                 className="btn btn-sm btn-danger position-absolute top-0 end-0"
                                 style={{ transform: 'translate(50%, -50%)' }}
                                 onClick={() => removeImage(actualIndex, true)}
                               >
                                 <i className="fas fa-times"></i>
                               </button>
                             </div>
                           );
                         })}
                      </div>
                    )}
                  </div>
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-secondary" onClick={closeModal}>Cancel</button>
                  <button type="submit" className="btn btn-primary" disabled={loading}>
                    {loading ? 'Saving...' : editingProduct ? 'Update' : 'Create'}
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

export default ProductsManagement;

