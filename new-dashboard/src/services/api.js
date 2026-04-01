import axios from 'axios';

// Always default to same-origin in non-dev to avoid browser localhost/CORS issues behind reverse proxy.
const API_BASE_URL = import.meta.env.VITE_API_URL || '';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add token
api.interceptors.request.use(
  (config) => {
    // Check both localStorage (remember me) and sessionStorage
    const rememberMe = localStorage.getItem('rememberMe') === 'true';
    const token = rememberMe 
      ? localStorage.getItem('token') 
      : sessionStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      localStorage.removeItem('rememberMe');
      sessionStorage.removeItem('token');
      sessionStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Helper function to handle API responses with toast notifications
export const handleApiResponse = async (apiCall, options = {}) => {
  const {
    showSuccessToast = false,
    showErrorToast = true,
    successMessage,
    errorMessage,
    onSuccess,
    onError,
  } = options;

  try {
    const response = await apiCall();
    
    if (response.success !== false) {
      if (showSuccessToast && successMessage) {
        const { showToast } = await import('../utils/toast');
        showToast.success(successMessage);
      }
      if (onSuccess) {
        onSuccess(response);
      }
      return response;
    } else {
      const errorMsg = errorMessage || response.message || 'Operation failed';
      if (showErrorToast) {
        const { showToast } = await import('../utils/toast');
        showToast.error(errorMsg);
      }
      if (onError) {
        onError(response);
      }
      return response;
    }
  } catch (error) {
    const errorMsg = errorMessage || error.response?.data?.message || error.message || 'An error occurred';
    if (showErrorToast) {
      const { showToast } = await import('../utils/toast');
      showToast.error(errorMsg);
    }
    if (onError) {
      onError(error);
    }
    throw error;
  }
};

export const authAPI = {
  login: async (email, password) => {
    const response = await api.post('/api/auth/login', { email, password });
    return response.data;
  },
  register: async (name, email, password, confirmPassword, roleId = null) => {
    const response = await api.post('/api/auth/register', {
      name,
      email,
      password,
      confirmPassword,
      roleId,
    });
    return response.data;
  },
  getCurrentUser: async () => {
    const response = await api.get('/api/auth/me');
    return response.data;
  },
  forgotPassword: async (email) => {
    const response = await api.post('/api/auth/forgot-password', { email });
    return response.data;
  },
  resetPassword: async (data) => {
    const response = await api.post('/api/auth/reset-password', data);
    return response.data;
  },
  getRoles: async () => {
    const response = await api.get('/api/roles');
    return response.data;
  },
  getRoleById: async (id) => {
    const response = await api.get(`/api/roles/${id}`);
    return response.data;
  },
  getPermissions: async () => {
    const response = await api.get('/api/roles/permissions');
    return response.data;
  },
  createRole: async (roleData) => {
    const response = await api.post('/api/roles', roleData);
    return response.data;
  },
  updateRole: async (id, roleData) => {
    const response = await api.put(`/api/roles/${id}`, roleData);
    return response.data;
  },
  deleteRole: async (id) => {
    const response = await api.delete(`/api/roles/${id}`);
    return response.data;
  },
  assignPermissions: async (roleId, permissionIds) => {
    const response = await api.post(`/api/roles/${roleId}/permissions`, { PermissionIds: permissionIds });
    return response.data;
  },
};

export const usersAPI = {
  getUsers: async () => {
    const response = await api.get('/api/users');
    return response.data;
  },
  getUserById: async (id) => {
    const response = await api.get(`/api/users/${id}`);
    return response.data;
  },
  createUser: async (userData) => {
    const response = await api.post('/api/users', userData);
    return response.data;
  },
  updateUser: async (id, userData) => {
    const response = await api.put(`/api/users/${id}`, userData);
    return response.data;
  },
  deleteUser: async (id) => {
    const response = await api.delete(`/api/users/${id}`);
    return response.data;
  },
};

export const clientAPI = {
  createClient: async (clientData) => {
    const response = await api.post('/api/clients', clientData);
    return response.data;
  },
  getClients: async () => {
    const response = await api.get('/api/clients');
    return response.data;
  },
  getClientById: async (id) => {
    const response = await api.get(`/api/clients/${id}`);
    return response.data;
  },
  updateClient: async (id, clientData) => {
    const response = await api.put(`/api/clients/${id}`, clientData);
    return response.data;
  },
  deleteClient: async (id) => {
    const response = await api.delete(`/api/clients/${id}`);
    return response.data;
  },
  approveClient: async (id) => {
    const response = await api.post(`/api/clients/${id}/approve`);
    return response.data;
  },
  getEmailInfo: async (id) => {
    const response = await api.get(`/api/clients/${id}/email-info`);
    return response.data;
  },
  sendFormEmail: async (id, emails) => {
    const response = await api.post(`/api/clients/${id}/send-form-email`, { emails });
    return response.data;
  },
  togglePremium: async (id) => {
    const response = await api.patch(`/api/clients/${id}/toggle-premium`);
    return response.data;
  },
};

export const permissionsAPI = {
  getPermissions: async () => {
    const response = await api.get('/api/permissions');
    return response.data;
  },
  getPermissionsByCategory: async () => {
    const response = await api.get('/api/permissions/by-category');
    return response.data;
  },
  getPermissionById: async (id) => {
    const response = await api.get(`/api/permissions/${id}`);
    return response.data;
  },
  createPermission: async (permissionData) => {
    const response = await api.post('/api/permissions', permissionData);
    return response.data;
  },
  updatePermission: async (id, permissionData) => {
    const response = await api.put(`/api/permissions/${id}`, permissionData);
    return response.data;
  },
  deletePermission: async (id) => {
    const response = await api.delete(`/api/permissions/${id}`);
    return response.data;
  },
};

export const serviceAPI = {
  getServices: async (includeInactive = false) => {
    const response = await api.get(`/api/services${includeInactive ? '?includeInactive=true' : ''}`);
    return response.data;
  },
  getServicesByCategory: async () => {
    const response = await api.get('/api/services/by-category');
    return response.data;
  },
  createService: async (serviceData) => {
    const response = await api.post('/api/services', serviceData);
    return response.data;
  },
  updateService: async (id, serviceData) => {
    const response = await api.put(`/api/services/${id}`, serviceData);
    return response.data;
  },
  deleteService: async (id) => {
    const response = await api.delete(`/api/services/${id}`);
    return response.data;
  },
};

export const transactionAPI = {
  getTransactionsByClient: async (clientId) => {
    const response = await api.get(`/api/transactions/client/${clientId}`);
    return response.data;
  },
  getTransactionById: async (id) => {
    const response = await api.get(`/api/transactions/${id}`);
    return response.data;
  },
  getClientBalance: async (clientId) => {
    const response = await api.get(`/api/transactions/client/${clientId}/balance`);
    return response.data;
  },
  createTransaction: async (transactionData) => {
    const response = await api.post('/api/transactions', transactionData);
    return response.data;
  },
  updateTransaction: async (id, transactionData) => {
    const response = await api.put(`/api/transactions/${id}`, transactionData);
    return response.data;
  },
  deleteTransaction: async (id) => {
    const response = await api.delete(`/api/transactions/${id}`);
    return response.data;
  },
};

export const industryAPI = {
  getIndustries: async () => {
    const response = await api.get('/api/Industries');
    return response.data;
  },
  getIndustryById: async (id) => {
    const response = await api.get(`/api/Industries/${id}`);
    return response.data;
  },
  createIndustry: async (industryData) => {
    const response = await api.post('/api/Industries', industryData);
    return response.data;
  },
  updateIndustry: async (id, industryData) => {
    const response = await api.put(`/api/Industries/${id}`, industryData);
    return response.data;
  },
  deleteIndustry: async (id) => {
    const response = await api.delete(`/api/Industries/${id}`);
    return response.data;
  },
};

export const categoryAPI = {
  getCategories: async () => {
    const response = await api.get('/api/Categories');
    return response.data;
  },
  getCategoriesByIndustry: async (industryId) => {
    const response = await api.get(`/api/Categories/by-industry/${industryId}`);
    return response.data;
  },
  getCategoryById: async (id) => {
    const response = await api.get(`/api/Categories/${id}`);
    return response.data;
  },
  createCategory: async (categoryData) => {
    const response = await api.post('/api/Categories', categoryData);
    return response.data;
  },
  updateCategory: async (id, categoryData) => {
    const response = await api.put(`/api/Categories/${id}`, categoryData);
    return response.data;
  },
  deleteCategory: async (id) => {
    const response = await api.delete(`/api/Categories/${id}`);
    return response.data;
  },
};

export const productAPI = {
  getProducts: async () => {
    const response = await api.get('/api/Products');
    return response.data;
  },
  getProductsByCategory: async (categoryId) => {
    const response = await api.get(`/api/Products/by-category/${categoryId}`);
    return response.data;
  },
  getProductsByClient: async (clientId) => {
    const response = await api.get(`/api/Products/by-client/${clientId}`);
    return response.data;
  },
  getProductById: async (id) => {
    const response = await api.get(`/api/Products/${id}`);
    return response.data;
  },
  createProduct: async (productData) => {
    const response = await api.post('/api/Products', productData);
    return response.data;
  },
  updateProduct: async (id, productData) => {
    const response = await api.put(`/api/Products/${id}`, productData);
    return response.data;
  },
  deleteProduct: async (id) => {
    const response = await api.delete(`/api/Products/${id}`);
    return response.data;
  },
};

export const clientProductAPI = {
  getClientProducts: async (clientId) => {
    const response = await api.get(`/api/ClientProducts/client/${clientId}`);
    return response.data;
  },
  attachProductToClient: async (clientId, productId) => {
    const response = await api.post('/api/ClientProducts/attach', {
      clientId,
      productId,
    });
    return response.data;
  },
  attachMultipleProductsToClient: async (clientId, productIds) => {
    const response = await api.post('/api/ClientProducts/attach-multiple', {
      clientId,
      productIds,
    });
    return response.data;
  },
  detachProductFromClient: async (clientId, productId) => {
    const response = await api.post('/api/ClientProducts/detach', {
      clientId,
      productId,
    });
    return response.data;
  },
  checkProductAttached: async (clientId, productId) => {
    const response = await api.get(`/api/ClientProducts/check/${clientId}/${productId}`);
    return response.data;
  },
};

export const userClientAPI = {
  attachUserToClient: async (userId, clientId) => {
    const response = await api.post('/api/UserClients/attach', {
      userId,
      clientId,
    });
    return response.data;
  },
  detachUserFromClient: async (userId, clientId) => {
    const response = await api.post('/api/UserClients/detach', {
      userId,
      clientId,
    });
    return response.data;
  },
  getUserClients: async (userId) => {
    const response = await api.get(`/api/UserClients/user/${userId}`);
    return response.data;
  },
  getClientUsers: async (clientId) => {
    const response = await api.get(`/api/UserClients/client/${clientId}`);
    return response.data;
  },
};

export const salesPersonClientAPI = {
  attachSalesPersonToClient: async (salesPersonId, clientId) => {
    const response = await api.post('/api/SalesPersonClients/attach', {
      salesPersonId,
      clientId,
    });
    return response.data;
  },
  attachMultipleSalesPersonsToClient: async (clientId, salesPersonIds) => {
    const response = await api.post('/api/SalesPersonClients/attach-multiple', {
      clientId,
      salesPersonIds,
    });
    return response.data;
  },
  detachSalesPersonFromClient: async (salesPersonId, clientId) => {
    const response = await api.post('/api/SalesPersonClients/detach', {
      salesPersonId,
      clientId,
    });
    return response.data;
  },
  getSalesPersonClients: async (salesPersonId) => {
    const response = await api.get(`/api/SalesPersonClients/salesperson/${salesPersonId}`);
    return response.data;
  },
  getClientSalesPersons: async (clientId) => {
    const response = await api.get(`/api/SalesPersonClients/client/${clientId}`);
    return response.data;
  },
};

export const salesManagerSalesPersonAPI = {
  attachSalesManagerToSalesPerson: async (salesManagerId, salesPersonId) => {
    const response = await api.post('/api/SalesManagerSalesPersons/attach', {
      salesManagerId,
      salesPersonId,
    });
    return response.data;
  },
  detachSalesManagerFromSalesPerson: async (salesManagerId, salesPersonId) => {
    const response = await api.post('/api/SalesManagerSalesPersons/detach', {
      salesManagerId,
      salesPersonId,
    });
    return response.data;
  },
  getSalesManagerSalesPersons: async (salesManagerId) => {
    const response = await api.get(`/api/SalesManagerSalesPersons/salesmanager/${salesManagerId}`);
    return response.data;
  },
  getSalesPersonManagers: async (salesPersonId) => {
    const response = await api.get(`/api/SalesManagerSalesPersons/salesperson/${salesPersonId}`);
    return response.data;
  },
};

export const salesManagerClientAPI = {
  attachSalesManagerToClient: async (salesManagerId, clientId) => {
    const response = await api.post('/api/SalesManagerClients/attach', {
      salesManagerId,
      clientId,
    });
    return response.data;
  },
  detachSalesManagerFromClient: async (salesManagerId, clientId) => {
    const response = await api.post('/api/SalesManagerClients/detach', {
      salesManagerId,
      clientId,
    });
    return response.data;
  },
  getSalesManagerClients: async (salesManagerId) => {
    const response = await api.get(`/api/SalesManagerClients/salesmanager/${salesManagerId}`);
    return response.data;
  },
  getClientSalesManagers: async (clientId) => {
    const response = await api.get(`/api/SalesManagerClients/client/${clientId}`);
    return response.data;
  },
};

export const ownerClientAPI = {
  attachOwnerToClient: async (ownerId, clientId) => {
    const response = await api.post('/api/OwnerClients/attach', {
      ownerId,
      clientId,
    });
    return response.data;
  },
  detachOwnerFromClient: async (ownerId, clientId) => {
    const response = await api.post('/api/OwnerClients/detach', {
      ownerId,
      clientId,
    });
    return response.data;
  },
  getOwnerClients: async (ownerId) => {
    const response = await api.get(`/api/OwnerClients/owner/${ownerId}`);
    return response.data;
  },
  getClientOwners: async (clientId) => {
    const response = await api.get(`/api/OwnerClients/client/${clientId}`);
    return response.data;
  },
};

export const imageUploadAPI = {
  uploadImage: async (file, folder = 'products') => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post(`/api/ImageUpload/upload?folder=${folder}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
  uploadMultipleImages: async (files, folder = 'products') => {
    const formData = new FormData();
    files.forEach((file) => {
      formData.append('files', file);
    });
    const response = await api.post(`/api/ImageUpload/upload-multiple?folder=${folder}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
  deleteImage: async (url) => {
    const response = await api.delete(`/api/ImageUpload/delete?url=${encodeURIComponent(url)}`);
    return response.data;
  },
};

// Public API (requires API Key) for creating enquiries
// This can be used by any external website with valid API key
export const publicEnquiryAPI = {
  createEnquiry: async (enquiryData, apiKey) => {
    // Use axios directly with API key header
    const response = await axios.post(`${API_BASE_URL}/api/enquiries`, enquiryData, {
      headers: {
        'X-API-Key': apiKey,
        'Content-Type': 'application/json',
      },
    });
    return response.data;
  },
};

// Private API (authentication required) for managing enquiries
export const enquiryAPI = {
  getEnquiries: async (startDate = null, endDate = null) => {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    const queryString = params.toString();
    const response = await api.get(`/api/enquiries${queryString ? '?' + queryString : ''}`);
    return response.data;
  },
  getEnquiryById: async (id) => {
    const response = await api.get(`/api/enquiries/${id}`);
    return response.data;
  },
  getEnquiriesByStatus: async (status, startDate = null, endDate = null) => {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    const queryString = params.toString();
    const response = await api.get(`/api/enquiries/status/${status}${queryString ? '?' + queryString : ''}`);
    return response.data;
  },
  getEnquiriesByClient: async (clientId, startDate = null, endDate = null) => {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    const queryString = params.toString();
    const response = await api.get(`/api/enquiries/client/${clientId}${queryString ? '?' + queryString : ''}`);
    return response.data;
  },
  updateEnquiry: async (id, enquiryData) => {
    const response = await api.put(`/api/enquiries/${id}`, enquiryData);
    return response.data;
  },
  deleteEnquiry: async (id) => {
    const response = await api.delete(`/api/enquiries/${id}`);
    return response.data;
  },
  getStatistics: async () => {
    const response = await api.get('/api/enquiries/statistics');
    return response.data;
  },
};

// Analytics API
export const analyticsAPI = {
  getAnalytics: async (startDate = null, endDate = null, period = 'monthly') => {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (period) params.append('period', period);
    const queryString = params.toString();
    const response = await api.get(`/api/analytics${queryString ? '?' + queryString : ''}`);
    return response.data;
  },
};

// API Key Management
export const apiKeyAPI = {
  createApiKey: async (apiKeyData) => {
    const response = await api.post('/api/apikeys', apiKeyData);
    return response.data;
  },
  getAllApiKeys: async () => {
    const response = await api.get('/api/apikeys');
    return response.data;
  },
  getApiKeyById: async (id) => {
    const response = await api.get(`/api/apikeys/${id}`);
    return response.data;
  },
  getApiKeysByClient: async (clientId) => {
    const response = await api.get(`/api/apikeys/client/${clientId}`);
    return response.data;
  },
  updateApiKey: async (id, apiKeyData) => {
    const response = await api.put(`/api/apikeys/${id}`, apiKeyData);
    return response.data;
  },
  revokeApiKey: async (apiKeyId) => {
    const response = await api.post(`/api/apikeys/${apiKeyId}/revoke`);
    return response.data;
  },
  reactivateApiKey: async (apiKeyId) => {
    const response = await api.post(`/api/apikeys/${apiKeyId}/reactivate`);
    return response.data;
  },
  deleteApiKey: async (id) => {
    const response = await api.delete(`/api/apikeys/${id}`);
    return response.data;
  },
};

// Ticket Management
export const ticketAPI = {
  createTicket: async (ticketData) => {
    const response = await api.post('/api/tickets', ticketData);
    return response.data;
  },
  getTickets: async () => {
    const response = await api.get('/api/tickets');
    return response.data;
  },
  getTicketById: async (id) => {
    const response = await api.get(`/api/tickets/${id}`);
    return response.data;
  },
  getTicketsByClient: async (clientId) => {
    const response = await api.get(`/api/tickets/client/${clientId}`);
    return response.data;
  },
  getTicketsByStatus: async (status) => {
    const response = await api.get(`/api/tickets/status/${status}`);
    return response.data;
  },
  updateTicket: async (id, ticketData) => {
    const response = await api.put(`/api/tickets/${id}`, ticketData);
    return response.data;
  },
  assignTicket: async (id, assignedToUserId) => {
    const response = await api.post(`/api/tickets/${id}/assign`, {
      assignedToUserId,
    });
    return response.data;
  },
  addComment: async (id, comment, isInternal = false) => {
    const response = await api.post(`/api/tickets/${id}/comments`, {
      comment,
      isInternal,
    });
    return response.data;
  },
  getComments: async (id) => {
    const response = await api.get(`/api/tickets/${id}/comments`);
    return response.data;
  },
  deleteTicket: async (id) => {
    const response = await api.delete(`/api/tickets/${id}`);
    return response.data;
  },
};

// Export API
export const exportAPI = {
  exportClientsToExcel: async () => {
    const rememberMe = localStorage.getItem('rememberMe') === 'true';
    const token = rememberMe ? localStorage.getItem('token') : sessionStorage.getItem('token');
    
    const response = await fetch(`${API_BASE_URL}/api/export/clients/excel`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });
    
    if (!response.ok) {
      throw new Error('Failed to export clients');
    }
    
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `clients_${new Date().toISOString().slice(0, 10)}.xlsx`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
  },

  exportClientsToCsv: async () => {
    const rememberMe = localStorage.getItem('rememberMe') === 'true';
    const token = rememberMe ? localStorage.getItem('token') : sessionStorage.getItem('token');
    
    const response = await fetch(`${API_BASE_URL}/api/export/clients/csv`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });
    
    if (!response.ok) {
      throw new Error('Failed to export clients');
    }
    
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `clients_${new Date().toISOString().slice(0, 10)}.csv`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
  },

  exportEnquiriesToExcel: async (startDate, endDate) => {
    const rememberMe = localStorage.getItem('rememberMe') === 'true';
    const token = rememberMe ? localStorage.getItem('token') : sessionStorage.getItem('token');
    
    let url = `${API_BASE_URL}/api/export/enquiries/excel`;
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (params.toString()) url += `?${params.toString()}`;
    
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });
    
    if (!response.ok) {
      throw new Error('Failed to export enquiries');
    }
    
    const blob = await response.blob();
    const urlObj = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = urlObj;
    a.download = `enquiries_${new Date().toISOString().slice(0, 10)}.xlsx`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(urlObj);
    document.body.removeChild(a);
  },

  exportEnquiriesToCsv: async (startDate, endDate) => {
    const rememberMe = localStorage.getItem('rememberMe') === 'true';
    const token = rememberMe ? localStorage.getItem('token') : sessionStorage.getItem('token');
    
    let url = `${API_BASE_URL}/api/export/enquiries/csv`;
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (params.toString()) url += `?${params.toString()}`;
    
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });
    
    if (!response.ok) {
      throw new Error('Failed to export enquiries');
    }
    
    const blob = await response.blob();
    const urlObj = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = urlObj;
    a.download = `enquiries_${new Date().toISOString().slice(0, 10)}.csv`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(urlObj);
    document.body.removeChild(a);
  },

  exportTransactionsToExcel: async (clientId, startDate, endDate) => {
    const rememberMe = localStorage.getItem('rememberMe') === 'true';
    const token = rememberMe ? localStorage.getItem('token') : sessionStorage.getItem('token');
    
    let url = `${API_BASE_URL}/api/export/transactions/excel`;
    const params = new URLSearchParams();
    if (clientId) params.append('clientId', clientId);
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (params.toString()) url += `?${params.toString()}`;
    
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });
    
    if (!response.ok) {
      throw new Error('Failed to export transactions');
    }
    
    const blob = await response.blob();
    const urlObj = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = urlObj;
    a.download = `transactions_${new Date().toISOString().slice(0, 10)}.xlsx`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(urlObj);
    document.body.removeChild(a);
  },

  exportTransactionsToCsv: async (clientId, startDate, endDate) => {
    const rememberMe = localStorage.getItem('rememberMe') === 'true';
    const token = rememberMe ? localStorage.getItem('token') : sessionStorage.getItem('token');
    
    let url = `${API_BASE_URL}/api/export/transactions/csv`;
    const params = new URLSearchParams();
    if (clientId) params.append('clientId', clientId);
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (params.toString()) url += `?${params.toString()}`;
    
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });
    
    if (!response.ok) {
      throw new Error('Failed to export transactions');
    }
    
    const blob = await response.blob();
    const urlObj = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = urlObj;
    a.download = `transactions_${new Date().toISOString().slice(0, 10)}.csv`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(urlObj);
    document.body.removeChild(a);
  },
};

