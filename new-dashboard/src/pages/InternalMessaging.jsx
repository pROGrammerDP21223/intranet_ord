import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import Loader from '../components/Loader';
import { messagesAPI, usersAPI } from '../services/api';
import { toast } from 'react-toastify';

const InternalMessaging = () => {
  const [messages, setMessages] = useState([]);
  const [activeTab, setActiveTab] = useState('inbox');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [selectedMessage, setSelectedMessage] = useState(null);
  const [showComposeModal, setShowComposeModal] = useState(false);
  const [users, setUsers] = useState([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [composeForm, setComposeForm] = useState({
    recipientId: '',
    subject: '',
    content: '',
  });

  useEffect(() => {
    loadData();
    loadUnreadCount();
    // Refresh unread count every 30 seconds
    const interval = setInterval(loadUnreadCount, 30000);
    return () => clearInterval(interval);
  }, [activeTab]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError('');
      let response;
      if (activeTab === 'inbox') {
        response = await messagesAPI.getInbox();
      } else {
        response = await messagesAPI.getSentMessages();
      }

      if (response.success && response.data) {
        setMessages(response.data);
      } else {
        setError(response.message || 'Failed to load messages');
      }
    } catch (err) {
      setError('An error occurred while loading messages');
      console.error('Failed to load messages:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadUsers = async () => {
    try {
      const response = await usersAPI.getUsers();
      if (response.success && response.data) {
        setUsers(response.data);
      }
    } catch (err) {
      console.error('Failed to load users:', err);
    }
  };

  const loadUnreadCount = async () => {
    try {
      const response = await messagesAPI.getUnreadCount();
      if (response.success && response.data) {
        setUnreadCount(response.data.count || 0);
      }
    } catch (err) {
      console.error('Failed to load unread count:', err);
    }
  };

  const handleCompose = () => {
    setComposeForm({ recipientId: '', subject: '', content: '' });
    setShowComposeModal(true);
    loadUsers();
  };

  const handleSend = async (e) => {
    e.preventDefault();
    if (!composeForm.recipientId || !composeForm.content) {
      toast.error('Please fill in all required fields');
      return;
    }

    try {
      setLoading(true);
      const response = await messagesAPI.sendMessage(composeForm);
      if (response.success) {
        toast.success('Message sent successfully');
        setShowComposeModal(false);
        loadData();
        loadUnreadCount();
      } else {
        toast.error(response.message || 'Failed to send message');
      }
    } catch (err) {
      toast.error('An error occurred while sending message');
      console.error('Send message error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleMessageClick = async (message) => {
    setSelectedMessage(message);
    if (!message.isRead && activeTab === 'inbox') {
      await messagesAPI.markAsRead(message.id);
      loadData();
      loadUnreadCount();
    }
  };

  const handleDelete = async (id, isSender) => {
    if (!window.confirm('Are you sure you want to delete this message?')) {
      return;
    }

    try {
      setLoading(true);
      const response = await messagesAPI.deleteMessage(id, isSender);
      if (response.success) {
        toast.success('Message deleted successfully');
        setSelectedMessage(null);
        loadData();
      } else {
        toast.error(response.message || 'Failed to delete message');
      }
    } catch (err) {
      toast.error('An error occurred while deleting message');
      console.error('Delete message error:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Layout>
      <div className="container-fluid">
        <div className="row">
          <div className="col-12">
            <div className="card">
              <div className="card-header">
                <div className="row align-items-center">
                  <div className="col">
                    <h4 className="card-title mb-0">Internal Messaging</h4>
                  </div>
                  <div className="col-auto">
                    <button type="button" className="btn btn-primary" onClick={handleCompose}>
                      <i className="fas fa-plus-circle me-1"></i> Compose
                    </button>
                  </div>
                </div>
              </div>
              <div className="card-body">
                <ul className="nav nav-tabs mb-3">
                  <li className="nav-item">
                    <button
                      type="button"
                      className={`nav-link ${activeTab === 'inbox' ? 'active' : ''}`}
                      onClick={() => {
                        setActiveTab('inbox');
                        setSelectedMessage(null);
                      }}
                    >
                      Inbox {unreadCount > 0 && <span className="badge bg-danger">{unreadCount}</span>}
                    </button>
                  </li>
                  <li className="nav-item">
                    <button
                      type="button"
                      className={`nav-link ${activeTab === 'sent' ? 'active' : ''}`}
                      onClick={() => {
                        setActiveTab('sent');
                        setSelectedMessage(null);
                      }}
                    >
                      Sent
                    </button>
                  </li>
                </ul>

                {error && <div className="alert alert-danger">{error}</div>}
                {loading && !messages.length ? (
                  <Loader />
                ) : (
                  <div className="row">
                    <div className="col-md-4">
                      <div className="list-group">
                        {messages.length === 0 ? (
                          <div className="list-group-item text-center text-muted">
                            No messages found
                          </div>
                        ) : (
                          messages.map((message) => (
                            <button
                              type="button"
                              key={message.id}
                              className={`list-group-item list-group-item-action text-start ${
                                selectedMessage?.id === message.id ? 'active' : ''
                              } ${!message.isRead && activeTab === 'inbox' ? 'fw-bold' : ''}`}
                              onClick={() => handleMessageClick(message)}
                            >
                              <div className="d-flex justify-content-between">
                                <div>
                                  <strong>
                                    {activeTab === 'inbox'
                                      ? message.sender?.name || 'Unknown'
                                      : message.recipient?.name || 'Unknown'}
                                  </strong>
                                  {message.subject && (
                                    <div className="small">{message.subject}</div>
                                  )}
                                  <div className="small text-muted">
                                    {message.content.substring(0, 50)}...
                                  </div>
                                </div>
                                <div className="text-end">
                                  <small className="text-muted">
                                    {new Date(message.createdAt).toLocaleDateString()}
                                  </small>
                                  {!message.isRead && activeTab === 'inbox' && (
                                    <span className="badge bg-primary ms-2">New</span>
                                  )}
                                </div>
                              </div>
                            </button>
                          ))
                        )}
                      </div>
                    </div>
                    <div className="col-md-8">
                      {selectedMessage ? (
                        <div className="card">
                          <div className="card-header">
                            <div className="row align-items-center">
                              <div className="col">
                                <h6 className="mb-0">
                                  {activeTab === 'inbox'
                                    ? `From: ${selectedMessage.sender?.name || 'Unknown'}`
                                    : `To: ${selectedMessage.recipient?.name || 'Unknown'}`}
                                </h6>
                                {selectedMessage.subject && (
                                  <small className="text-muted">{selectedMessage.subject}</small>
                                )}
                              </div>
                              <div className="col-auto">
                                <button
                                  type="button"
                                  className="btn btn-sm btn-danger"
                                  onClick={() =>
                                    handleDelete(
                                      selectedMessage.id,
                                      activeTab === 'sent'
                                    )
                                  }
                                >
                                  <i className="fas fa-trash"></i>
                                </button>
                              </div>
                            </div>
                          </div>
                          <div className="card-body">
                            <div className="mb-2">
                              <small className="text-muted">
                                {new Date(selectedMessage.createdAt).toLocaleString()}
                              </small>
                            </div>
                            <div>{selectedMessage.content}</div>
                          </div>
                        </div>
                      ) : (
                        <div className="text-center text-muted py-5">
                          Select a message to view
                        </div>
                      )}
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Compose Modal */}
        {showComposeModal && (
          <div
            className="modal show d-block"
            tabIndex="-1"
            style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}
          >
            <div className="modal-dialog">
              <div className="modal-content">
                <div className="modal-header">
                  <h5 className="modal-title">Compose Message</h5>
                  <button
                    type="button"
                    className="btn-close"
                    onClick={() => setShowComposeModal(false)}
                  ></button>
                </div>
                <form onSubmit={handleSend}>
                  <div className="modal-body">
                    <div className="mb-3">
                      <label className="form-label">To *</label>
                      <select
                        className="form-select"
                        value={composeForm.recipientId}
                        onChange={(e) =>
                          setComposeForm({ ...composeForm, recipientId: e.target.value })
                        }
                        required
                      >
                        <option value="">Select recipient</option>
                        {users.map((user) => (
                          <option key={user.id} value={user.id}>
                            {user.name} ({user.email})
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Subject</label>
                      <input
                        type="text"
                        className="form-control"
                        value={composeForm.subject}
                        onChange={(e) =>
                          setComposeForm({ ...composeForm, subject: e.target.value })
                        }
                      />
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Message *</label>
                      <textarea
                        className="form-control"
                        rows="8"
                        value={composeForm.content}
                        onChange={(e) =>
                          setComposeForm({ ...composeForm, content: e.target.value })
                        }
                        required
                      />
                    </div>
                  </div>
                  <div className="modal-footer">
                    <button
                      type="button"
                      className="btn btn-secondary"
                      onClick={() => setShowComposeModal(false)}
                    >
                      Cancel
                    </button>
                    <button type="submit" className="btn btn-primary" disabled={loading}>
                      {loading ? 'Sending...' : 'Send'}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
};

export default InternalMessaging;

