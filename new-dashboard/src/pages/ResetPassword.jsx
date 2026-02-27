import { useState, useEffect } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { authAPI } from '../services/api';
import { showToast } from '../utils/toast';
import { useLoading } from '../contexts/LoadingContext';

const ResetPassword = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    confirmPassword: '',
  });
  const [token, setToken] = useState('');
  const [errors, setErrors] = useState({});
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);
  const { startLoading, stopLoading } = useLoading();

  useEffect(() => {
    const tokenParam = searchParams.get('token');
    if (tokenParam) {
      setToken(tokenParam);
    } else {
      setError('Invalid reset link. Please request a new password reset.');
    }
  }, [searchParams]);

  const validateForm = () => {
    const newErrors = {};

    if (!formData.email.trim()) {
      newErrors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Invalid email format';
    }

    if (!formData.password) {
      newErrors.password = 'Password is required';
    } else if (formData.password.length < 6) {
      newErrors.password = 'Password must be at least 6 characters';
    } else if (formData.password.length > 100) {
      newErrors.password = 'Password must not exceed 100 characters';
    }

    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Confirm Password is required';
    } else if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }

    if (!token) {
      newErrors.token = 'Reset token is missing';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: value,
    });
    if (errors[name]) {
      setErrors({
        ...errors,
        [name]: '',
      });
    }
    setError('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess(false);

    if (!validateForm()) {
      return;
    }

    setLoading(true);
    startLoading('Resetting password...');

    try {
      await authAPI.resetPassword({
        token,
        email: formData.email,
        password: formData.password,
        confirmPassword: formData.confirmPassword,
      });
      setSuccess(true);
      showToast.success('Password has been reset successfully! Redirecting to login...');
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Failed to reset password';
      setError(message);
      showToast.error(message);
    } finally {
      setLoading(false);
      stopLoading();
    }
  };

  if (!token && !error) {
    return null; // Loading state
  }

  return (
    <div className="auth-page" style={{ backgroundImage: "url('/p-1.png')", backgroundSize: 'cover', backgroundPosition: 'center center' }}>
      <div className="container-md">
        <div className="row vh-100 d-flex justify-content-center">
          <div className="col-12 align-self-center">
            <div className="card-body">
              <div className="row">
                <div className="col-lg-4 mx-auto">
                  <div className="card">
                    <div className="card-body p-0 auth-header-box">
                      <div className="text-center p-3">
                        <a href="/" className="logo logo-admin">
                          <img src="/logo-sm.png" height="50" alt="logo" className="auth-logo" />
                        </a>
                        <h4 className="mt-3 mb-1 fw-semibold text-white font-18">
                          Reset Your Password
                        </h4>
                        <p className="text-muted mb-0">Enter your new password below.</p>
                      </div>
                    </div>
                    <div className="card-body pt-0">
                      {success && (
                        <div className="alert alert-success" role="alert">
                          Password has been reset successfully! Redirecting to login...
                        </div>
                      )}
                      {error && (
                        <div className="alert alert-danger" role="alert">
                          {error}
                        </div>
                      )}
                      {token ? (
                        <form className="my-4" onSubmit={handleSubmit}>
                          <fieldset disabled={loading || success}>
                          <div className="form-group mb-2">
                            <label className="form-label" htmlFor="email">
                              Email
                            </label>
                            <input
                              type="email"
                              className={`form-control ${errors.email ? 'is-invalid' : ''}`}
                              id="email"
                              name="email"
                              placeholder="Enter your email"
                              value={formData.email}
                              onChange={handleChange}
                              required
                            />
                            {errors.email && (
                              <div className="invalid-feedback d-block">{errors.email}</div>
                            )}
                          </div>

                          <div className="form-group mb-2">
                            <label className="form-label" htmlFor="password">
                              New Password
                            </label>
                            <input
                              type="password"
                              className={`form-control ${errors.password ? 'is-invalid' : ''}`}
                              name="password"
                              id="password"
                              placeholder="Enter new password"
                              value={formData.password}
                              onChange={handleChange}
                              required
                            />
                            {errors.password && (
                              <div className="invalid-feedback d-block">{errors.password}</div>
                            )}
                          </div>

                          <div className="form-group mb-2">
                            <label className="form-label" htmlFor="confirmPassword">
                              Confirm Password
                            </label>
                            <input
                              type="password"
                              className={`form-control ${errors.confirmPassword ? 'is-invalid' : ''}`}
                              name="confirmPassword"
                              id="confirmPassword"
                              placeholder="Confirm new password"
                              value={formData.confirmPassword}
                              onChange={handleChange}
                              required
                            />
                            {errors.confirmPassword && (
                              <div className="invalid-feedback d-block">{errors.confirmPassword}</div>
                            )}
                          </div>

                          <div className="form-group mb-0 row">
                            <div className="col-12">
                              <div className="d-grid mt-3">
                                <button
                                  className="btn btn-primary"
                                  type="submit"
                                  disabled={loading || success}
                                >
                                  {loading ? 'Resetting...' : 'Reset Password'}{' '}
                                  <i className="fas fa-key ms-1"></i>
                                </button>
                              </div>
                            </div>
                          </div>
                          </fieldset>
                        </form>
                      ) : (
                        <div className="text-center">
                          <p className="text-danger">{error}</p>
                          <Link to="/forgot-password" className="btn btn-primary">
                            Request New Reset Link
                          </Link>
                        </div>
                      )}
                      <div className="m-3 text-center text-muted">
                        <p className="mb-0">
                          Remember your password? <Link to="/login" className="text-primary ms-2">Log in</Link>
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ResetPassword;

