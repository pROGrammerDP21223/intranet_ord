import { useState } from 'react';
import { Link } from 'react-router-dom';
import { authAPI } from '../services/api';
import { showToast } from '../utils/toast';
import { useLoading } from '../contexts/LoadingContext';

const ForgotPassword = () => {
  const [email, setEmail] = useState('');
  const [errors, setErrors] = useState({});
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);
  const { startLoading, stopLoading } = useLoading();

  const validateForm = () => {
    const newErrors = {};

    if (!email.trim()) {
      newErrors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      newErrors.email = 'Invalid email format';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleChange = (e) => {
    setEmail(e.target.value);
    if (errors.email) {
      setErrors({ ...errors, email: '' });
    }
    setError('');
    setSuccess(false);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess(false);

    if (!validateForm()) {
      return;
    }

    setLoading(true);
    startLoading('Sending reset email...');

    try {
      const response = await authAPI.forgotPassword(email);
      setSuccess(true);
      setEmail('');
      showToast.success('If an account exists with this email, a password reset link has been sent.');
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Failed to send reset email';
      setError(message);
      showToast.error(message);
    } finally {
      setLoading(false);
      stopLoading();
    }
  };

  return (
    <div className="auth-page" style={{ backgroundImage: "url('/p-1.png')", backgroundSize: 'cover', backgroundPosition: 'center center' }}>
      <div className="container">
        <div className="row vh-100 d-flex justify-content-center">
          <div className="col-12 align-self-center">
            <div className="row">
              <div className="col-lg-5 mx-auto">
                <div className="card">
                  <div className="card-body p-0 auth-header-box">
                    <div className="text-center p-3">
                      <a href="/" className="logo logo-admin d-inline-block">
                        <span className="auth-logo-wrap">
                          <img src="/logo-sm.png" height="50" alt="One Rank Digital" className="auth-logo" />
                        </span>
                      </a>
                      <h4 className="mt-3 mb-1 fw-semibold text-white font-18">Reset Password For One Rank Digital</h4>
                      <p className="auth-subtitle mb-0 small">Enter your Email and instructions will be sent to you!</p>
                    </div>
                  </div>
                  <div className="card-body pt-0">
                    {success && (
                      <div className="alert alert-success" role="alert">
                        If an account exists with this email, a password reset link has been sent.
                      </div>
                    )}
                    {error && (
                      <div className="alert alert-danger" role="alert">
                        {error}
                      </div>
                    )}
                    <form className="my-4" onSubmit={handleSubmit}>
                      <fieldset disabled={loading}>
                      <div className="form-group mb-3">
                        <label className="form-label" htmlFor="userEmail">
                          Email
                        </label>
                        <input
                          type="email"
                          className={`form-control ${errors.email ? 'is-invalid' : ''}`}
                          id="userEmail"
                          name="Email"
                          placeholder="Enter Email Address"
                          value={email}
                          onChange={handleChange}
                          required
                        />
                        {errors.email && (
                          <div className="invalid-feedback d-block">{errors.email}</div>
                        )}
                      </div>

                      <div className="form-group mb-0 row">
                        <div className="col-12">
                          <button
                            className="btn btn-primary w-100"
                            type="submit"
                            disabled={loading}
                          >
                            {loading ? 'Sending...' : 'Reset'}{' '}
                            <i className="fas fa-paper-plane ms-1"></i>
                          </button>
                        </div>
                      </div>
                      </fieldset>
                    </form>
                    <div className="text-center text-muted">
                      <p className="mb-1">
                        Remember It ? <Link to="/login" className="text-primary ms-2">Sign in here</Link>
                      </p>
                    </div>
                  </div>
                  <div className="card-body bg-light-alt text-center">
                    &copy; {new Date().getFullYear()} One Rank Digital
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

export default ForgotPassword;

