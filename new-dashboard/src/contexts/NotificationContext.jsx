import { createContext, useContext, useEffect, useState, useCallback } from 'react';
import { useAuth } from './AuthContext';
import signalRService from '../services/signalr';
import { toast } from 'react-toastify';

const NotificationContext = createContext();

export const useNotification = () => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error('useNotification must be used within NotificationProvider');
  }
  return context;
};

export const NotificationProvider = ({ children }) => {
  const { isAuthenticated, token } = useAuth();
  const [notifications, setNotifications] = useState([]);
  const [isConnected, setIsConnected] = useState(false);

  const handleNotification = useCallback((notification) => {
    // Add notification to state
    setNotifications((prev) => [notification, ...prev.slice(0, 49)]); // Keep last 50

    // Show toast notification
    const toastType = notification.Type === 'error' ? 'error' : 
                      notification.Type === 'warning' ? 'warning' : 
                      notification.Type === 'success' ? 'success' : 'info';

    toast[toastType](notification.Message, {
      position: 'top-right',
      autoClose: 5000,
      hideProgressBar: false,
      closeOnClick: true,
      pauseOnHover: true,
      draggable: true,
    });
  }, []);

  useEffect(() => {
    if (isAuthenticated && token) {
      // Connect to SignalR
      signalRService
        .connect(token)
        .then(() => {
          setIsConnected(true);
          signalRService.onNotification(handleNotification);
        })
        .catch((error) => {
          console.error('Failed to connect to SignalR:', error);
          setIsConnected(false);
        });

      return () => {
        signalRService.offNotification(handleNotification);
        signalRService.disconnect();
        setIsConnected(false);
      };
    } else {
      // Disconnect if not authenticated
      signalRService.disconnect();
      setIsConnected(false);
      setNotifications([]);
    }
  }, [isAuthenticated, token, handleNotification]);

  const clearNotifications = useCallback(() => {
    setNotifications([]);
  }, []);

  const removeNotification = useCallback((index) => {
    setNotifications((prev) => prev.filter((_, i) => i !== index));
  }, []);

  const value = {
    notifications,
    isConnected,
    clearNotifications,
    removeNotification,
  };

  return (
    <NotificationContext.Provider value={value}>
      {children}
    </NotificationContext.Provider>
  );
};

