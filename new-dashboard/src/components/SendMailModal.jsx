import { useState, useEffect } from 'react';
import { clientAPI } from '../services/api';
import { showToast } from '../utils/toast';

const SendMailModal = ({ client, onClose }) => {
  const [emailInfo, setEmailInfo] = useState(null);
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);

  // selected is a Set of email strings; extra is an array of custom emails
  const [selected, setSelected] = useState(new Set());
  const [extraEmail, setExtraEmail] = useState('');

  useEffect(() => {
    loadEmailInfo();
  }, [client.id]);

  const loadEmailInfo = async () => {
    try {
      const res = await clientAPI.getEmailInfo(client.id);
      if (res.success && res.data) {
        const info = res.data;
        setEmailInfo(info);
        // Pre-select both emails if present
        const pre = new Set();
        if (info.clientEmail) pre.add(info.clientEmail);
        if (info.executiveEmail) pre.add(info.executiveEmail);
        setSelected(pre);
      }
    } catch {
      showToast.error('Failed to load email info');
    } finally {
      setLoading(false);
    }
  };

  const toggle = (email) => {
    setSelected(prev => {
      const next = new Set(prev);
      next.has(email) ? next.delete(email) : next.add(email);
      return next;
    });
  };

  const addExtra = () => {
    const e = extraEmail.trim();
    if (!e) return;
    if (!/\S+@\S+\.\S+/.test(e)) { showToast.error('Invalid email address'); return; }
    setSelected(prev => new Set(prev).add(e));
    setExtraEmail('');
  };

  const handleSend = async () => {
    const emails = Array.from(selected).filter(Boolean);
    if (!emails.length) { showToast.error('Select at least one recipient'); return; }
    try {
      setSending(true);
      const res = await clientAPI.sendFormEmail(client.id, emails);
      if (res.success) {
        showToast.success(res.message || 'Email sent successfully');
        onClose();
      } else {
        showToast.error(res.message || 'Failed to send email');
      }
    } catch {
      showToast.error('Failed to send email');
    } finally {
      setSending(false);
    }
  };

  return (
    <div
      style={{
        position: 'fixed', inset: 0, background: 'rgba(0,0,0,.45)',
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        zIndex: 1055
      }}
      onClick={(e) => e.target === e.currentTarget && onClose()}
    >
      <div style={{
        background: '#fff', borderRadius: 8, width: '100%', maxWidth: 480,
        boxShadow: '0 8px 32px rgba(0,0,0,.18)', overflow: 'hidden'
      }}>
        {/* Header */}
        <div style={{ background: '#1a237e', padding: '16px 20px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            <div style={{ color: '#fff', fontWeight: 700, fontSize: 15 }}>Send Client Form</div>
            <div style={{ color: '#a5b4fc', fontSize: 12, marginTop: 2 }}>{client.companyName}</div>
          </div>
          <button onClick={onClose} style={{ background: 'none', border: 'none', color: '#fff', fontSize: 20, cursor: 'pointer', lineHeight: 1 }}>×</button>
        </div>

        {/* Body */}
        <div style={{ padding: '20px 24px' }}>
          {loading ? (
            <div style={{ textAlign: 'center', padding: '24px 0', color: '#888' }}>
              <div className="spinner-border spinner-border-sm me-2" role="status"></div>
              Loading recipients…
            </div>
          ) : (
            <>
              <p style={{ fontSize: 13, color: '#555', marginBottom: 14 }}>
                Select recipients to receive the client order form via email.
              </p>

              {/* Client email */}
              {emailInfo?.clientEmail && (
                <RecipientRow
                  email={emailInfo.clientEmail}
                  label="Client"
                  sublabel={client.contactPerson}
                  checked={selected.has(emailInfo.clientEmail)}
                  onToggle={() => toggle(emailInfo.clientEmail)}
                />
              )}

              {/* Executive email */}
              {emailInfo?.executiveEmail && (
                <RecipientRow
                  email={emailInfo.executiveEmail}
                  label="Executive"
                  sublabel={emailInfo.executiveName}
                  checked={selected.has(emailInfo.executiveEmail)}
                  onToggle={() => toggle(emailInfo.executiveEmail)}
                />
              )}

              {/* Extra selected emails (from manual add) */}
              {Array.from(selected)
                .filter(e => e !== emailInfo?.clientEmail && e !== emailInfo?.executiveEmail)
                .map(e => (
                  <RecipientRow
                    key={e}
                    email={e}
                    label="Additional"
                    checked={true}
                    onToggle={() => toggle(e)}
                  />
                ))
              }

              {/* Add custom email */}
              <div style={{ display: 'flex', gap: 8, marginTop: 14 }}>
                <input
                  type="email"
                  value={extraEmail}
                  onChange={e => setExtraEmail(e.target.value)}
                  onKeyDown={e => e.key === 'Enter' && addExtra()}
                  placeholder="Add another email address…"
                  style={{
                    flex: 1, border: '1px solid #d1d5db', borderRadius: 6,
                    padding: '7px 10px', fontSize: 13, outline: 'none'
                  }}
                />
                <button
                  onClick={addExtra}
                  style={{
                    padding: '7px 14px', background: '#e8eaf6', color: '#1a237e',
                    border: '1px solid #c5cae9', borderRadius: 6, fontSize: 13,
                    cursor: 'pointer', fontWeight: 600
                  }}
                >
                  Add
                </button>
              </div>
            </>
          )}
        </div>

        {/* Footer */}
        <div style={{ padding: '12px 24px 20px', display: 'flex', justifyContent: 'flex-end', gap: 10 }}>
          <button onClick={onClose} style={{
            padding: '8px 20px', border: '1px solid #d1d5db', borderRadius: 6,
            background: '#fff', color: '#555', cursor: 'pointer', fontSize: 14
          }}>
            Cancel
          </button>
          <button
            onClick={handleSend}
            disabled={sending || loading || selected.size === 0}
            style={{
              padding: '8px 24px', border: 'none', borderRadius: 6,
              background: selected.size === 0 ? '#9fa8da' : '#1a237e',
              color: '#fff', cursor: selected.size === 0 ? 'not-allowed' : 'pointer',
              fontSize: 14, fontWeight: 600, display: 'flex', alignItems: 'center', gap: 8
            }}
          >
            {sending && <span className="spinner-border spinner-border-sm" role="status"></span>}
            {sending ? 'Sending…' : `Send to ${selected.size} recipient${selected.size !== 1 ? 's' : ''}`}
          </button>
        </div>
      </div>
    </div>
  );
};

const RecipientRow = ({ email, label, sublabel, checked, onToggle }) => (
  <label style={{
    display: 'flex', alignItems: 'center', gap: 12, padding: '10px 12px',
    borderRadius: 6, border: `1px solid ${checked ? '#c5cae9' : '#e5e7eb'}`,
    background: checked ? '#f0f4ff' : '#fafafa',
    cursor: 'pointer', marginBottom: 8, userSelect: 'none'
  }}>
    <input type="checkbox" checked={checked} onChange={onToggle} style={{ width: 16, height: 16, accentColor: '#1a237e' }} />
    <div style={{ flex: 1, minWidth: 0 }}>
      <div style={{ fontSize: 13, fontWeight: 600, color: '#1a237e' }}>{label}</div>
      {sublabel && <div style={{ fontSize: 12, color: '#666' }}>{sublabel}</div>}
      <div style={{ fontSize: 12, color: '#444', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{email}</div>
    </div>
  </label>
);

export default SendMailModal;