// Email Templates API
export const emailTemplatesAPI = {
  getAllTemplates: async () => {
    const response = await api.get('/api/EmailTemplates');
    return response.data;
  },
  getTemplateById: async (id) => {
    const response = await api.get(`/api/EmailTemplates/${id}`);
    return response.data;
  },
  getTemplateByType: async (templateType) => {
    const response = await api.get(`/api/EmailTemplates/type/${templateType}`);
    return response.data;
  },
  createTemplate: async (templateData) => {
    const response = await api.post('/api/EmailTemplates', templateData);
    return response.data;
  },
  updateTemplate: async (id, templateData) => {
    const response = await api.put(`/api/EmailTemplates/${id}`, templateData);
    return response.data;
  },
  deleteTemplate: async (id) => {
    const response = await api.delete(`/api/EmailTemplates/${id}`);
    return response.data;
  },
};

// Audit Logs API
export const auditLogsAPI = {
  getAuditLogs: async (filters = {}) => {
    const params = new URLSearchParams();
    if (filters.entityType) params.append('entityType', filters.entityType);
    if (filters.entityId) params.append('entityId', filters.entityId);
    if (filters.userId) params.append('userId', filters.userId);
    if (filters.startDate) params.append('startDate', filters.startDate);
    if (filters.endDate) params.append('endDate', filters.endDate);
    const queryString = params.toString();
    const response = await api.get(`/api/AuditLogs${queryString ? '?' + queryString : ''}`);
    return response.data;
  },
  getAuditLogById: async (id) => {
    const response = await api.get(`/api/AuditLogs/${id}`);
    return response.data;
  },
};

