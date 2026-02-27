import { useState, useEffect } from 'react';
import { clientProductAPI, productAPI } from '../services/api';

const ClientProducts = ({ clientId }) => {
  const [clientProducts, setClientProducts] = useState([]);
  const [allProducts, setAllProducts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showAttachModal, setShowAttachModal] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    loadClientProducts();
    loadAllProducts();
  }, [clientId]);

  const loadClientProducts = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await clientProductAPI.getClientProducts(clientId);
      if (response.success && response.data) {
        setClientProducts(response.data);
      } else {
        setError(response.message || 'Failed to load client products');
      }
    } catch (err) {
      setError('An error occurred while loading client products');
      console.error('Failed to load client products:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadAllProducts = async () => {
    try {
      const response = await productAPI.getProducts();
      if (response.success && response.data) {
        setAllProducts(response.data);
      }
    } catch (err) {
      console.error('Failed to load products:', err);
    }
  };

  const handleAttach = async (productId) => {
    try {
      setLoading(true);
      const response = await clientProductAPI.attachProductToClient(clientId, productId);
      if (response.success) {
        await loadClientProducts();
        setShowAttachModal(false);
        alert('Product attached successfully');
      } else {
        alert(response.message || 'Failed to attach product');
      }
    } catch (err) {
      alert(err.response?.data?.message || 'An error occurred while attaching product');
      console.error('Failed to attach product:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDetach = async (productId) => {
    if (!window.confirm('Are you sure you want to detach this product from the client?')) {
      return;
    }

    try {
      setLoading(true);
      const response = await clientProductAPI.detachProductFromClient(clientId, productId);
      if (response.success) {
        await loadClientProducts();
        alert('Product detached successfully');
      } else {
        alert(response.message || 'Failed to detach product');
      }
    } catch (err) {
      alert('An error occurred while detaching product');
      console.error('Failed to detach product:', err);
    } finally {
      setLoading(false);
    }
  };

  // Get products that are not already attached
  const availableProducts = allProducts.filter(product => {
    const isAttached = clientProducts.some(cp => cp.id === product.id);
    const matchesSearch = product.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         product.description?.toLowerCase().includes(searchTerm.toLowerCase());
    return !isAttached && matchesSearch;
  });

  return (
    <>
      <div className="row">
        <div className="col-12">
          <div className="card mb-4">
            <div className="card-header">
              <div className="row align-items-center">
                <div className="col">
                  <h5 className="mb-0">Client Products</h5>
                </div>
                <div className="col-auto">
                  <button
                    className="btn btn-primary btn-sm"
                    onClick={() => setShowAttachModal(true)}
                  >
                    <i className="fas fa-plus me-1"></i>Attach Product
                  </button>
                </div>
              </div>
            </div>
            <div className="card-body">
              {error && (
                <div className="alert alert-danger alert-dismissible fade show" role="alert">
                  {error}
                  <button type="button" className="btn-close" onClick={() => setError('')}></button>
                </div>
              )}

              {loading && clientProducts.length === 0 ? (
                <div className="text-center py-5">
                  <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading...</span>
                  </div>
                </div>
              ) : clientProducts.length === 0 ? (
                <div className="text-center py-5">
                  <i className="fas fa-box-open fa-3x text-muted mb-3 d-block"></i>
                  <p className="text-muted mb-3">No products attached to this client.</p>
                  <button
                    className="btn btn-primary btn-sm"
                    onClick={() => setShowAttachModal(true)}
                  >
                    <i className="fas fa-plus me-1"></i>Attach Product
                  </button>
                </div>
              ) : (
                <div className="row">
                  {clientProducts.map((product) => (
                    <div key={product.id} className="col-md-6 col-lg-4 mb-3">
                      <div className="card h-100">
                        <div style={{ height: '150px', overflow: 'hidden' }}>
                          <img
                            src={product.mainImage || '/placeholder.png'}
                            alt={product.name}
                            className="card-img-top"
                            style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                            onError={(e) => {
                              e.target.src = 'https://via.placeholder.com/300x200?text=No+Image';
                            }}
                          />
                        </div>
                        <div className="card-body">
                          <h6 className="card-title">{product.name}</h6>
                          <p className="card-text text-muted small" style={{
                            display: '-webkit-box',
                            WebkitLineClamp: 2,
                            WebkitBoxOrient: 'vertical',
                            overflow: 'hidden'
                          }}>
                            {product.description || 'No description'}
                          </p>
                          <div className="mb-2">
                            <span className="badge bg-primary">{product.categoryName || 'Uncategorized'}</span>
                            {product.industryName && (
                              <span className="badge bg-info ms-1">{product.industryName}</span>
                            )}
                          </div>
                        </div>
                        <div className="card-footer">
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
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Attach Product Modal */}
      {showAttachModal && (
        <div className="modal fade show" style={{ display: 'block', backgroundColor: 'rgba(0,0,0,0.5)' }} tabIndex="-1">
          <div className="modal-dialog modal-lg modal-dialog-scrollable">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Attach Product to Client</h5>
                <button type="button" className="btn-close" onClick={() => setShowAttachModal(false)}></button>
              </div>
              <div className="modal-body">
                <div className="mb-3">
                  <input
                    type="text"
                    className="form-control"
                    placeholder="Search products..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                  />
                </div>
                {availableProducts.length === 0 ? (
                  <div className="text-center py-5">
                    <p className="text-muted">
                      {searchTerm ? 'No products found matching your search.' : 'All available products are already attached.'}
                    </p>
                  </div>
                ) : (
                  <div className="row">
                    {availableProducts.map((product) => (
                      <div key={product.id} className="col-md-6 mb-3">
                        <div className="card">
                          <div style={{ height: '120px', overflow: 'hidden' }}>
                            <img
                              src={product.mainImage || '/placeholder.png'}
                              alt={product.name}
                              className="card-img-top"
                              style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                              onError={(e) => {
                                e.target.src = 'https://via.placeholder.com/300x200?text=No+Image';
                              }}
                            />
                          </div>
                          <div className="card-body">
                            <h6 className="card-title">{product.name}</h6>
                            <p className="card-text text-muted small" style={{
                              display: '-webkit-box',
                              WebkitLineClamp: 2,
                              WebkitBoxOrient: 'vertical',
                              overflow: 'hidden'
                            }}>
                              {product.description || 'No description'}
                            </p>
                            <div className="mb-2">
                              <span className="badge bg-primary">{product.categoryName || 'Uncategorized'}</span>
                              {product.industryName && (
                                <span className="badge bg-info ms-1">{product.industryName}</span>
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
                    ))}
                  </div>
                )}
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowAttachModal(false)}>Close</button>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export default ClientProducts;

