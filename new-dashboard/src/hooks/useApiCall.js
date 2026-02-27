import { useState } from 'react';
import { showToast } from '../utils/toast';
import { useLoading } from '../contexts/LoadingContext';

/**
 * Custom hook for handling API calls with toast notifications and loading states
 * @param {Object} options - Configuration options
 * @param {boolean} options.showSuccessToast - Whether to show success toast (default: true)
 * @param {boolean} options.showErrorToast - Whether to show error toast (default: true)
 * @param {string} options.successMessage - Custom success message
 * @param {string} options.errorMessage - Custom error message
 * @param {string} options.loadingMessage - Custom loading message
 * @returns {Object} - { execute, loading, error }
 */
export const useApiCall = (options = {}) => {
  const {
    showSuccessToast = true,
    showErrorToast = true,
    successMessage,
    errorMessage,
    loadingMessage = 'Processing...',
  } = options;

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const { startLoading, stopLoading } = useLoading();

  const execute = async (apiCall, customOptions = {}) => {
    const opts = { ...options, ...customOptions };
    setLoading(true);
    setError(null);
    startLoading(opts.loadingMessage || loadingMessage);

    try {
      const response = await apiCall();

      if (response.success !== false) {
        if (opts.showSuccessToast && (opts.successMessage || successMessage)) {
          showToast.success(opts.successMessage || successMessage);
        }
        return { success: true, data: response };
      } else {
        const errorMsg = opts.errorMessage || errorMessage || response.message || 'Operation failed';
        setError(errorMsg);
        if (opts.showErrorToast) {
          showToast.error(errorMsg);
        }
        return { success: false, error: errorMsg, data: response };
      }
    } catch (err) {
      const errorMsg = opts.errorMessage || errorMessage || err.response?.data?.message || err.message || 'An error occurred';
      setError(errorMsg);
      if (opts.showErrorToast) {
        showToast.error(errorMsg);
      }
      return { success: false, error: errorMsg };
    } finally {
      setLoading(false);
      stopLoading();
    }
  };

  return { execute, loading, error };
};