// Documents API
export const documentsAPI = {
  getDocumentsByEntity: async (entityType, entityId) => {
    const response = await api.get(`/api/Documents/entity/${entityType}/${entityId}`);
    return response.data;
  },
  getDocumentById: async (id) => {
    const response = await api.get(`/api/Documents/${id}`);
    return response.data;
  },
  uploadDocument: async (file, documentData) => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('EntityType', documentData.entityType);
    formData.append('EntityId', documentData.entityId);
    if (documentData.category) formData.append('Category', documentData.category);
    if (documentData.description) formData.append('Description', documentData.description);
    if (documentData.tags) formData.append('Tags', documentData.tags);
    
    const response = await api.post('/api/Documents/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
  downloadDocument: async (id) => {
    const rememberMe = localStorage.getItem('rememberMe') === 'true';
    const token = rememberMe ? localStorage.getItem('token') : sessionStorage.getItem('token');
    
    const response = await fetch(`${API_BASE_URL}/api/Documents/${id}/download`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });
    
    if (!response.ok) {
      throw new Error('Failed to download document');
    }
    
    const blob = await response.blob();
    return blob;
  },
  deleteDocument: async (id) => {
    const response = await api.delete(`/api/Documents/${id}`);
    return response.data;
  },
  getDocumentVersions: async (id) => {
    const response = await api.get(`/api/Documents/${id}/versions`);
    return response.data;
  },
};

