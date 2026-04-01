import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { LoadingProvider } from './contexts/LoadingContext';
import { NotificationProvider } from './contexts/NotificationContext';
import { ThemeProvider } from './contexts/ThemeContext';
import { LanguageProvider } from './contexts/LanguageContext';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import ForgotPassword from './pages/ForgotPassword';
import ResetPassword from './pages/ResetPassword';
import ClientForm from './pages/ClientForm';
import ClientsList from './pages/ClientsList';
import ClientDetail from './pages/ClientDetail';
import ClientFormPrint from './pages/ClientFormPrint';
import EditClient from './pages/EditClient';
import ServicesManagement from './pages/ServicesManagement';
import UsersManagement from './pages/UsersManagement';
import RolesManagement from './pages/RolesManagement';
import IndustriesManagement from './pages/IndustriesManagement';
import CategoriesManagement from './pages/CategoriesManagement';
import ProductsManagement from './pages/ProductsManagement';
import AttachProductsToClient from './pages/AttachProductsToClient';
import ManageUserRelationships from './pages/ManageUserRelationships';
import EnquiriesManagement from './pages/EnquiriesManagement';
import Analytics from './pages/Analytics';
import APIKeysManagement from './pages/APIKeysManagement';
import TicketsManagement from './pages/TicketsManagement';
import EmailTemplatesManagement from './pages/EmailTemplatesManagement';
import AuditLogs from './pages/AuditLogs';
import DocumentsManagement from './pages/DocumentsManagement';
import InternalMessaging from './pages/InternalMessaging';
import BackupManagement from './pages/BackupManagement';
import ArchiveManagement from './pages/ArchiveManagement';
import ThemeCustomization from './pages/ThemeCustomization';
import CacheStatistics from './pages/CacheStatistics';
import FreeRegistrationsManagement from './pages/FreeRegistrationsManagement';
import OrdpanelEnquiriesManagement from './pages/OrdpanelEnquiriesManagement';
import ContactFormsManagement from './pages/ContactFormsManagement';
import ProtectedRoute from './components/ProtectedRoute';
import './App.css';

const AppRoutes = () => {
  const { isAuthenticated } = useAuth();

  return (
    <Routes>
      <Route
        path="/login"
        element={isAuthenticated ? <Navigate to="/dashboard" replace /> : <Login />}
      />
      <Route
        path="/register"
        element={isAuthenticated ? <Navigate to="/dashboard" replace /> : <Register />}
      />
      <Route
        path="/forgot-password"
        element={isAuthenticated ? <Navigate to="/dashboard" replace /> : <ForgotPassword />}
      />
      <Route
        path="/reset-password"
        element={isAuthenticated ? <Navigate to="/dashboard" replace /> : <ResetPassword />}
      />
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <Dashboard />
          </ProtectedRoute>
        }
      />
      <Route
        path="/client-form"
        element={
          <ProtectedRoute>
            <ClientForm />
          </ProtectedRoute>
        }
      />
      <Route
        path="/services"
        element={
          <ProtectedRoute>
            <ServicesManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/clients"
        element={
          <ProtectedRoute>
            <ClientsList />
          </ProtectedRoute>
        }
      />
      <Route
        path="/clients/:id/print"
        element={
          <ProtectedRoute>
            <ClientFormPrint />
          </ProtectedRoute>
        }
      />
      <Route
        path="/clients/:id/edit"
        element={
          <ProtectedRoute>
            <EditClient />
          </ProtectedRoute>
        }
      />
      <Route
        path="/clients/:id"
        element={
          <ProtectedRoute>
            <ClientDetail />
          </ProtectedRoute>
        }
      />
      <Route
        path="/users"
        element={
          <ProtectedRoute>
            <UsersManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/roles"
        element={
          <ProtectedRoute>
            <RolesManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/industries"
        element={
          <ProtectedRoute>
            <IndustriesManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/categories"
        element={
          <ProtectedRoute>
            <CategoriesManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/products"
        element={
          <ProtectedRoute>
            <ProductsManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/attach-products"
        element={
          <ProtectedRoute>
            <AttachProductsToClient />
          </ProtectedRoute>
        }
      />
      <Route
        path="/user-relationships"
        element={
          <ProtectedRoute>
            <ManageUserRelationships />
          </ProtectedRoute>
        }
      />
      <Route
        path="/enquiries"
        element={
          <ProtectedRoute>
            <EnquiriesManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/analytics"
        element={
          <ProtectedRoute>
            <Analytics />
          </ProtectedRoute>
        }
      />
      <Route
        path="/api-keys"
        element={
          <ProtectedRoute>
            <APIKeysManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/tickets"
        element={
          <ProtectedRoute>
            <TicketsManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/email-templates"
        element={
          <ProtectedRoute>
            <EmailTemplatesManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/audit-logs"
        element={
          <ProtectedRoute>
            <AuditLogs />
          </ProtectedRoute>
        }
      />
      <Route
        path="/documents/:entityType/:entityId"
        element={
          <ProtectedRoute>
            <DocumentsManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/messages"
        element={
          <ProtectedRoute>
            <InternalMessaging />
          </ProtectedRoute>
        }
      />
      <Route
        path="/theme"
        element={
          <ProtectedRoute>
            <ThemeCustomization />
          </ProtectedRoute>
        }
      />
      <Route
        path="/cache-statistics"
        element={
          <ProtectedRoute>
            <CacheStatistics />
          </ProtectedRoute>
        }
      />
      <Route
        path="/backups"
        element={
          <ProtectedRoute>
            <BackupManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/archives"
        element={
          <ProtectedRoute>
            <ArchiveManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/free-registrations"
        element={
          <ProtectedRoute>
            <FreeRegistrationsManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/ordpanel-enquiries"
        element={
          <ProtectedRoute>
            <OrdpanelEnquiriesManagement />
          </ProtectedRoute>
        }
      />
      <Route
        path="/contact-forms"
        element={
          <ProtectedRoute>
            <ContactFormsManagement />
          </ProtectedRoute>
        }
      />
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
};

function App() {
  return (
    <ThemeProvider>
      <LanguageProvider>
        <AuthProvider>
          <LoadingProvider>
            <NotificationProvider>
              <Router>
                <AppRoutes />
                <ToastContainer
                  position="top-right"
                  autoClose={3000}
                  hideProgressBar={false}
                  newestOnTop={false}
                  closeOnClick
                  rtl={false}
                  pauseOnFocusLoss
                  draggable
                  pauseOnHover
                  theme="light"
                />
              </Router>
            </NotificationProvider>
          </LoadingProvider>
        </AuthProvider>
      </LanguageProvider>
    </ThemeProvider>
  );
}

export default App;
