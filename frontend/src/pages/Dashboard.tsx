import Tile from '../components/Tile';
import { useAuth } from '../context/AuthContext';

const Dashboard = () => {
  const { user } = useAuth();
  const role = user?.role;
  const department = user?.department;

  const isAdmin = role === 'Admin';
  const isFullTime = role === 'FullTime';
  const isStudent = role === 'Student';

  return (
    <div className="p-4 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      {/* Document Library: all roles */}
      <Tile
        title="Document Library"
        description="Browse and view documents in your department."
        to="/documents"
        visible={!!user}
      />

      {/* Upload: Full-Time + Admin */}
      <Tile
        title="Upload Document"
        description="Upload or update department documents."
        to="/upload"
        visible={isFullTime || isAdmin}
      />

      {/* Chat: all roles */}
      <Tile
        title="Chat Assistant"
        description="Ask the AI about your department's documents."
        to="/chat"
        visible={!!user}
      />

      {/* Admin Stats: Admin only */}
      <Tile
        title="System Stats"
        description="Monitor user activity and system health."
        to="/admin-stats"
        visible={isAdmin}
      />
    </div>
  );
};

export default Dashboard;