// Workflows API
export const workflowsAPI = {
  getAllWorkflows: async () => {
    const response = await api.get('/api/Workflows');
    return response.data;
  },
  getWorkflowById: async (id) => {
    const response = await api.get(`/api/Workflows/${id}`);
    return response.data;
  },
  createWorkflow: async (workflowData) => {
    const response = await api.post('/api/Workflows', workflowData);
    return response.data;
  },
  updateWorkflow: async (id, workflowData) => {
    const response = await api.put(`/api/Workflows/${id}`, workflowData);
    return response.data;
  },
  deleteWorkflow: async (id) => {
    const response = await api.delete(`/api/Workflows/${id}`);
    return response.data;
  },
};

// Internal Messaging API
export const messagesAPI = {
  getInbox: async () => {
    const response = await api.get('/api/Messages/inbox');
    return response.data;
  },
  getSentMessages: async () => {
    const response = await api.get('/api/Messages/sent');
    return response.data;
  },
  getMessageById: async (id) => {
    const response = await api.get(`/api/Messages/${id}`);
    return response.data;
  },
  sendMessage: async (messageData) => {
    const response = await api.post('/api/Messages', messageData);
    return response.data;
  },
  markAsRead: async (id) => {
    const response = await api.post(`/api/Messages/${id}/read`);
    return response.data;
  },
  deleteMessage: async (id, isSender = false) => {
    const response = await api.delete(`/api/Messages/${id}?isSender=${isSender}`);
    return response.data;
  },
  getUnreadCount: async () => {
    const response = await api.get('/api/Messages/unread/count');
    return response.data;
  },
};

