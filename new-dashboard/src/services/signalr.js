import * as signalR from '@microsoft/signalr';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:8080';

class SignalRService {
  constructor() {
    this.connection = null;
    this.isConnected = false;
    this.reconnectAttempts = 0;
    this.maxReconnectAttempts = 5;
    this.reconnectDelay = 3000;
  }

  async connect(token) {
    if (this.connection && this.isConnected) {
      return;
    }

    try {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${API_BASE_URL}/notificationHub`, {
          accessTokenFactory: () => token,
          skipNegotiation: false,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
              return this.reconnectDelay;
            }
            return null; // Stop reconnecting
          }
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Connection event handlers
      this.connection.onclose((error) => {
        this.isConnected = false;
        console.log('SignalR connection closed', error);
      });

      this.connection.onreconnecting((error) => {
        this.isConnected = false;
        console.log('SignalR reconnecting...', error);
      });

      this.connection.onreconnected((connectionId) => {
        this.isConnected = true;
        this.reconnectAttempts = 0;
        console.log('SignalR reconnected', connectionId);
      });

      await this.connection.start();
      this.isConnected = true;
      this.reconnectAttempts = 0;
      console.log('SignalR connected');

      // Join role-based groups
      await this.joinRoleGroups();
    } catch (error) {
      console.error('SignalR connection error:', error);
      this.isConnected = false;
      throw error;
    }
  }

  async joinRoleGroups() {
    if (!this.connection || !this.isConnected) {
      return;
    }

    try {
      // Get user role from token or user context
      // For now, we'll let the backend handle group assignment
      // The hub automatically adds users to user_{userId} group
    } catch (error) {
      console.error('Error joining role groups:', error);
    }
  }

  async disconnect() {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      this.isConnected = false;
    }
  }

  onNotification(callback) {
    if (this.connection) {
      this.connection.on('ReceiveNotification', (notification) => {
        callback(notification);
      });
    }
  }

  offNotification(callback) {
    if (this.connection) {
      this.connection.off('ReceiveNotification', callback);
    }
  }
}

export default new SignalRService();

