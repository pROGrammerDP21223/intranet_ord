import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { clientAPI } from '../services/api';
import ClientFormPrintView from '../components/ClientFormPrintView';
import printCSS from '../components/ClientFormPrint.css?raw';

const ClientFormPrint = () => {
  const { id } = useParams();
  const [client, setClient] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadClient();
  }, [id]);

  const loadClient = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await clientAPI.getClientById(id);
      if (response.success && response.data) {
        setClient(response.data);
      } else {
        setError(response.message || 'Client not found');
      }
    } catch (err) {
      setError('An error occurred while loading client');
      console.error('Failed to load client:', err);
    } finally {
      setLoading(false);
    }
  };

  const handlePrint = () => {
    const element = document.getElementById('client-form-pdf-content');

    // Create a fully isolated iframe with ONLY print.html CSS — no Bootstrap, no app CSS
    const iframe = document.createElement('iframe');
    iframe.style.cssText = 'position:fixed;width:0;height:0;border:0;left:-9999px;top:-9999px;';
    document.body.appendChild(iframe);

    const iDoc = iframe.contentDocument || iframe.contentWindow.document;
    iDoc.documentElement.innerHTML = `<head>
      <meta charset="UTF-8">
      <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet">
      <style>${printCSS}</style>
    </head><body>${element.innerHTML}</body>`;

    // Give fonts a moment to load, then trigger native print (Save as PDF in dialog)
    setTimeout(() => {
      iframe.contentWindow.focus();
      iframe.contentWindow.print();
      setTimeout(() => document.body.removeChild(iframe), 1000);
    }, 500);
  };

  if (loading) {
    return (
      <div className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
        <div className="text-center">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
          <p className="mt-2 text-muted">Loading client form...</p>
        </div>
      </div>
    );
  }

  if (error || !client) {
    return (
      <div className="min-vh-100 d-flex align-items-center justify-content-center bg-light p-4">
        <div className="text-center">
          <div className="alert alert-danger">{error || 'Client not found'}</div>
          <Link to={`/clients/${id}`} className="btn btn-primary">Back to Client</Link>
        </div>
      </div>
    );
  }

  return (
    <>
      <style>{`
        .print-action-bar {
          display: flex;
          align-items: center;
          justify-content: space-between;
          gap: 12px;
          padding: 12px 20px;
          background: #fff;
          border-bottom: 1px solid #dee2e6;
          box-shadow: 0 2px 4px rgba(0,0,0,0.08);
        }
        .print-action-bar a,
        .print-action-bar button {
          padding: 6px 14px;
          border-radius: 4px;
          font-size: 14px;
          cursor: pointer;
          text-decoration: none;
          display: inline-flex;
          align-items: center;
          gap: 6px;
          border: 1px solid transparent;
        }
        .print-action-bar .btn-back {
          color: #495057;
          border-color: #6c757d;
          background: #fff;
        }
        .print-action-bar .btn-print {
          color: #fff;
          background: #1a237e;
          border-color: #1a237e;
        }
        .print-action-bar .label {
          flex: 1;
          text-align: center;
          font-size: 13px;
          color: #6c757d;
        }
        @media print {
          .print-action-bar { display: none !important; }
        }
      `}</style>

      <div className="print-action-bar">
        <Link to={`/clients/${id}`} className="btn-back">
          <i className="fas fa-arrow-left"></i>Back to Client
        </Link>
        <span className="label">Client Form - Print View</span>
        <button onClick={handlePrint} className="btn-print">
          <i className="fas fa-print"></i>Print Form
        </button>
      </div>

      <div id="client-form-pdf-content">
        <ClientFormPrintView client={client} />
      </div>
    </>
  );
};

export default ClientFormPrint;
