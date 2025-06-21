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
import BubbleTest from './pages/BubbleTest'; // Test page

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
    <>
      <Routes>
        <Route path="/" element={<Navigate to="/login" />} />
        <Route path="/login" element={<Login />} />
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
        <Route path="/bubble-test" element={<BubbleTest />} />;
      </Routes>

      {/* ✅ Hidden div outside <Routes> to force Tailwind class inclusion */}
      <div className="hidden">
        bg-byuNavy bg-byuRoyal text-white text-black dark:text-white rounded-2xl
        rounded-br-sm rounded-bl-sm shadow
      </div>
    </>
  );
};

export default App;
