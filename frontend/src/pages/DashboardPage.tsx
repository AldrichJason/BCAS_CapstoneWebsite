import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export function DashboardPage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  async function handleLogout() {
    await logout();
    navigate('/login', { replace: true });
  }

  return (
    <div className="mx-auto max-w-4xl px-6 py-8">
      <header className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-primary">
            Welcome, {user?.firstName} {user?.lastName}
          </h1>
          <p className="text-neutral-600">
            Role: {user?.role}
            {user?.departmentName ? ` · ${user.departmentName}` : ''}
          </p>
        </div>
        <button
          onClick={handleLogout}
          className="rounded-lg border border-primary bg-white px-4 py-2 font-semibold text-primary transition-colors hover:bg-primary-tint"
        >
          Log out
        </button>
      </header>
      {user?.role === 'SuperAdmin' && (
        <p>
          <Link
            to="/admin/accounts"
            className="font-semibold text-primary hover:text-accent-dark hover:underline"
          >
            Manage admin accounts
          </Link>
        </p>
      )}
      <p className="text-neutral-500">
        Role-specific dashboard content will be built out in the following sprint tickets.
      </p>
    </div>
  );
}
