import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export function DashboardPage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  async function handleLogout() {
    await logout();
    navigate('/login', { replace: true });
  }

  return (
    <div className="dashboard-page">
      <header className="dashboard-header">
        <div>
          <h1>Welcome, {user?.fullName}</h1>
          <p>
            Role: {user?.role}
            {user?.departmentName ? ` · ${user.departmentName}` : ''}
          </p>
        </div>
        <button onClick={handleLogout}>Log out</button>
      </header>
      <p className="dashboard-placeholder">
        Role-specific dashboard content will be built out in the following sprint tickets.
      </p>
    </div>
  );
}
