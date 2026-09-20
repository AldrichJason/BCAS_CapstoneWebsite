import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { RoleNames } from '../types/roles';

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
      {user?.role === RoleNames.SuperAdmin && (
        <nav className="mb-6 flex flex-wrap gap-4">
          <Link
            to="/admin/accounts"
            className="font-semibold text-primary hover:text-accent-dark hover:underline"
          >
            Manage admin accounts
          </Link>
          <Link
            to="/admin/news"
            className="font-semibold text-primary hover:text-accent-dark hover:underline"
          >
            School News
          </Link>
          <Link
            to="/admin/announcements"
            className="font-semibold text-primary hover:text-accent-dark hover:underline"
          >
            School Announcements
          </Link>
          <Link
            to="/admin/events"
            className="font-semibold text-primary hover:text-accent-dark hover:underline"
          >
            School Events
          </Link>
        </nav>
      )}

      {user?.role === RoleNames.AcademicHead && (
        <nav className="mb-6 flex flex-wrap gap-4">
          <Link
            to="/news"
            className="font-semibold text-primary hover:text-accent-dark hover:underline"
          >
            Department News
          </Link>
          <Link
            to="/announcements"
            className="font-semibold text-primary hover:text-accent-dark hover:underline"
          >
            Announcements
          </Link>
          <Link
            to="/events"
            className="font-semibold text-primary hover:text-accent-dark hover:underline"
          >
            Events
          </Link>
        </nav>
      )}

      <p className="text-neutral-500">
        Role-specific dashboard content will be built out in the following sprint tickets.
      </p>
    </div>
  );
}
