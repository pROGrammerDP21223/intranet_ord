import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import Layout from '../components/Layout';
import Loader from '../components/Loader';
import { documentsAPI } from '../services/api';
import { toast } from 'react-toastify';

const DocumentsManagement = () => {
  const { entityType, entityId } = useParams();
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showUploadModal, setShowUploadModal] = useState(false);
  const [uploadForm, setUploadForm] = useState({
    file: null,
    category: '',
    description: '',
    tags: '',
  });

  useEffect(() => {
    if (entityType && entityId) {
      loadDocuments();
    }
  }, [entityType, entityId]);

  const loadDocuments = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await documentsAPI.getDocumentsByEntity(entityType, parseInt(entityId));
      if (response.success && response.data) {
        setDocuments(response.data);
      } else {
        setError(response.message || 'Failed to load documents');
      }
    } catch (err) {
      setError('An error occurred while loading documents');
      console.error('Failed to load documents:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleFileChange = (e) => {
    setUploadForm({ ...uploadForm, file: e.target.files[0] });
  };

  const handleUpload = async (e) => {
    e.preventDefault();
    if (!uploadForm.file) {
      toast.error('Please select a file');
      return;
    }

    try {
      setLoading(true);
      const documentData = {
        entityType: entityType || 'Client',
        entityId: parseInt(entityId || 0),
        category: uploadForm.category,
        description: uploadForm.description,
        tags: uploadForm.tags,
      };

      const response = await documentsAPI.uploadDocument(uploadForm.file, documentData);
      if (response.success) {
        toast.success('Document uploaded successfully');
        setShowUploadModal(false);
        setUploadForm({ file: null, category: '', description: '', tags: '' });
        loadDocuments();
      } else {
        toast.error(response.message || 'Failed to upload document');
      }
    } catch (err) {
      toast.error('An error occurred while uploading document');
      console.error('Upload error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDownload = async (id, fileName) => {
    try {
      const blob = await documentsAPI.downloadDocument(id);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      toast.success('Document downloaded');
    } catch (err) {
      toast.error('Failed to download document');
      console.error('Download error:', err);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this document?')) {
      return;
    }

    try {
      setLoading(true);
      const response = await documentsAPI.deleteDocument(id);
      if (response.success) {
        toast.success('Document deleted successfully');
        loadDocuments();
      } else {
        toast.error(response.message || 'Failed to delete document');
      }
    } catch (err) {
      toast.error('An error occurred while deleting document');
      console.error('Delete error:', err);
    } finally {
      setLoading(false);
    }
  };

  const formatFileSize = (bytes) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
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
                    <h4 className="card-title mb-0">
                      Documents - {entityType} #{entityId}
                    </h4>
                  </div>
                  <div className="col-auto">
                    <button
                      type="button"
                      className="btn btn-primary"
                      onClick={() => setShowUploadModal(true)}
                    >
                      <i className="fas fa-upload me-1"></i> Upload Document
                    </button>
                  </div>
                </div>
              </div>
              <div className="card-body">
                {error && <div className="alert alert-danger">{error}</div>}
                {loading && !documents.length ? (
                  <Loader />
                ) : (
                  <div className="table-responsive">
                    <table className="table table-striped">
                      <thead>
                        <tr>
                          <th>File Name</th>
                          <th>Category</th>
                          <th>Type</th>
                          <th>Size</th>
                          <th>Uploaded By</th>
                          <th>Upload Date</th>
                          <th>Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {documents.length === 0 ? (
                          <tr>
                            <td colSpan="7" className="text-center">
                              No documents found
                            </td>
                          </tr>
                        ) : (
                          documents.map((doc) => (
                            <tr key={doc.id}>
                              <td>{doc.fileName}</td>
                              <td>
                                {doc.category && (
                                  <span className="badge bg-info">{doc.category}</span>
                                )}
                              </td>
                              <td>{doc.fileType}</td>
                              <td>{formatFileSize(doc.fileSize)}</td>
                              <td>{doc.uploadedBy?.name || 'Unknown'}</td>
                              <td>{new Date(doc.uploadedAt).toLocaleDateString()}</td>
                              <td>
                                <button
                                  className="btn btn-sm btn-primary me-2"
                                  onClick={() => handleDownload(doc.id, doc.fileName)}
                                >
                                  <i className="fas fa-download"></i>
                                </button>
                                <button
                                  className="btn btn-sm btn-danger"
                                  onClick={() => handleDelete(doc.id)}
                                >
                                  <i className="fas fa-trash"></i>
                                </button>
                              </td>
                            </tr>
                          ))
                        )}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Upload Modal */}
        {showUploadModal && (
          <div
            className="modal show d-block"
            tabIndex="-1"
            style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}
          >
            <div className="modal-dialog">
              <div className="modal-content">
                <div className="modal-header">
                  <h5 className="modal-title">Upload Document</h5>
                  <button
                    type="button"
                    className="btn-close"
                    onClick={() => setShowUploadModal(false)}
                  ></button>
                </div>
                <form onSubmit={handleUpload}>
                  <div className="modal-body">
                    <div className="mb-3">
                      <label className="form-label">File *</label>
                      <input
                        type="file"
                        className="form-control"
                        onChange={handleFileChange}
                        required
                      />
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Category</label>
                      <input
                        type="text"
                        className="form-control"
                        value={uploadForm.category}
                        onChange={(e) =>
                          setUploadForm({ ...uploadForm, category: e.target.value })
                        }
                        placeholder="e.g., Contract, Invoice"
                      />
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Description</label>
                      <textarea
                        className="form-control"
                        rows="3"
                        value={uploadForm.description}
                        onChange={(e) =>
                          setUploadForm({ ...uploadForm, description: e.target.value })
                        }
                      />
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Tags (comma-separated)</label>
                      <input
                        type="text"
                        className="form-control"
                        value={uploadForm.tags}
                        onChange={(e) =>
                          setUploadForm({ ...uploadForm, tags: e.target.value })
                        }
                        placeholder="e.g., important, contract, 2024"
                      />
                    </div>
                  </div>
                  <div className="modal-footer">
                    <button
                      type="button"
                      className="btn btn-secondary"
                      onClick={() => setShowUploadModal(false)}
                    >
                      Cancel
                    </button>
                    <button type="submit" className="btn btn-primary" disabled={loading}>
                      {loading ? 'Uploading...' : 'Upload'}
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

export default DocumentsManagement;

