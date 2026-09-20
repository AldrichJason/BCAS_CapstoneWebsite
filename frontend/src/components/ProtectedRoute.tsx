import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface ProtectedRouteProps {
  roles?: string[];
}

export function ProtectedRoute({ roles }: ProtectedRouteProps) {
  const { user, isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <div className="page-loading">Loading...</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (roles && roles.length > 0 && (!user || !roles.includes(user.role))) {
    return <Navigate to="/dashboard" replace />;
  }

  return <Outlet />;
}
