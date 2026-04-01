import { useState, useEffect } from 'react';
import { clientProductAPI } from '../services/api';

const ClientProducts = ({ clientId }) => {
  const [clientProducts, setClientProducts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    loadClientProducts();
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

  return (
    <>
      <div className="row">
        <div className="col-12">
          <div className="card mb-4">
            <div className="card-header">
              <h5 className="mb-0">Attached Products</h5>
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
  );
};

export default ClientProducts;