export const backupAPI = {
  createBackup: async (backupPath) => {
    const params = backupPath ? { backupPath } : {};
    const response = await api.post('/api/backup/create', null, { params });
    return response.data;
  },
  restoreBackup: async (backupFilePath) => {
    const response = await api.post('/api/backup/restore', { backupFilePath });
    return response.data;
  },
  listBackups: async (backupPath) => {
    const params = backupPath ? { backupPath } : {};
    const response = await api.get('/api/backup/list', { params });
    return response.data;
  },
  deleteBackup: async (backupFilePath) => {
    const response = await api.delete('/api/backup', { params: { backupFilePath } });
    return response.data;
  },
};

export const archiveAPI = {
  archiveOldClients: async (daysOld) => {
    const response = await api.post('/api/archive/clients', { daysOld });
    return response.data;
  },
  archiveOldEnquiries: async (daysOld) => {
    const response = await api.post('/api/archive/enquiries', { daysOld });
    return response.data;
  },
  archiveOldTickets: async (daysOld) => {
    const response = await api.post('/api/archive/tickets', { daysOld });
    return response.data;
  },
  archiveOldTransactions: async (daysOld) => {
    const response = await api.post('/api/archive/transactions', { daysOld });
    return response.data;
  },
  archiveOldAuditLogs: async (daysOld) => {
    const response = await api.post('/api/archive/audit-logs', { daysOld });
    return response.data;
  },
  getArchivedData: async (entityType, fromDate, toDate) => {
    const params = {};
    if (fromDate) params.fromDate = fromDate;
    if (toDate) params.toDate = toDate;
    const response = await api.get(`/api/archive/${entityType}`, { params });
    return response.data;
  },
  restoreArchivedData: async (entityType, entityId) => {
    const response = await api.post(`/api/archive/restore/${entityType}/${entityId}`);
    return response.data;
  },
};

