import { Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { SuperAdminRoute } from './components/SuperAdminRoute';
import { LoginPage } from './pages/LoginPage';
import { ForgotPasswordPage } from './pages/ForgotPasswordPage';
import { ResetPasswordPage } from './pages/ResetPasswordPage';
import { DashboardPage } from './pages/DashboardPage';
import { AdminAccountsPage } from './pages/AdminAccountsPage';
import { NewsPage } from './pages/news/NewsPage';
import { NewsAdminPage } from './pages/news/NewsAdminPage';
import { AnnouncementsPage } from './pages/announcements/AnnouncementsPage';
import { AnnouncementsAdminPage } from './pages/announcements/AnnouncementsAdminPage';
import { EventsPage } from './pages/events/EventsPage';
import { EventsAdminPage } from './pages/events/EventsAdminPage';
import { RoleNames } from './types/roles';

export function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route element={<ProtectedRoute />}>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route element={<SuperAdminRoute />}>
            <Route path="/admin/accounts" element={<AdminAccountsPage />} />
          </Route>
        </Route>
        <Route element={<ProtectedRoute roles={[RoleNames.AcademicHead]} />}>
          <Route path="/news" element={<NewsPage />} />
          <Route path="/announcements" element={<AnnouncementsPage />} />
          <Route path="/events" element={<EventsPage />} />
        </Route>
        <Route element={<ProtectedRoute roles={[RoleNames.SuperAdmin]} />}>
          <Route path="/admin/news" element={<NewsAdminPage />} />
          <Route path="/admin/announcements" element={<AnnouncementsAdminPage />} />
          <Route path="/admin/events" element={<EventsAdminPage />} />
        </Route>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </AuthProvider>
  );
}
