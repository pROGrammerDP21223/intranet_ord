import React from 'react';

const Loader = ({ size = 'md', color = 'primary', className = '', fullScreen = false }) => {
  const sizeClasses = {
    sm: 'spinner-border-sm',
    md: '',
    lg: 'spinner-border-lg'
  };

  const colorClass = `text-${color}`;
  const sizeClass = sizeClasses[size] || '';

  if (fullScreen) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <div className={`spinner-border ${sizeClass} ${colorClass} ${className}`} role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  return (
    <div className={`spinner-border ${sizeClass} ${colorClass} ${className}`} role="status">
      <span className="visually-hidden">Loading...</span>
    </div>
  );
};

export default Loader;