export const backgroundJobsAPI = {
  scheduleTaskReminder: async (taskId, reminderDate) => {
    const response = await api.post('/api/backgroundjobs/schedule-task-reminder', {
      taskId,
      reminderDate,
    });
    return response.data;
  },
  scheduleBackup: async (backupTime) => {
    const response = await api.post('/api/backgroundjobs/schedule-backup', {
      backupTime,
    });
    return response.data;
  },
  scheduleArchiving: async (archiveDate) => {
    const response = await api.post('/api/backgroundjobs/schedule-archiving', {
      archiveDate,
    });
    return response.data;
  },
  deleteJob: async (jobId) => {
    const response = await api.delete(`/api/backgroundjobs/${jobId}`);
    return response.data;
  },
};

export const cacheStatisticsAPI = {
  getStatistics: async () => {
    const response = await api.get('/api/cachestatistics');
    return response.data;
  },
  clearStatistics: async () => {
    const response = await api.post('/api/cachestatistics/clear');
    return response.data;
  },
};

// Free Registrations API (Admin, Owner, SalesManager)
export const freeRegistrationAPI = {
  getAll: async (status = null) => {
    const params = status ? `?status=${status}` : '';
    const response = await api.get(`/api/free-registrations${params}`);
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(`/api/free-registrations/${id}`);
    return response.data;
  },
  approve: async (id, notes = '') => {
    const response = await api.post(`/api/free-registrations/${id}/approve`, { notes });
    return response.data;
  },
  reject: async (id, reason) => {
    const response = await api.post(`/api/free-registrations/${id}/reject`, { reason });
    return response.data;
  },
  updateNotes: async (id, notes) => {
    const response = await api.put(`/api/free-registrations/${id}/notes`, { notes });
    return response.data;
  },
  delete: async (id) => {
    const response = await api.delete(`/api/free-registrations/${id}`);
    return response.data;
  },
  getStats: async () => {
    const response = await api.get('/api/free-registrations/stats');
    return response.data;
  },
};

