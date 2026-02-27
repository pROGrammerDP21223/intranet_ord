import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/Layout';
import { clientAPI, productAPI, clientProductAPI } from '../services/api';

const AttachProductsToClient = () => {
  const [clients, setClients] = useState([]);
  const [selectedClient, setSelectedClient] = useState(null);
  const [allProducts, setAllProducts] = useState([]);
  const [attachedProducts, setAttachedProducts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [categories, setCategories] = useState([]);
  const [selectedProductIds, setSelectedProductIds] = useState(new Set());

  useEffect(() => {
    loadClients();
    loadAllProducts();
  }, []);

  useEffect(() => {
    if (selectedClient) {
      loadAttachedProducts();
    } else {
      setAttachedProducts([]);
    }
  }, [selectedClient]);

  const loadClients = async () => {
    try {
      setLoading(true);
      const response = await clientAPI.getClients();
      if (response.success && response.data) {
        setClients(response.data);
      }
    } catch (err) {
      console.error('Failed to load clients:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadAllProducts = async () => {
    try {
      const response = await productAPI.getProducts();
      if (response.success && response.data) {
        setAllProducts(response.data);
        // Extract unique categories
        const uniqueCategories = [...new Set(response.data
          .map(p => p.categoryName)
          .filter(Boolean))];
        setCategories(uniqueCategories.sort());
      }
    } catch (err) {
      console.error('Failed to load products:', err);
    }
  };

  const loadAttachedProducts = async () => {
    if (!selectedClient) return;
    
    try {
      setLoading(true);
      setError('');
      const response = await clientProductAPI.getClientProducts(selectedClient.id);
      if (response.success && response.data) {
        setAttachedProducts(response.data);
      } else {
        setError(response.message || 'Failed to load attached products');
      }
    } catch (err) {
      setError('An error occurred while loading attached products');
      console.error('Failed to load attached products:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAttach = async (productId) => {
    if (!selectedClient) {
      alert('Please select a client first');
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');
      const response = await clientProductAPI.attachProductToClient(selectedClient.id, productId);
      if (response.success) {
        setSuccess('Product attached successfully');
        await loadAttachedProducts();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to attach product');
        setTimeout(() => setError(''), 5000);
      }
    } catch (err) {
      const errorMsg = err.response?.data?.message || 'An error occurred while attaching product';
      setError(errorMsg);
      setTimeout(() => setError(''), 5000);
      console.error('Failed to attach product:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleBulkAttach = async () => {
    if (!selectedClient) {
      alert('Please select a client first');
      return;
    }

    if (selectedProductIds.size === 0) {
      alert('Please select at least one product to attach');
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');
      const productIdsArray = Array.from(selectedProductIds);
      const response = await clientProductAPI.attachMultipleProductsToClient(selectedClient.id, productIdsArray);
      if (response.success) {
        setSuccess(`${productIdsArray.length} product(s) attached successfully`);
        setSelectedProductIds(new Set());
        await loadAttachedProducts();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to attach products');
        setTimeout(() => setError(''), 5000);
      }
    } catch (err) {
      const errorMsg = err.response?.data?.message || 'An error occurred while attaching products';
      setError(errorMsg);
      setTimeout(() => setError(''), 5000);
      console.error('Failed to attach products:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSelectProduct = (productId) => {
    const newSelected = new Set(selectedProductIds);
    if (newSelected.has(productId)) {
      newSelected.delete(productId);
    } else {
      newSelected.add(productId);
    }
    setSelectedProductIds(newSelected);
  };

  const handleSelectAll = () => {
    if (selectedProductIds.size === availableProducts.length) {
      setSelectedProductIds(new Set());
    } else {
      setSelectedProductIds(new Set(availableProducts.map(p => p.id)));
    }
  };

  const handleDetach = async (productId) => {
    if (!selectedClient) return;

    if (!window.confirm('Are you sure you want to detach this product from the client?')) {
      return;
    }

    try {
      setLoading(true);
      setError('');
      setSuccess('');
      const response = await clientProductAPI.detachProductFromClient(selectedClient.id, productId);
      if (response.success) {
        setSuccess('Product detached successfully');
        await loadAttachedProducts();
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to detach product');
        setTimeout(() => setError(''), 5000);
      }
    } catch (err) {
      setError('An error occurred while detaching product');
      setTimeout(() => setError(''), 5000);
      console.error('Failed to detach product:', err);
    } finally {
      setLoading(false);
    }
  };

  // Get available products (not attached)
  const availableProducts = allProducts.filter(product => {
    const isAttached = attachedProducts.some(ap => ap.id === product.id);
    const matchesSearch = !searchTerm || 
      product.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      product.description?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      product.categoryName?.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesCategory = !categoryFilter || product.categoryName === categoryFilter;
    return !isAttached && matchesSearch && matchesCategory;
  });

  // Filter attached products
  const filteredAttachedProducts = attachedProducts.filter(product => {
    const matchesSearch = !searchTerm || 
      product.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      product.description?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      product.categoryName?.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesCategory = !categoryFilter || product.categoryName === categoryFilter;
    return matchesSearch && matchesCategory;
  });

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0 font-size-18">Attach Products to Client</h4>
            <div className="page-title-right">
              <ol className="breadcrumb m-0">
                <li className="breadcrumb-item">
                  <Link to="/dashboard">Dashboard</Link>
                </li>
                <li className="breadcrumb-item">
                  <Link to="/clients">Clients</Link>
                </li>
                <li className="breadcrumb-item active">Attach Products</li>
              </ol>
            </div>
          </div>
        </div>
      </div>

      {/* Alerts */}
      {error && (
        <div className="alert alert-danger alert-dismissible fade show" role="alert">
          <i className="fas fa-exclamation-circle me-2"></i>
          {error}
          <button type="button" className="btn-close" onClick={() => setError('')}></button>
        </div>
      )}

      {success && (
        <div className="alert alert-success alert-dismissible fade show" role="alert">
          <i className="fas fa-check-circle me-2"></i>
          {success}
          <button type="button" className="btn-close" onClick={() => setSuccess('')}></button>
        </div>
      )}

      {/* Client Selection */}
      <div className="row mb-4">
        <div className="col-12">
          <div className="card">
            <div className="card-header">
              <h5 className="mb-0">
                <i className="fas fa-user-tie me-2"></i>Select Client
              </h5>
            </div>
            <div className="card-body">
              <div className="row">
                <div className="col-md-6">
                  <label className="form-label">Client</label>
                  <select
                    className="form-select"
                    value={selectedClient?.id || ''}
                    onChange={(e) => {
                      const clientId = parseInt(e.target.value);
                      const client = clients.find(c => c.id === clientId);
                      setSelectedClient(client || null);
                      setSearchTerm('');
                      setCategoryFilter('');
                      setSelectedProductIds(new Set());
                    }}
                  >
                    <option value="">-- Select a Client --</option>
                    {clients.map((client) => (
                      <option key={client.id} value={client.id}>
                        {client.customerNo} - {client.companyName}
                      </option>
                    ))}
                  </select>
                </div>
                {selectedClient && (
                  <div className="col-md-6">
                    <label className="form-label">Client Information</label>
                    <div className="p-3 bg-light rounded">
                      <p className="mb-1">
                        <strong>Company:</strong> {selectedClient.companyName}
                      </p>
                      <p className="mb-1">
                        <strong>Contact:</strong> {selectedClient.contactPerson || '-'}
                      </p>
                      <p className="mb-0">
                        <strong>Email:</strong> {selectedClient.email || '-'}
                      </p>
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>

      {selectedClient && (
        <>
          {/* Search and Filter */}
          <div className="row mb-3">
            <div className="col-md-4">
              <div className="input-group">
                <span className="input-group-text">
                  <i className="fas fa-search"></i>
                </span>
                <input
                  type="text"
                  className="form-control"
                  placeholder="Search products..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </div>
            </div>
            <div className="col-md-3">
              <select
                className="form-select"
                value={categoryFilter}
                onChange={(e) => setCategoryFilter(e.target.value)}
              >
                <option value="">All Categories</option>
                {categories.map((category) => (
                  <option key={category} value={category}>
                    {category}
                  </option>
                ))}
              </select>
            </div>
            <div className="col-md-5 text-end">
              <div className="d-flex gap-2 justify-content-end align-items-center">
                {selectedProductIds.size > 0 && (
                  <button
                    className="btn btn-success btn-sm"
                    onClick={handleBulkAttach}
                    disabled={loading}
                  >
                    <i className="fas fa-link me-1"></i>
                    Attach Selected ({selectedProductIds.size})
                  </button>
                )}
                <span className="badge bg-primary">
                  Available: {availableProducts.length}
                </span>
                <span className="badge bg-success">
                  Attached: {filteredAttachedProducts.length}
                </span>
              </div>
            </div>
          </div>

          {/* Products Display */}
          <div className="row">
            {/* Available Products */}
            <div className="col-lg-6">
              <div className="card h-100">
                <div className="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                  <h5 className="mb-0">
                    <i className="fas fa-box-open me-2"></i>
                    Available Products ({availableProducts.length})
                  </h5>
                  {availableProducts.length > 0 && (
                    <div className="form-check form-check-inline">
                      <input
                        className="form-check-input"
                        type="checkbox"
                        checked={selectedProductIds.size === availableProducts.length && availableProducts.length > 0}
                        onChange={handleSelectAll}
                        id="selectAll"
                      />
                      <label className="form-check-label text-white" htmlFor="selectAll" style={{ fontSize: '0.9rem' }}>
                        Select All
                      </label>
                    </div>
                  )}
                </div>
                <div className="card-body" style={{ maxHeight: '600px', overflowY: 'auto' }}>
                  {loading && availableProducts.length === 0 ? (
                    <div className="text-center py-5">
                      <div className="spinner-border text-primary" role="status">
                        <span className="visually-hidden">Loading...</span>
                      </div>
                    </div>
                  ) : availableProducts.length === 0 ? (
                    <div className="text-center py-5">
                      <i className="fas fa-inbox fa-3x text-muted mb-3 d-block"></i>
                      <p className="text-muted">
                        {searchTerm || categoryFilter
                          ? 'No products found matching your filters.'
                          : 'All products are already attached to this client.'}
                      </p>
                    </div>
                  ) : (
                    <div className="row g-3">
                      {availableProducts.map((product) => (
                        <div key={product.id} className="col-12">
                          <div className={`card border ${selectedProductIds.has(product.id) ? 'border-primary border-2' : ''}`}>
                            <div className="row g-0">
                              <div className="col-1 d-flex align-items-center justify-content-center">
                                <input
                                  type="checkbox"
                                  className="form-check-input"
                                  checked={selectedProductIds.has(product.id)}
                                  onChange={() => handleSelectProduct(product.id)}
                                  style={{ cursor: 'pointer' }}
                                />
                              </div>
                              <div className="col-3">
                                <div style={{ height: '120px', overflow: 'hidden' }}>
                                  <img
                                    src={product.mainImage || '/placeholder.png'}
                                    alt={product.name}
                                    className="img-fluid"
                                    style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                                    onError={(e) => {
                                      e.target.src = 'https://via.placeholder.com/300x200?text=No+Image';
                                    }}
                                  />
                                </div>
                              </div>
                              <div className="col-8">
                                <div className="card-body p-2">
                                  <h6 className="card-title mb-1" style={{ fontSize: '0.9rem' }}>
                                    {product.name}
                                  </h6>
                                  <p className="card-text text-muted small mb-2" style={{
                                    display: '-webkit-box',
                                    WebkitLineClamp: 2,
                                    WebkitBoxOrient: 'vertical',
                                    overflow: 'hidden',
                                    fontSize: '0.75rem'
                                  }}>
                                    {product.description || 'No description'}
                                  </p>
                                  <div className="mb-2">
                                    <span className="badge bg-primary" style={{ fontSize: '0.7rem' }}>
                                      {product.categoryName || 'Uncategorized'}
                                    </span>
                                    {product.industryName && (
                                      <span className="badge bg-info ms-1" style={{ fontSize: '0.7rem' }}>
                                        {product.industryName}
                                      </span>
                                    )}
                                  </div>
                                  <button
                                    className="btn btn-sm btn-primary w-100"
                                    onClick={() => handleAttach(product.id)}
                                    disabled={loading}
                                  >
                                    <i className="fas fa-link me-1"></i>Attach
                                  </button>
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Attached Products */}
            <div className="col-lg-6">
              <div className="card h-100">
                <div className="card-header bg-success text-white">
                  <h5 className="mb-0">
                    <i className="fas fa-check-circle me-2"></i>
                    Attached Products ({filteredAttachedProducts.length})
                  </h5>
                </div>
                <div className="card-body" style={{ maxHeight: '600px', overflowY: 'auto' }}>
                  {loading && filteredAttachedProducts.length === 0 ? (
                    <div className="text-center py-5">
                      <div className="spinner-border text-success" role="status">
                        <span className="visually-hidden">Loading...</span>
                      </div>
                    </div>
                  ) : filteredAttachedProducts.length === 0 ? (
                    <div className="text-center py-5">
                      <i className="fas fa-box-open fa-3x text-muted mb-3 d-block"></i>
                      <p className="text-muted">
                        {searchTerm || categoryFilter
                          ? 'No attached products found matching your filters.'
                          : 'No products attached to this client yet.'}
                      </p>
                    </div>
                  ) : (
                    <div className="row g-3">
                      {filteredAttachedProducts.map((product) => (
                        <div key={product.id} className="col-12">
                          <div className="card border border-success">
                            <div className="row g-0">
                              <div className="col-4">
                                <div style={{ height: '120px', overflow: 'hidden' }}>
                                  <img
                                    src={product.mainImage || '/placeholder.png'}
                                    alt={product.name}
                                    className="img-fluid"
                                    style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                                    onError={(e) => {
                                      e.target.src = 'https://via.placeholder.com/300x200?text=No+Image';
                                    }}
                                  />
                                </div>
                              </div>
                              <div className="col-8">
                                <div className="card-body p-2">
                                  <h6 className="card-title mb-1" style={{ fontSize: '0.9rem' }}>
                                    {product.name}
                                  </h6>
                                  <p className="card-text text-muted small mb-2" style={{
                                    display: '-webkit-box',
                                    WebkitLineClamp: 2,
                                    WebkitBoxOrient: 'vertical',
                                    overflow: 'hidden',
                                    fontSize: '0.75rem'
                                  }}>
                                    {product.description || 'No description'}
                                  </p>
                                  <div className="mb-2">
                                    <span className="badge bg-primary" style={{ fontSize: '0.7rem' }}>
                                      {product.categoryName || 'Uncategorized'}
                                    </span>
                                    {product.industryName && (
                                      <span className="badge bg-info ms-1" style={{ fontSize: '0.7rem' }}>
                                        {product.industryName}
                                      </span>
                                    )}
                                  </div>
                                  <button
                                    className="btn btn-sm btn-danger w-100"
                                    onClick={() => handleDetach(product.id)}
                                    disabled={loading}
                                  >
                                    <i className="fas fa-unlink me-1"></i>Detach
                                  </button>
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        </>
      )}

      {!selectedClient && (
        <div className="row">
          <div className="col-12">
            <div className="card">
              <div className="card-body text-center py-5">
                <i className="fas fa-hand-pointer fa-3x text-muted mb-3 d-block"></i>
                <h5 className="text-muted">Please select a client to manage products</h5>
                <p className="text-muted">Choose a client from the dropdown above to view and attach products.</p>
              </div>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
};

export default AttachProductsToClient;

