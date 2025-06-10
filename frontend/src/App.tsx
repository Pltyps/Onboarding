import { Route, Routes, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Documents from './pages/Documents';
import DocumentView from './pages/DocumentView';
import UploadForm from './pages/UploadForm';
import Chat from './pages/Chat.tsx'; // Optional: scaffold a placeholder if not yet created
import AdminStats from './pages/AdminStats.tsx'; // Optional: same here
import Layout from './components/Layout';
import { useAuth } from './context/AuthContext';

// 🔐 Wrapper: Blocks route if not logged in
const PrivateRoute = ({ children }: { children: React.ReactNode }) => {
  const { isLoggedIn } = useAuth();
  return isLoggedIn ? <>{children}</> : <Navigate to="/login" />;
};

// 🔐 Wrapper: Admin-only
const AdminRoute = ({ children }: { children: React.ReactNode }) => {
  const { isLoggedIn, user } = useAuth();
  return isLoggedIn && user?.role === 'Admin' ? (
    <>{children}</>
  ) : (
    <Navigate to="/login" />
  );
};

// 🔐 Wrapper: Admins and Full-Time Staff
const StaffRoute = ({ children }: { children: React.ReactNode }) => {
  const { isLoggedIn, user } = useAuth();
  const allowed = user?.role === 'Admin' || user?.role === 'FullTime';
  return isLoggedIn && allowed ? <>{children}</> : <Navigate to="/login" />;
};

const App: React.FC = () => {
  return (
    <Routes>
      {/* 🔓 Public Login route */}
      <Route path="/" element={<Navigate to="/login" />} />
      <Route path="/login" element={<Login />} />

      {/* 🧠 Dashboard (all roles) */}
      <Route
        path="/dashboard"
        element={
          <PrivateRoute>
            <Layout>
              <Dashboard />
            </Layout>
          </PrivateRoute>
        }
      />

      {/* 📁 Document list and viewer */}
      <Route
        path="/documents"
        element={
          <PrivateRoute>
            <Layout>
              <Documents />
            </Layout>
          </PrivateRoute>
        }
      />
      <Route
        path="/documents/:fileName"
        element={
          <PrivateRoute>
            <Layout>
              <DocumentView />
            </Layout>
          </PrivateRoute>
        }
      />

      {/* 📤 Upload: Admin or Full-Time */}
      <Route
        path="/upload"
        element={
          <StaffRoute>
            <Layout>
              <UploadForm />
            </Layout>
          </StaffRoute>
        }
      />

      {/* 💬 Chat: All roles */}
      <Route
        path="/chat"
        element={
          <PrivateRoute>
            <Layout>
              <Chat />
            </Layout>
          </PrivateRoute>
        }
      />

      {/* 📊 Admin Stats: Admin-only */}
      <Route
        path="/admin-stats"
        element={
          <AdminRoute>
            <Layout>
              <AdminStats />
            </Layout>
          </AdminRoute>
        }
      />
    </Routes>
  );
};

export default App;
