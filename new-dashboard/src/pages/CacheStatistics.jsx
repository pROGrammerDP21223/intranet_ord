import React, { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import Loader from '../components/Loader';
import { cacheStatisticsAPI } from '../services/api';
import { toast } from 'react-toastify';
import { useRole } from '../hooks/useRole';
import { Line } from 'react-chartjs-2';

const CacheStatistics = () => {
  const role = useRole();
  const [statistics, setStatistics] = useState(null);
  const [loading, setLoading] = useState(true);
  const [history, setHistory] = useState([]);

  useEffect(() => {
    if (role.isAdmin || role.isOwner) {
      loadStatistics();
      const interval = setInterval(loadStatistics, 5000); // Refresh every 5 seconds
      return () => clearInterval(interval);
    }
  }, []);

  const loadStatistics = async () => {
    try {
      const response = await cacheStatisticsAPI.getStatistics();
      if (response.success) {
        setStatistics(response.data);
        setHistory((prev) => {
          const newHistory = [...prev, {
            time: new Date().toLocaleTimeString(),
            hitRate: response.data.hitRate,
            hits: response.data.cacheHits,
            misses: response.data.cacheMisses,
          }];
          return newHistory.slice(-20); // Keep last 20 data points
        });
      } else {
        toast.error(response.message || 'Failed to load cache statistics.');
      }
    } catch (error) {
      console.error('Error loading cache statistics:', error);
      toast.error('An error occurred while loading cache statistics.');
    } finally {
      setLoading(false);
    }
  };

  const handleClearStatistics = async () => {
    if (!window.confirm('Are you sure you want to clear cache statistics?')) {
      return;
    }

    try {
      const response = await cacheStatisticsAPI.clearStatistics();
      if (response.success) {
        toast.success('Cache statistics cleared successfully!');
        loadStatistics();
        setHistory([]);
      } else {
        toast.error(response.message || 'Failed to clear cache statistics.');
      }
    } catch (error) {
      console.error('Error clearing cache statistics:', error);
      toast.error('An error occurred while clearing cache statistics.');
    }
  };

  if (!role.isAdmin && !role.isOwner) {
    return (
      <Layout>
        <div className="alert alert-danger">You don't have permission to access this page.</div>
      </Layout>
    );
  }

  const chartData = {
    labels: history.map((h) => h.time),
    datasets: [
      {
        label: 'Hit Rate (%)',
        data: history.map((h) => h.hitRate),
        borderColor: 'rgb(75, 192, 192)',
        backgroundColor: 'rgba(75, 192, 192, 0.2)',
        tension: 0.1,
      },
    ],
  };

  return (
    <Layout>
      <div className="row">
        <div className="col-12">
          <div className="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 className="mb-sm-0">Cache Statistics</h4>
            <div className="page-title-right">
              <button className="btn btn-secondary me-2" onClick={loadStatistics}>
                <i className="fas fa-sync me-2"></i>Refresh
              </button>
              <button className="btn btn-danger" onClick={handleClearStatistics}>
                <i className="fas fa-trash me-2"></i>Clear Statistics
              </button>
            </div>
          </div>
        </div>
      </div>

      {loading ? (
        <Loader />
      ) : statistics ? (
        <>
          <div className="row">
            <div className="col-lg-3 col-md-6">
              <div className="card">
                <div className="card-body">
                  <div className="d-flex align-items-center">
                    <div className="flex-grow-1">
                      <span className="text-muted text-uppercase fs-12 fw-bold">Total Requests</span>
                      <h3 className="mb-0">{statistics.totalRequests.toLocaleString()}</h3>
                    </div>
                    <div className="flex-shrink-0 align-self-center">
                      <i className="fas fa-chart-line text-primary"></i>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div className="col-lg-3 col-md-6">
              <div className="card">
                <div className="card-body">
                  <div className="d-flex align-items-center">
                    <div className="flex-grow-1">
                      <span className="text-muted text-uppercase fs-12 fw-bold">Cache Hits</span>
                      <h3 className="mb-0 text-success">{statistics.cacheHits.toLocaleString()}</h3>
                    </div>
                    <div className="flex-shrink-0 align-self-center">
                      <i className="fas fa-check-circle text-success"></i>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div className="col-lg-3 col-md-6">
              <div className="card">
                <div className="card-body">
                  <div className="d-flex align-items-center">
                    <div className="flex-grow-1">
                      <span className="text-muted text-uppercase fs-12 fw-bold">Cache Misses</span>
                      <h3 className="mb-0 text-danger">{statistics.cacheMisses.toLocaleString()}</h3>
                    </div>
                    <div className="flex-shrink-0 align-self-center">
                      <i className="fas fa-times-circle text-danger"></i>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div className="col-lg-3 col-md-6">
              <div className="card">
                <div className="card-body">
                  <div className="d-flex align-items-center">
                    <div className="flex-grow-1">
                      <span className="text-muted text-uppercase fs-12 fw-bold">Hit Rate</span>
                      <h3 className="mb-0 text-info">{statistics.hitRate.toFixed(2)}%</h3>
                    </div>
                    <div className="flex-shrink-0 align-self-center">
                      <i className="fas fa-percentage text-info"></i>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div className="row">
            <div className="col-lg-12">
              <div className="card">
                <div className="card-header">
                  <h5 className="card-title mb-0">Hit Rate Over Time</h5>
                </div>
                <div className="card-body">
                  {history.length > 0 ? (
                    <Line data={chartData} options={{ responsive: true, maintainAspectRatio: false }} />
                  ) : (
                    <p className="text-center text-muted">No data available yet. Statistics will appear as cache is used.</p>
                  )}
                </div>
              </div>
            </div>
          </div>

          <div className="row">
            <div className="col-lg-6">
              <div className="card">
                <div className="card-header">
                  <h5 className="card-title mb-0">Cache Information</h5>
                </div>
                <div className="card-body">
                  <table className="table table-striped">
                    <tbody>
                      <tr>
                        <td><strong>Cache Type</strong></td>
                        <td>{statistics.cacheType}</td>
                      </tr>
                      <tr>
                        <td><strong>Memory Usage</strong></td>
                        <td>{(statistics.memoryUsage / 1024 / 1024).toFixed(2)} MB</td>
                      </tr>
                      <tr>
                        <td><strong>Total Keys</strong></td>
                        <td>{statistics.totalKeys}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>
        </>
      ) : (
        <div className="alert alert-info">No statistics available.</div>
      )}
    </Layout>
  );
};

export default CacheStatistics;

