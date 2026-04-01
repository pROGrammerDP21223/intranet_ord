import { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate, Link, useLocation } from 'react-router-dom';
import { useRole } from '../hooks/useRole';
import { useTheme } from '../contexts/ThemeContext';
import { useLanguage } from '../contexts/LanguageContext';

const Layout = ({ children }) => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const role = useRole();
  const { isDarkMode, toggleTheme } = useTheme();
  const { language, setLanguage, t, availableLanguages } = useLanguage();
  
  // Determine active tab based on current route
  const getActiveTab = (path) => {
    if (path.startsWith('/clients') || path.startsWith('/client-form') || path.startsWith('/attach-products')) {
      return 'clients';
    } else if (path.startsWith('/products') || path.startsWith('/categories') || path.startsWith('/industries')) {
      return 'products';
    } else if (path.startsWith('/users') || path.startsWith('/roles') || path.startsWith('/user-relationships')) {
      return 'users';
    } else if (path.startsWith('/services')) {
      return 'services';
    } else if (path.startsWith('/enquiries')) {
      return 'enquiries';
    } else if (path.startsWith('/tickets')) {
      return 'tickets';
    } else if (path.startsWith('/analytics')) {
      return 'analytics';
    } else if (path.startsWith('/api-keys') || path.startsWith('/email-templates') || path.startsWith('/audit-logs') || path.startsWith('/backups') || path.startsWith('/archives')) {
      return 'settings';
    } else if (path.startsWith('/messages')) {
      return 'tools';
    } else if (path.startsWith('/free-registrations') || path.startsWith('/ordpanel-enquiries') || path.startsWith('/contact-forms')) {
      return 'ordpanel';
    }
    return 'dashboard';
  };

  const [activeTab, setActiveTab] = useState(() => getActiveTab(location.pathname));

  // Update active tab when route changes
  useEffect(() => {
    setActiveTab(getActiveTab(location.pathname));
  }, [location.pathname]);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const isActivePath = (pathPrefix) => location.pathname.startsWith(pathPrefix);

  return (
    <div id="body">
      {/* leftbar-tab-menu */}
      <div className="leftbar-tab-menu">
        <div className="main-menu-inner">
          {/* LOGO */}
          <div className="topbar-left">
            <Link to="/dashboard" className="logo">
              <span>
                <img src="https://ordbusinesshub.com/images/logo.png" alt="logo-large" className="logo-lg logo-dark" />
                <img src="https://ordbusinesshub.com/images/logo.png" alt="logo-large" className="logo-lg logo-light" />
              </span>
            </Link>
          </div>

          <div className="menu-body navbar-vertical" data-simplebar>
            <ul className="nav flex-column">
              <li className="nav-item">
                <Link className={`nav-link ${isActivePath('/dashboard') ? 'active' : ''}`} to="/dashboard">
                  <i className="fas fa-home me-2"></i> Dashboard
                </Link>
              </li>

              <li className="nav-item">
                <a
                  className="nav-link d-flex justify-content-between align-items-center"
                  data-bs-toggle="collapse"
                  href="#menuClients"
                  role="button"
                  aria-expanded={activeTab === 'clients'}
                  aria-controls="menuClients"
                >
                  <span><i className="fas fa-users me-2"></i> Clients</span>
                  <i className="fas fa-chevron-down"></i>
                </a>
                <div className={`collapse ${activeTab === 'clients' ? 'show' : ''}`} id="menuClients">
                  <ul className="nav flex-column ms-3">
                    <li className="nav-item">
                      <Link className={`nav-link ${isActivePath('/clients') ? 'active' : ''}`} to="/clients">Clients List</Link>
                    </li>
                    {role.canCreateClient && (
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/client-form') ? 'active' : ''}`} to="/client-form">Add New Client</Link>
                      </li>
                    )}
                    {role.canAttachProducts && (
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/attach-products') ? 'active' : ''}`} to="/attach-products">Attach Products</Link>
                      </li>
                    )}
                  </ul>
                </div>
              </li>

              <li className="nav-item">
                <a
                  className="nav-link d-flex justify-content-between align-items-center"
                  data-bs-toggle="collapse"
                  href="#menuProducts"
                  role="button"
                  aria-expanded={activeTab === 'products'}
                  aria-controls="menuProducts"
                >
                  <span><i className="fas fa-box me-2"></i> Products</span>
                  <i className="fas fa-chevron-down"></i>
                </a>
                <div className={`collapse ${activeTab === 'products' ? 'show' : ''}`} id="menuProducts">
                  <ul className="nav flex-column ms-3">
                    <li className="nav-item">
                      <Link className={`nav-link ${isActivePath('/products') ? 'active' : ''}`} to="/products">Products</Link>
                    </li>
                    {role.canManageCategories && (
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/categories') ? 'active' : ''}`} to="/categories">Categories</Link>
                      </li>
                    )}
                    {role.canManageIndustries && (
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/industries') ? 'active' : ''}`} to="/industries">Industries</Link>
                      </li>
                    )}
                  </ul>
                </div>
              </li>

              {role.canManageUsers && (
                <li className="nav-item">
                  <a
                    className="nav-link d-flex justify-content-between align-items-center"
                    data-bs-toggle="collapse"
                    href="#menuUsers"
                    role="button"
                    aria-expanded={activeTab === 'users'}
                    aria-controls="menuUsers"
                  >
                    <span><i className="fas fa-user-cog me-2"></i> Users & Roles</span>
                    <i className="fas fa-chevron-down"></i>
                  </a>
                  <div className={`collapse ${activeTab === 'users' ? 'show' : ''}`} id="menuUsers">
                    <ul className="nav flex-column ms-3">
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/users') ? 'active' : ''}`} to="/users">Users Management</Link>
                      </li>
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/roles') ? 'active' : ''}`} to="/roles">Roles Management</Link>
                      </li>
                      {role.canManageRelationships && (
                        <li className="nav-item">
                          <Link className={`nav-link ${isActivePath('/user-relationships') ? 'active' : ''}`} to="/user-relationships">User Relationships</Link>
                        </li>
                      )}
                    </ul>
                  </div>
                </li>
              )}

              {role.canManageUsers && (
                <li className="nav-item">
                  <a
                    className="nav-link d-flex justify-content-between align-items-center"
                    data-bs-toggle="collapse"
                    href="#menuServices"
                    role="button"
                    aria-expanded={activeTab === 'services'}
                    aria-controls="menuServices"
                  >
                    <span><i className="fas fa-concierge-bell me-2"></i> Services</span>
                    <i className="fas fa-chevron-down"></i>
                  </a>
                  <div className={`collapse ${activeTab === 'services' ? 'show' : ''}`} id="menuServices">
                    <ul className="nav flex-column ms-3">
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/services') ? 'active' : ''}`} to="/services">Services Management</Link>
                      </li>
                    </ul>
                  </div>
                </li>
              )}

              {(role.isClient || role.isSalesPerson || role.isSalesManager || role.isOwner || role.isCallingStaff || role.canViewAll) && (
                <li className="nav-item">
                  <a
                    className="nav-link d-flex justify-content-between align-items-center"
                    data-bs-toggle="collapse"
                    href="#menuEnquiries"
                    role="button"
                    aria-expanded={activeTab === 'enquiries'}
                    aria-controls="menuEnquiries"
                  >
                    <span><i className="fas fa-inbox me-2"></i> Enquiries</span>
                    <i className="fas fa-chevron-down"></i>
                  </a>
                  <div className={`collapse ${activeTab === 'enquiries' ? 'show' : ''}`} id="menuEnquiries">
                    <ul className="nav flex-column ms-3">
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/enquiries') ? 'active' : ''}`} to="/enquiries">Enquiries</Link>
                      </li>
                    </ul>
                  </div>
                </li>
              )}

              <li className="nav-item">
                <a
                  className="nav-link d-flex justify-content-between align-items-center"
                  data-bs-toggle="collapse"
                  href="#menuTickets"
                  role="button"
                  aria-expanded={activeTab === 'tickets'}
                  aria-controls="menuTickets"
                >
                  <span><i className="fas fa-ticket-alt me-2"></i> Tickets</span>
                  <i className="fas fa-chevron-down"></i>
                </a>
                <div className={`collapse ${activeTab === 'tickets' ? 'show' : ''}`} id="menuTickets">
                  <ul className="nav flex-column ms-3">
                    <li className="nav-item">
                      <Link className={`nav-link ${isActivePath('/tickets') ? 'active' : ''}`} to="/tickets">All Tickets</Link>
                    </li>
                  </ul>
                </div>
              </li>

              {(role.isAdmin || role.isOwner || role.isSalesManager || role.isSalesPerson || role.isHOD || role.isCallingStaff) && (
                <li className="nav-item">
                  <a
                    className="nav-link d-flex justify-content-between align-items-center"
                    data-bs-toggle="collapse"
                    href="#menuAnalytics"
                    role="button"
                    aria-expanded={activeTab === 'analytics'}
                    aria-controls="menuAnalytics"
                  >
                    <span><i className="fas fa-chart-line me-2"></i> Analytics</span>
                    <i className="fas fa-chevron-down"></i>
                  </a>
                  <div className={`collapse ${activeTab === 'analytics' ? 'show' : ''}`} id="menuAnalytics">
                    <ul className="nav flex-column ms-3">
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/analytics') ? 'active' : ''}`} to="/analytics">Analytics</Link>
                      </li>
                    </ul>
                  </div>
                </li>
              )}

              <li className="nav-item">
                <a
                  className="nav-link d-flex justify-content-between align-items-center"
                  data-bs-toggle="collapse"
                  href="#menuTools"
                  role="button"
                  aria-expanded={activeTab === 'tools'}
                  aria-controls="menuTools"
                >
                  <span><i className="fas fa-envelope me-2"></i> Internal Messaging</span>
                  <i className="fas fa-chevron-down"></i>
                </a>
                <div className={`collapse ${activeTab === 'tools' ? 'show' : ''}`} id="menuTools">
                  <ul className="nav flex-column ms-3">
                    <li className="nav-item">
                      <Link className={`nav-link ${isActivePath('/messages') ? 'active' : ''}`} to="/messages">Internal Messaging</Link>
                    </li>
                  </ul>
                </div>
              </li>

              {(role.isAdmin || role.isOwner || role.isSalesManager) && (
                <li className="nav-item">
                  <a
                    className="nav-link d-flex justify-content-between align-items-center"
                    data-bs-toggle="collapse"
                    href="#menuOrdpanel"
                    role="button"
                    aria-expanded={activeTab === 'ordpanel'}
                    aria-controls="menuOrdpanel"
                  >
                    <span><i className="fas fa-globe me-2"></i> Ordpanel</span>
                    <i className="fas fa-chevron-down"></i>
                  </a>
                  <div className={`collapse ${activeTab === 'ordpanel' ? 'show' : ''}`} id="menuOrdpanel">
                    <ul className="nav flex-column ms-3">
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/free-registrations') ? 'active' : ''}`} to="/free-registrations">Free Registrations</Link>
                      </li>
                      {(role.isAdmin || role.isOwner) && (
                        <li className="nav-item">
                          <Link className={`nav-link ${isActivePath('/ordpanel-enquiries') ? 'active' : ''}`} to="/ordpanel-enquiries">Portal Enquiries</Link>
                        </li>
                      )}
                      {(role.isAdmin || role.isOwner) && (
                        <li className="nav-item">
                          <Link className={`nav-link ${isActivePath('/contact-forms') ? 'active' : ''}`} to="/contact-forms">Contact Forms</Link>
                        </li>
                      )}
                    </ul>
                  </div>
                </li>
              )}

              {(role.isAdmin || role.isOwner) && (
                <li className="nav-item">
                  <a
                    className="nav-link d-flex justify-content-between align-items-center"
                    data-bs-toggle="collapse"
                    href="#menuSettings"
                    role="button"
                    aria-expanded={activeTab === 'settings'}
                    aria-controls="menuSettings"
                  >
                    <span><i className="fas fa-cog me-2"></i> Settings</span>
                    <i className="fas fa-chevron-down"></i>
                  </a>
                  <div className={`collapse ${activeTab === 'settings' ? 'show' : ''}`} id="menuSettings">
                    <ul className="nav flex-column ms-3">
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/api-keys') ? 'active' : ''}`} to="/api-keys">API Keys</Link>
                      </li>
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/email-templates') ? 'active' : ''}`} to="/email-templates">Email Templates</Link>
                      </li>
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/audit-logs') ? 'active' : ''}`} to="/audit-logs">Audit Logs</Link>
                      </li>
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/backups') ? 'active' : ''}`} to="/backups">Backups</Link>
                      </li>
                      <li className="nav-item">
                        <Link className={`nav-link ${isActivePath('/archives') ? 'active' : ''}`} to="/archives">Archives</Link>
                      </li>
                    </ul>
                  </div>
                </li>
              )}
            </ul>
          </div>
        </div>
      </div>

      {/* Top Bar Start */}
      <div className="topbar">
        {/* Navbar */}
        <nav className="navbar-custom" id="navbar-custom">
          <ul className="list-unstyled topbar-nav float-end mb-0">
            <li className="dropdown notification-list">
              <a 
                className="nav-link dropdown-toggle arrow-none nav-icon" 
                data-bs-toggle="dropdown" 
                href="#" 
                role="button"
                aria-haspopup="false" 
                aria-expanded="false"
              >
                <i className="fas fa-bell"></i>
                <span className="alert-badge"></span>
              </a>
              <div className="dropdown-menu dropdown-menu-end dropdown-lg pt-0">
                <h6 className="dropdown-item-text font-15 m-0 py-3 border-bottom d-flex justify-content-between align-items-center">
                  Notifications <span className="badge bg-soft-primary badge-pill">0</span>
                </h6>
                <div className="notification-menu" data-simplebar>
                  <div className="dropdown-item py-3 text-center text-muted">
                    No notifications
                  </div>
                </div>
              </div>
            </li>

            <li className="dropdown">
              <a 
                className="nav-link dropdown-toggle nav-user" 
                data-bs-toggle="dropdown" 
                href="#" 
                role="button"
                aria-haspopup="false" 
                aria-expanded="false"
              >
                <div className="d-flex align-items-center">
                  <img 
                    src="/assets/images/users/user-4.jpg" 
                    alt="profile-user" 
                    className="rounded-circle me-2 thumb-sm"
                    onError={(e) => {
                      e.target.src = 'https://ui-avatars.com/api/?name=' + encodeURIComponent(user?.name || 'User') + '&background=random';
                    }}
                  />
                  <div>
                    <small className="d-none d-md-block font-11">{role.roleName || 'User'}</small>
                    <span className="d-none d-md-block fw-semibold font-12">
                      {user?.name || 'User'} <i className="fas fa-chevron-down"></i>
                    </span>
                  </div>
                </div>
              </a>
              <div className="dropdown-menu dropdown-menu-end">
                <a className="dropdown-item" href="#">
                  <i className="fas fa-user font-16 me-1 align-text-bottom"></i> Profile
                </a>
                <a className="dropdown-item" href="#">
                  <i className="fas fa-cog font-16 me-1 align-text-bottom"></i> Settings
                </a>
                <div className="dropdown-divider mb-0"></div>
                <a className="dropdown-item" href="#" onClick={(e) => { e.preventDefault(); toggleTheme(); }}>
                  <i className={`fas ${isDarkMode ? 'fa-sun' : 'fa-moon'} font-16 me-1 align-text-bottom`}></i> 
                  {isDarkMode ? 'Light Mode' : 'Dark Mode'}
                </a>
                <div className="dropdown-item">
                  <label className="form-label me-2">Language:</label>
                  <select 
                    className="form-select form-select-sm" 
                    value={language} 
                    onChange={(e) => setLanguage(e.target.value)}
                    onClick={(e) => e.stopPropagation()}
                  >
                    <option value="en">English</option>
                    <option value="es">Español</option>
                    <option value="fr">Français</option>
                  </select>
                </div>
                <div className="dropdown-divider mb-0"></div>
                <a className="dropdown-item" href="#" onClick={handleLogout}>
                  <i className="fas fa-sign-out-alt font-16 me-1 align-text-bottom"></i> Logout
                </a>
              </div>
            </li>
            <li className="notification-list">
              <a 
                className="nav-link arrow-none nav-icon offcanvas-btn" 
                href="#" 
                data-bs-toggle="offcanvas" 
                data-bs-target="#Appearance" 
                role="button" 
                aria-controls="Rightbar"
              >
                <i className="fas fa-cog fa-spin"></i>
              </a>
            </li>
          </ul>
          {/*end topbar-nav*/}

          <ul className="list-unstyled topbar-nav mb-0">
            <li>
              <button 
                className="nav-link button-menu-mobile nav-icon" 
                id="togglemenu"
                type="button"
                onClick={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  const body = document.body || document.getElementsByTagName('body')[0];
                  if (body) {
                    body.classList.toggle('enlarge-menu');
                  }
                }}
                style={{ cursor: 'pointer' }}
              >
                <i className="fas fa-bars"></i>
              </button>
            </li>
            <li className="hide-phone app-search">
              <form role="search" action="#" method="get">
                <input type="search" name="search" className="form-control top-search mb-0" placeholder="Type text..." />
                <button type="submit"><i className="fas fa-search"></i></button>
              </form>
            </li>
          </ul>
        </nav>
        {/* end navbar*/}
      </div>
      {/* Top Bar End */}

      {/* Main Content */}
      <div className="page-wrapper">
        {/* Page Content*/}
        <div className="page-content-tab">
          <div className="container-fluid">
            {children}
          </div>

          {/*Start Footer*/}
          {/* Footer Start */}
          <footer className="footer text-center text-sm-start">
            &copy; {new Date().getFullYear()} One Rank Digital <span className="text-muted d-none d-sm-inline-block float-end">
              Crafted with <i className="fas fa-heart text-danger"></i> by One Rank Digital
            </span>
          </footer>
          {/* end Footer */}
          {/*end footer*/}
        </div>
        {/* end page content */}
      </div>
      {/* end page-wrapper */}
    </div>
  );
};

export default Layout;

