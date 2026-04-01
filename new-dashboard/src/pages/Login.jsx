import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { showToast } from '../utils/toast';
import { useLoading } from '../contexts/LoadingContext';

const Login = () => {
  const [formData, setFormData] = useState({
    email: '',
    password: '',
  });
  const [rememberMe, setRememberMe] = useState(false);
  const [errors, setErrors] = useState({});
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();
  const { startLoading, stopLoading } = useLoading();

  // Load remembered email if exists
  useEffect(() => {
    const rememberedEmail = localStorage.getItem('rememberedEmail');
    if (rememberedEmail) {
      setFormData(prev => ({ ...prev, email: rememberedEmail }));
      setRememberMe(true);
    }
  }, []);

  const validateForm = () => {
    const newErrors = {};

    // Email validation
    if (!formData.email.trim()) {
      newErrors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Invalid email format';
    }

    // Password validation
    if (!formData.password) {
      newErrors.password = 'Password is required';
    } else if (formData.password.length < 6) {
      newErrors.password = 'Password must be at least 6 characters';
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
    // Clear error for this field when user starts typing
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

    if (!validateForm()) {
      return;
    }

    setLoading(true);
    startLoading('Logging in...');

    const result = await login(formData.email, formData.password, rememberMe);
    
    if (result.success) {
      // Save email if remember me is checked
      if (rememberMe) {
        localStorage.setItem('rememberedEmail', formData.email);
      } else {
        localStorage.removeItem('rememberedEmail');
      }
      showToast.success('Login successful!');
      navigate('/dashboard');
    } else {
      const errorMsg = result.error || 'Login failed. Please try again.';
      setError(errorMsg);
      showToast.error(errorMsg);
    }
    setLoading(false);
    stopLoading();
  };

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
                        <a href="/" className="logo logo-admin d-inline-block">
                          <span className="auth-logo-wrap">
                            <img src="/logo-sm.png" height="50" alt="One Rank Digital" className="auth-logo" />
                          </span>
                        </a>
                        <h4 className="mt-3 mb-1 fw-semibold text-white font-18">
                          Let's Get Started One Rank Digital
                        </h4>
                        <p className="auth-subtitle mb-0 small">Sign in to continue to One Rank Digital.</p>
                      </div>
                    </div>
                    <div className="card-body pt-0">
                      {error && (
                        <div className="alert alert-danger" role="alert">
                          {error}
                        </div>
                      )}
                      <form className="my-4" onSubmit={handleSubmit}>
                        <fieldset disabled={loading}>
                        <div className="form-group mb-2">
                          <label className="form-label" htmlFor="email">
                            Email
                          </label>
                          <input
                            type="email"
                            className={`form-control ${errors.email ? 'is-invalid' : ''}`}
                            id="email"
                            name="email"
                            placeholder="Enter email"
                            value={formData.email}
                            onChange={handleChange}
                            required
                          />
                          {errors.email && (
                            <div className="invalid-feedback d-block">{errors.email}</div>
                          )}
                        </div>

                        <div className="form-group">
                          <label className="form-label" htmlFor="password">
                            Password
                          </label>
                          <input
                            type="password"
                            className={`form-control ${errors.password ? 'is-invalid' : ''}`}
                            name="password"
                            id="password"
                            placeholder="Enter password"
                            value={formData.password}
                            onChange={handleChange}
                            required
                          />
                          {errors.password && (
                            <div className="invalid-feedback d-block">{errors.password}</div>
                          )}
                        </div>

                        <div className="form-group row mt-3">
                          <div className="col-sm-6">
                            <div className="form-check form-switch form-switch-success">
                              <input
                                className="form-check-input"
                                type="checkbox"
                                id="rememberMe"
                                checked={rememberMe}
                                onChange={(e) => setRememberMe(e.target.checked)}
                              />
                              <label className="form-check-label" htmlFor="rememberMe">
                                Remember me
                              </label>
                            </div>
                          </div>
                          <div className="col-sm-6 text-end">
                            <Link to="/forgot-password" className="text-muted font-13">
                              <i className="fas fa-lock"></i> Forgot password?
                            </Link>
                          </div>
                        </div>

                        <div className="form-group mb-0 row">
                          <div className="col-12">
                            <div className="d-grid mt-3">
                              <button
                                className="btn btn-primary"
                                type="submit"
                                disabled={loading}
                              >
                                {loading ? 'Logging in...' : 'Log In'}{' '}
                                <i className="fas fa-sign-in-alt ms-1"></i>
                              </button>
                            </div>
                          </div>
                        </div>
                        </fieldset>
                      </form>
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

export default Login;
