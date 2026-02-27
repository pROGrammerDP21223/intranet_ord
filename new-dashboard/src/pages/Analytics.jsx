import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import { analyticsAPI } from '../services/api';
import { useRole } from '../hooks/useRole';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
} from 'chart.js';
import { Line, Bar, Pie, Doughnut } from 'react-chartjs-2';

// Register Chart.js components
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
);

const Analytics = () => {
  const role = useRole();
  const [analytics, setAnalytics] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [period, setPeriod] = useState('monthly');

  const canViewAnalytics = role.isAdmin || role.isOwner || role.isSalesManager || role.isSalesPerson || role.isHOD || role.isCallingStaff;

  useEffect(() => {
    if (canViewAnalytics) {
      loadAnalytics();
    }
  }, [startDate, endDate, period, canViewAnalytics]);

  const loadAnalytics = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await analyticsAPI.getAnalytics(
        startDate || null,
        endDate || null,
        period
      );
      if (response.success && response.data) {
        setAnalytics(response.data);
      } else {
        setError(response.message || 'Failed to load analytics');
      }
    } catch (err) {
      setError('An error occurred while loading analytics');
      console.error('Failed to load analytics:', err);
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(amount || 0);
  };

  const formatNumber = (num) => {
    return new Intl.NumberFormat('en-IN').format(num || 0);
  };

  const formatPercentage = (num) => {
    return `${(num || 0).toFixed(2)}%`;
  };

  if (!canViewAnalytics) {
    return (
      <Layout>
        <div className="container-fluid">
          <div className="row">
            <div className="col-12">
              <div className="alert alert-danger">You don't have permission to view analytics.</div>
            </div>
          </div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="container-fluid">
        <div className="row">
          <div className="col-12">
            <div className="page-title-box d-sm-flex align-items-center justify-content-between">
              <h4 className="mb-sm-0 font-size-18">Business Analytics</h4>
              <div className="page-title-right">
                <ol className="breadcrumb m-0">
                  <li className="breadcrumb-item"><a href="/dashboard">Dashboard</a></li>
                  <li className="breadcrumb-item active">Analytics</li>
                </ol>
              </div>
            </div>
          </div>
        </div>

        {/* Date Range Filter */}
        <div className="row mb-4">
          <div className="col-12">
            <div className="card">
              <div className="card-body">
                <h5 className="card-title mb-3">Filter Options</h5>
                <div className="row">
                  <div className="col-md-3">
                    <label className="form-label">Start Date</label>
                    <input
                      type="date"
                      className="form-control"
                      value={startDate}
                      onChange={(e) => setStartDate(e.target.value)}
                    />
                  </div>
                  <div className="col-md-3">
                    <label className="form-label">End Date</label>
                    <input
                      type="date"
                      className="form-control"
                      value={endDate}
                      onChange={(e) => setEndDate(e.target.value)}
                    />
                  </div>
                  <div className="col-md-3">
                    <label className="form-label">Period</label>
                    <select
                      className="form-select"
                      value={period}
                      onChange={(e) => setPeriod(e.target.value)}
                    >
                      <option value="daily">Daily</option>
                      <option value="monthly">Monthly</option>
                      <option value="yearly">Yearly</option>
                    </select>
                  </div>
                  <div className="col-md-3 d-flex align-items-end">
                    <button
                      className="btn btn-primary w-100"
                      onClick={loadAnalytics}
                      disabled={loading}
                    >
                      {loading ? 'Loading...' : 'Refresh'}
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        {error && (
          <div className="row">
            <div className="col-12">
              <div className="alert alert-danger">{error}</div>
            </div>
          </div>
        )}

        {loading && !analytics ? (
          <div className="row">
            <div className="col-12">
              <div className="text-center py-5">
                <div className="spinner-border text-primary" role="status">
                  <span className="visually-hidden">Loading...</span>
                </div>
              </div>
            </div>
          </div>
        ) : analytics ? (
          <>
            {/* Overview Metrics */}
            <div className="row">
              <div className="col-xl-3 col-md-6">
                <div className="card">
                  <div className="card-body">
                    <div className="d-flex align-items-center">
                      <div className="flex-grow-1">
                        <p className="text-truncate font-size-14 mb-2">Total Revenue</p>
                        <h4 className="mb-2">{formatCurrency(analytics.overview?.totalRevenue)}</h4>
                        <p className="text-muted mb-0">
                          <span className="text-success me-2">
                            <i className="mdi mdi-arrow-up"></i>
                          </span>
                          Package Value: {formatCurrency(analytics.overview?.totalPackageValue)}
                        </p>
                      </div>
                      <div className="avatar-sm">
                        <span className="avatar-title bg-primary-subtle text-primary rounded-circle font-size-18">
                          <i className="fas fa-rupee-sign"></i>
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="col-xl-3 col-md-6">
                <div className="card">
                  <div className="card-body">
                    <div className="d-flex align-items-center">
                      <div className="flex-grow-1">
                        <p className="text-truncate font-size-14 mb-2">Total Clients</p>
                        <h4 className="mb-2">{formatNumber(analytics.overview?.totalClients)}</h4>
                        <p className="text-muted mb-0">
                          <span className="text-info me-2">
                            <i className="mdi mdi-arrow-up"></i>
                          </span>
                          Avg Value: {formatCurrency(analytics.overview?.averageClientValue)}
                        </p>
                      </div>
                      <div className="avatar-sm">
                        <span className="avatar-title bg-success-subtle text-success rounded-circle font-size-18">
                          <i className="fas fa-users"></i>
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="col-xl-3 col-md-6">
                <div className="card">
                  <div className="card-body">
                    <div className="d-flex align-items-center">
                      <div className="flex-grow-1">
                        <p className="text-truncate font-size-14 mb-2">Total Transactions</p>
                        <h4 className="mb-2">{formatNumber(analytics.overview?.totalTransactions)}</h4>
                        <p className="text-muted mb-0">
                          <span className="text-warning me-2">
                            <i className="mdi mdi-arrow-up"></i>
                          </span>
                          Avg Amount: {formatCurrency(analytics.overview?.averageTransactionAmount)}
                        </p>
                      </div>
                      <div className="avatar-sm">
                        <span className="avatar-title bg-warning-subtle text-warning rounded-circle font-size-18">
                          <i className="fas fa-exchange-alt"></i>
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="col-xl-3 col-md-6">
                <div className="card">
                  <div className="card-body">
                    <div className="d-flex align-items-center">
                      <div className="flex-grow-1">
                        <p className="text-truncate font-size-14 mb-2">Revenue Growth</p>
                        <h4 className="mb-2">{formatPercentage(analytics.revenue?.revenueGrowth)}</h4>
                        <p className="text-muted mb-0">
                          <span className="text-danger me-2">
                            <i className="mdi mdi-arrow-down"></i>
                          </span>
                          Outstanding: {formatCurrency(analytics.revenue?.outstandingAmount)}
                        </p>
                      </div>
                      <div className="avatar-sm">
                        <span className="avatar-title bg-danger-subtle text-danger rounded-circle font-size-18">
                          <i className="fas fa-chart-line"></i>
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Revenue Breakdown Chart */}
            <div className="row">
              <div className="col-lg-6">
                <div className="card">
                  <div className="card-body">
                    <h4 className="card-title mb-4">Revenue Breakdown</h4>
                    <div style={{ height: '300px', position: 'relative' }}>
                      <Doughnut
                        data={{
                          labels: ['Payments', 'Outstanding', 'Refunds'],
                          datasets: [{
                            data: [
                              analytics.revenue?.totalPayments || 0,
                              analytics.revenue?.outstandingAmount || 0,
                              analytics.revenue?.totalRefunds || 0
                            ],
                            backgroundColor: [
                              'rgba(40, 167, 69, 0.8)',
                              'rgba(255, 193, 7, 0.8)',
                              'rgba(220, 53, 69, 0.8)'
                            ],
                            borderColor: [
                              'rgba(40, 167, 69, 1)',
                              'rgba(255, 193, 7, 1)',
                              'rgba(220, 53, 69, 1)'
                            ],
                            borderWidth: 2
                          }]
                        }}
                        options={{
                          responsive: true,
                          maintainAspectRatio: false,
                          plugins: {
                            legend: {
                              position: 'bottom',
                            },
                            tooltip: {
                              callbacks: {
                                label: function(context) {
                                  return context.label + ': ' + formatCurrency(context.parsed);
                                }
                              }
                            }
                          }
                        }}
                      />
                    </div>
                    <div className="mt-3">
                      <div className="d-flex justify-content-between mb-2">
                        <span>Total Revenue:</span>
                        <strong>{formatCurrency(analytics.revenue?.totalRevenue)}</strong>
                      </div>
                      <div className="d-flex justify-content-between mb-2">
                        <span>Revenue Growth:</span>
                        <strong className={analytics.revenue?.revenueGrowth >= 0 ? 'text-success' : 'text-danger'}>
                          {formatPercentage(analytics.revenue?.revenueGrowth)}
                        </strong>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="col-lg-6">
                <div className="card">
                  <div className="card-body">
                    <h4 className="card-title mb-4">Client Growth</h4>
                    <div style={{ height: '300px', position: 'relative' }}>
                      <Bar
                        data={{
                          labels: ['Total Clients', 'New This Month', 'New This Year'],
                          datasets: [{
                            label: 'Client Count',
                            data: [
                              analytics.clients?.totalClients || 0,
                              analytics.clients?.newClientsThisMonth || 0,
                              analytics.clients?.newClientsThisYear || 0
                            ],
                            backgroundColor: [
                              'rgba(54, 162, 235, 0.8)',
                              'rgba(40, 167, 69, 0.8)',
                              'rgba(23, 162, 184, 0.8)'
                            ],
                            borderColor: [
                              'rgba(54, 162, 235, 1)',
                              'rgba(40, 167, 69, 1)',
                              'rgba(23, 162, 184, 1)'
                            ],
                            borderWidth: 2
                          }]
                        }}
                        options={{
                          responsive: true,
                          maintainAspectRatio: false,
                          plugins: {
                            legend: {
                              display: false
                            },
                            tooltip: {
                              callbacks: {
                                label: function(context) {
                                  return 'Clients: ' + formatNumber(context.parsed.y);
                                }
                              }
                            }
                          },
                          scales: {
                            y: {
                              beginAtZero: true,
                              ticks: {
                                callback: function(value) {
                                  return formatNumber(value);
                                }
                              }
                            }
                          }
                        }}
                      />
                    </div>
                    <div className="mt-3">
                      <div className="d-flex justify-content-between mb-2">
                        <span>Average Client Value:</span>
                        <strong>{formatCurrency(analytics.clients?.averageClientValue)}</strong>
                      </div>
                      <div className="d-flex justify-content-between">
                        <span>Client Growth Rate:</span>
                        <strong className={analytics.clients?.clientGrowthRate >= 0 ? 'text-success' : 'text-danger'}>
                          {formatPercentage(analytics.clients?.clientGrowthRate)}
                        </strong>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Transaction Metrics Charts */}
            <div className="row">
              <div className="col-lg-6">
                <div className="card">
                  <div className="card-body">
                    <h4 className="card-title mb-4">Transactions by Type</h4>
                    <div style={{ height: '300px', position: 'relative' }}>
                      <Pie
                        data={{
                          labels: analytics.transactionsByType?.map(item => item.transactionType) || [],
                          datasets: [{
                            data: analytics.transactionsByType?.map(item => item.totalAmount) || [],
                            backgroundColor: [
                              'rgba(54, 162, 235, 0.8)',
                              'rgba(255, 99, 132, 0.8)',
                              'rgba(255, 206, 86, 0.8)',
                              'rgba(75, 192, 192, 0.8)',
                              'rgba(153, 102, 255, 0.8)',
                              'rgba(255, 159, 64, 0.8)'
                            ],
                            borderColor: [
                              'rgba(54, 162, 235, 1)',
                              'rgba(255, 99, 132, 1)',
                              'rgba(255, 206, 86, 1)',
                              'rgba(75, 192, 192, 1)',
                              'rgba(153, 102, 255, 1)',
                              'rgba(255, 159, 64, 1)'
                            ],
                            borderWidth: 2
                          }]
                        }}
                        options={{
                          responsive: true,
                          maintainAspectRatio: false,
                          plugins: {
                            legend: {
                              position: 'bottom',
                            },
                            tooltip: {
                              callbacks: {
                                label: function(context) {
                                  const item = analytics.transactionsByType[context.dataIndex];
                                  return [
                                    context.label + ': ' + formatCurrency(context.parsed),
                                    'Count: ' + formatNumber(item.count),
                                    'Percentage: ' + formatPercentage(item.percentage)
                                  ];
                                }
                              }
                            }
                          }
                        }}
                      />
                    </div>
                  </div>
                </div>
              </div>
              <div className="col-lg-6">
                <div className="card">
                  <div className="card-body">
                    <h4 className="card-title mb-4">Transactions by Payment Method</h4>
                    <div style={{ height: '300px', position: 'relative' }}>
                      <Pie
                        data={{
                          labels: analytics.transactionsByPaymentMethod?.map(item => item.paymentMethod || 'Unknown') || [],
                          datasets: [{
                            data: analytics.transactionsByPaymentMethod?.map(item => item.totalAmount) || [],
                            backgroundColor: [
                              'rgba(40, 167, 69, 0.8)',
                              'rgba(0, 123, 255, 0.8)',
                              'rgba(255, 193, 7, 0.8)',
                              'rgba(220, 53, 69, 0.8)',
                              'rgba(108, 117, 125, 0.8)',
                              'rgba(23, 162, 184, 0.8)'
                            ],
                            borderColor: [
                              'rgba(40, 167, 69, 1)',
                              'rgba(0, 123, 255, 1)',
                              'rgba(255, 193, 7, 1)',
                              'rgba(220, 53, 69, 1)',
                              'rgba(108, 117, 125, 1)',
                              'rgba(23, 162, 184, 1)'
                            ],
                            borderWidth: 2
                          }]
                        }}
                        options={{
                          responsive: true,
                          maintainAspectRatio: false,
                          plugins: {
                            legend: {
                              position: 'bottom',
                            },
                            tooltip: {
                              callbacks: {
                                label: function(context) {
                                  const item = analytics.transactionsByPaymentMethod[context.dataIndex];
                                  return [
                                    context.label + ': ' + formatCurrency(context.parsed),
                                    'Count: ' + formatNumber(item.count),
                                    'Percentage: ' + formatPercentage(item.percentage)
                                  ];
                                }
                              }
                            }
                          }
                        }}
                      />
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Revenue by Period Chart */}
            <div className="row">
              <div className="col-12">
                <div className="card">
                  <div className="card-body">
                    <h4 className="card-title mb-4">Revenue Trend by Period</h4>
                    <div style={{ height: '400px', position: 'relative' }}>
                      <Line
                        data={{
                          labels: analytics.revenueByPeriod?.map(item => item.period) || [],
                          datasets: [
                            {
                              label: 'Revenue',
                              data: analytics.revenueByPeriod?.map(item => item.revenue) || [],
                              borderColor: 'rgba(54, 162, 235, 1)',
                              backgroundColor: 'rgba(54, 162, 235, 0.1)',
                              fill: true,
                              tension: 0.4,
                              yAxisID: 'y'
                            },
                            {
                              label: 'Transactions',
                              data: analytics.revenueByPeriod?.map(item => item.transactionCount) || [],
                              borderColor: 'rgba(255, 99, 132, 1)',
                              backgroundColor: 'rgba(255, 99, 132, 0.1)',
                              fill: true,
                              tension: 0.4,
                              yAxisID: 'y1'
                            }
                          ]
                        }}
                        options={{
                          responsive: true,
                          maintainAspectRatio: false,
                          interaction: {
                            mode: 'index',
                            intersect: false,
                          },
                          plugins: {
                            legend: {
                              position: 'top',
                            },
                            tooltip: {
                              callbacks: {
                                label: function(context) {
                                  if (context.datasetIndex === 0) {
                                    return 'Revenue: ' + formatCurrency(context.parsed.y);
                                  } else {
                                    return 'Transactions: ' + formatNumber(context.parsed.y);
                                  }
                                }
                              }
                            }
                          },
                          scales: {
                            y: {
                              type: 'linear',
                              display: true,
                              position: 'left',
                              title: {
                                display: true,
                                text: 'Revenue (₹)'
                              },
                              ticks: {
                                callback: function(value) {
                                  return formatCurrency(value);
                                }
                              }
                            },
                            y1: {
                              type: 'linear',
                              display: true,
                              position: 'right',
                              title: {
                                display: true,
                                text: 'Transaction Count'
                              },
                              grid: {
                                drawOnChartArea: false,
                              },
                              ticks: {
                                callback: function(value) {
                                  return formatNumber(value);
                                }
                              }
                            }
                          }
                        }}
                      />
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Top Clients Chart */}
            <div className="row">
              <div className="col-12">
                <div className="card">
                  <div className="card-body">
                    <h4 className="card-title mb-4">Top Clients by Revenue</h4>
                    <div style={{ height: '400px', position: 'relative' }}>
                      <Bar
                        data={{
                          labels: analytics.topClients?.slice(0, 10).map(client => client.clientName) || [],
                          datasets: [{
                            label: 'Total Revenue',
                            data: analytics.topClients?.slice(0, 10).map(client => client.totalRevenue) || [],
                            backgroundColor: 'rgba(40, 167, 69, 0.8)',
                            borderColor: 'rgba(40, 167, 69, 1)',
                            borderWidth: 2
                          }]
                        }}
                        options={{
                          indexAxis: 'y',
                          responsive: true,
                          maintainAspectRatio: false,
                          plugins: {
                            legend: {
                              display: false
                            },
                            tooltip: {
                              callbacks: {
                                label: function(context) {
                                  const client = analytics.topClients[context.dataIndex];
                                  return [
                                    'Revenue: ' + formatCurrency(context.parsed.x),
                                    'Package Value: ' + formatCurrency(client.packageValue),
                                    'Transactions: ' + formatNumber(client.transactionCount)
                                  ];
                                }
                              }
                            }
                          },
                          scales: {
                            x: {
                              beginAtZero: true,
                              ticks: {
                                callback: function(value) {
                                  return formatCurrency(value);
                                }
                              }
                            }
                          }
                        }}
                      />
                    </div>
                    <div className="mt-3">
                      <div className="table-responsive">
                        <table className="table table-sm table-hover mb-0">
                          <thead>
                            <tr>
                              <th>#</th>
                              <th>Client Name</th>
                              <th>Customer No</th>
                              <th className="text-end">Package Value</th>
                              <th className="text-end">Total Revenue</th>
                              <th className="text-end">Transactions</th>
                            </tr>
                          </thead>
                          <tbody>
                            {analytics.topClients?.slice(0, 5).map((client, index) => (
                              <tr key={index}>
                                <td>{index + 1}</td>
                                <td><strong>{client.clientName}</strong></td>
                                <td>{client.customerNo}</td>
                                <td className="text-end">{formatCurrency(client.packageValue)}</td>
                                <td className="text-end text-success">{formatCurrency(client.totalRevenue)}</td>
                                <td className="text-end">{formatNumber(client.transactionCount)}</td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </>
        ) : null}
      </div>
    </Layout>
  );
};

export default Analytics;