// Ordpanel Enquiries API (Admin, Owner only)
export const ordpanelEnquiryAPI = {
  getAll: async (filters = {}) => {
    const params = new URLSearchParams();
    if (filters.status) params.append('status', filters.status);
    if (filters.pageType) params.append('pageType', filters.pageType);
    if (filters.from) params.append('from', filters.from);
    if (filters.to) params.append('to', filters.to);
    const qs = params.toString();
    const response = await api.get(`/api/ordpanel-enquiries${qs ? '?' + qs : ''}`);
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(`/api/ordpanel-enquiries/${id}`);
    return response.data;
  },
  updateStatus: async (id, status) => {
    const response = await api.put(`/api/ordpanel-enquiries/${id}/status`, { status });
    return response.data;
  },
  delete: async (id) => {
    const response = await api.delete(`/api/ordpanel-enquiries/${id}`);
    return response.data;
  },
  getStats: async () => {
    const response = await api.get('/api/ordpanel-enquiries/stats');
    return response.data;
  },
};

// Contact Forms API (Admin, Owner only)
export const contactFormAPI = {
  getAll: async (status = null) => {
    const params = status ? `?status=${status}` : '';
    const response = await api.get(`/api/contact-forms${params}`);
    return response.data;
  },
  getById: async (id) => {
    const response = await api.get(`/api/contact-forms/${id}`);
    return response.data;
  },
  updateStatus: async (id, status) => {
    const response = await api.put(`/api/contact-forms/${id}/status`, { status });
    return response.data;
  },
  delete: async (id) => {
    const response = await api.delete(`/api/contact-forms/${id}`);
    return response.data;
  },
  getStats: async () => {
    const response = await api.get('/api/contact-forms/stats');
    return response.data;
  },
};

export default api;

