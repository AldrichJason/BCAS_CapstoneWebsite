import { Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { NewsPage } from './pages/news/NewsPage';
import { NewsAdminPage } from './pages/news/NewsAdminPage';
import { AnnouncementsPage } from './pages/announcements/AnnouncementsPage';
import { EventsPage } from './pages/events/EventsPage';
import { RoleNames } from './types/roles';

export function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<ProtectedRoute />}>
          <Route path="/dashboard" element={<DashboardPage />} />
        </Route>
        <Route element={<ProtectedRoute roles={[RoleNames.AcademicHead]} />}>
          <Route path="/news" element={<NewsPage />} />
          <Route path="/announcements" element={<AnnouncementsPage />} />
          <Route path="/events" element={<EventsPage />} />
        </Route>
        <Route element={<ProtectedRoute roles={[RoleNames.SuperAdmin]} />}>
          <Route path="/admin/news" element={<NewsAdminPage />} />
        </Route>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </AuthProvider>
  );
}
