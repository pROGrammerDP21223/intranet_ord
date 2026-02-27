import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { authAPI } from '../services/api';
import { showToast } from '../utils/toast';
import { useLoading } from '../contexts/LoadingContext';

const Register = () => {
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    password: '',
    confirmPassword: '',
    roleId: null,
  });
  const [roles, setRoles] = useState([]);
  const [loadingRoles, setLoadingRoles] = useState(true);
  const [agreeToTerms, setAgreeToTerms] = useState(false);
  const [errors, setErrors] = useState({});
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();
  const { startLoading, stopLoading } = useLoading();

  useEffect(() => {
    loadRoles();
  }, []);

  const loadRoles = async () => {
    try {
      const response = await authAPI.getRoles();
      if (response.success && response.data) {
        setRoles(response.data);
        // Set default role to Employee if available
        const employeeRole = response.data.find(r => r.name === 'Employee');
        if (employeeRole) {
          setFormData(prev => ({ ...prev, roleId: employeeRole.id }));
        }
      } else {
        showToast.error('Failed to load roles');
      }
    } catch (error) {
      showToast.error('Failed to load roles');
      console.error('Failed to load roles:', error);
    } finally {
      setLoadingRoles(false);
    }
  };

  const validateForm = () => {
    const newErrors = {};

    // Name validation
    if (!formData.name.trim()) {
      newErrors.name = 'Name is required';
    } else if (formData.name.trim().length < 2) {
      newErrors.name = 'Name must be at least 2 characters';
    } else if (formData.name.length > 100) {
      newErrors.name = 'Name must not exceed 100 characters';
    }

    // Email validation
    if (!formData.email.trim()) {
      newErrors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Invalid email format';
    } else if (formData.email.length > 255) {
      newErrors.email = 'Email must not exceed 255 characters';
    }

    // Password validation
    if (!formData.password) {
      newErrors.password = 'Password is required';
    } else if (formData.password.length < 6) {
      newErrors.password = 'Password must be at least 6 characters';
    } else if (formData.password.length > 100) {
      newErrors.password = 'Password must not exceed 100 characters';
    }

    // Confirm Password validation
    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Confirm Password is required';
    } else if (formData.password !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }

    // Terms validation
    if (!agreeToTerms) {
      newErrors.terms = 'You must agree to the terms of use';
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
    startLoading('Registering...');

    const result = await register(
      formData.name,
      formData.email,
      formData.password,
      formData.confirmPassword,
      formData.roleId
    );

    if (result.success) {
      showToast.success('Registration successful!');
      navigate('/dashboard');
    } else {
      // Handle backend validation errors
      if (result.error && typeof result.error === 'object') {
        const backendErrors = {};
        Object.keys(result.error).forEach(key => {
          backendErrors[key.toLowerCase()] = Array.isArray(result.error[key]) 
            ? result.error[key][0] 
            : result.error[key];
        });
        setErrors(backendErrors);
        showToast.error('Please fix the form errors');
      } else {
        const errorMsg = result.error || 'Registration failed. Please try again.';
        setError(errorMsg);
        showToast.error(errorMsg);
      }
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
                        <a href="/" className="logo logo-admin">
                          <img src="/logo-sm.png" height="50" alt="logo" className="auth-logo" />
                        </a>
                        <h4 className="mt-3 mb-1 fw-semibold text-white font-18">
                          Let's Get Started One Rank Digital
                        </h4>
                        <p className="text-muted mb-0">Sign up to continue to One Rank Digital.</p>
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
                          <label className="form-label" htmlFor="name">
                            Name
                          </label>
                          <input
                            type="text"
                            className={`form-control ${errors.name ? 'is-invalid' : ''}`}
                            id="name"
                            name="name"
                            placeholder="Enter name"
                            value={formData.name}
                            onChange={handleChange}
                            required
                          />
                          {errors.name && (
                            <div className="invalid-feedback d-block">{errors.name}</div>
                          )}
                        </div>

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

                        <div className="form-group mb-2">
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

                        <div className="form-group mb-2">
                          <label className="form-label" htmlFor="confirmPassword">
                            Confirm Password
                          </label>
                          <input
                            type="password"
                            className={`form-control ${errors.confirmPassword ? 'is-invalid' : ''}`}
                            name="confirmPassword"
                            id="confirmPassword"
                            placeholder="Enter confirm password"
                            value={formData.confirmPassword}
                            onChange={handleChange}
                            required
                          />
                          {errors.confirmPassword && (
                            <div className="invalid-feedback d-block">{errors.confirmPassword}</div>
                          )}
                        </div>

                        <div className="form-group mb-2">
                          <label className="form-label" htmlFor="roleId">
                            Role
                          </label>
                          <select
                            className={`form-control ${errors.roleId ? 'is-invalid' : ''}`}
                            name="roleId"
                            id="roleId"
                            value={formData.roleId || ''}
                            onChange={(e) => {
                              const value = e.target.value ? parseInt(e.target.value) : null;
                              setFormData(prev => ({ ...prev, roleId: value }));
                              if (errors.roleId) {
                                setErrors(prev => ({ ...prev, roleId: '' }));
                              }
                            }}
                            disabled={loadingRoles}
                            required
                          >
                            {loadingRoles ? (
                              <option>Loading roles...</option>
                            ) : (
                              <>
                                <option value="">Select a role</option>
                                {roles.map((role) => (
                                  <option key={role.id} value={role.id}>
                                    {role.name} {role.description ? `- ${role.description}` : ''}
                                  </option>
                                ))}
                              </>
                            )}
                          </select>
                          {errors.roleId && (
                            <div className="invalid-feedback d-block">{errors.roleId}</div>
                          )}
                        </div>

                        <div className="form-group row mt-3">
                          <div className="col-12">
                            <div className="form-check form-switch form-switch-success">
                              <input
                                className={`form-check-input ${errors.terms ? 'is-invalid' : ''}`}
                                type="checkbox"
                                id="agreeToTerms"
                                checked={agreeToTerms}
                                onChange={(e) => {
                                  setAgreeToTerms(e.target.checked);
                                  if (errors.terms) {
                                    setErrors({ ...errors, terms: '' });
                                  }
                                }}
                              />
                              <label className="form-check-label" htmlFor="agreeToTerms">
                                By registering you agree to the One Rank Digital{' '}
                                <a href="#" className="text-primary">
                                  Terms of Use
                                </a>
                              </label>
                            </div>
                            {errors.terms && (
                              <div className="invalid-feedback d-block">{errors.terms}</div>
                            )}
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
                                {loading ? 'Registering...' : 'Register'}{' '}
                                <i className="fas fa-user-plus ms-1"></i>
                              </button>
                            </div>
                          </div>
                        </div>
                        </fieldset>
                      </form>
                      <div className="m-3 text-center text-muted">
                        <p className="mb-0">Already have an account ? <Link to="/login" className="text-primary ms-2">Log in</Link></p>
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

export default Register;
