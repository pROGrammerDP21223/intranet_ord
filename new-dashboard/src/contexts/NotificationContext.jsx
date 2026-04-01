import { createContext, useContext } from 'react';

const NotificationContext = createContext({ notifications: [], isConnected: false });

export const useNotification = () => useContext(NotificationContext);

export const NotificationProvider = ({ children }) => {
  return (
    <NotificationContext.Provider value={{ notifications: [], isConnected: false }}>
      {children}
    </NotificationContext.Provider>
  );
};
