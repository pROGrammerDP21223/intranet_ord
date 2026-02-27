import { createContext, useContext, useState, useEffect } from 'react';
import { authAPI } from '../services/api';

const AuthContext = createContext(null);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [token, setToken] = useState(localStorage.getItem('token'));

  useEffect(() => {
    // Check both localStorage (remember me) and sessionStorage
    const rememberMe = localStorage.getItem('rememberMe') === 'true';
    const storedToken = rememberMe 
      ? localStorage.getItem('token') 
      : sessionStorage.getItem('token');
    const storedUser = rememberMe 
      ? localStorage.getItem('user') 
      : sessionStorage.getItem('user');
    
    if (storedToken && storedUser) {
      setToken(storedToken);
      setUser(JSON.parse(storedUser));
    }
    setLoading(false);
  }, []);

  const login = async (email, password, rememberMe = false) => {
    try {
      const response = await authAPI.login(email, password);
      const responseData = response.data || response;
      
      // Backend returns: { message, data: { token, refreshToken, expiresAt, user }, success }
      const authData = responseData.data || responseData;
      
      if (authData && authData.token) {
        // Store token and user
        if (rememberMe) {
          // Store in localStorage (persists until cleared)
          localStorage.setItem('token', authData.token);
          localStorage.setItem('user', JSON.stringify(authData.user));
          localStorage.setItem('rememberMe', 'true');
        } else {
          // Store in sessionStorage (cleared when browser closes)
          sessionStorage.setItem('token', authData.token);
          sessionStorage.setItem('user', JSON.stringify(authData.user));
          localStorage.removeItem('rememberMe');
        }
        
        setToken(authData.token);
        setUser(authData.user);
        return { success: true, data: authData };
      }
      throw new Error('Invalid response format');
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Login failed';
      return { success: false, error: message };
    }
  };

  const register = async (name, email, password, confirmPassword, roleId = null) => {
    try {
      const response = await authAPI.register(name, email, password, confirmPassword, roleId);
      const responseData = response.data || response;
      
      // Backend returns: { message, data: { token, refreshToken, expiresAt, user }, success }
      const authData = responseData.data || responseData;
      
      if (authData && authData.token) {
        localStorage.setItem('token', authData.token);
        localStorage.setItem('user', JSON.stringify(authData.user));
        setToken(authData.token);
        setUser(authData.user);
        return { success: true, data: authData };
      }
      throw new Error('Invalid response format');
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Registration failed';
      return { success: false, error: message };
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    localStorage.removeItem('rememberMe');
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('user');
    setToken(null);
    setUser(null);
  };

  const value = {
    user,
    token,
    login,
    register,
    logout,
    isAuthenticated: !!token,
    loading,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

