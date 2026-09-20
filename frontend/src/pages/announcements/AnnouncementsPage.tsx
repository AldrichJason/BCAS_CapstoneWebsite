import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import * as announcementsApi from '../../api/announcementsApi';
import { CONTENT_STATUSES } from '../../types/news';
import type { ContentStatusValue } from '../../types/news';
import type { AnnouncementDto, AnnouncementRequest } from '../../types/announcement';
import type { ErrorResponseDto } from '../../types/auth';

function isAxiosErrorResponse(error: unknown): error is { response?: { data?: ErrorResponseDto } } {
  return typeof error === 'object' && error !== null && 'response' in error;
}

function errorMessage(error: unknown, fallback: string): string {
  return (isAxiosErrorResponse(error) ? error.response?.data?.message : undefined) ?? fallback;
}

function toDateTimeLocal(isoUtc: string | null): string {
  if (!isoUtc) return '';
  const date = new Date(isoUtc);
  const offset = date.getTimezoneOffset();
  const local = new Date(date.getTime() - offset * 60 * 1000);
  return local.toISOString().slice(0, 16);
}

const emptyForm: AnnouncementRequest = {
  title: '',
  body: '',
  effectiveDateUtc: '',
  status: 'Draft',
};

export function AnnouncementsPage() {
  const [items, setItems] = useState<AnnouncementDto[]>([]);
  const [statusFilter, setStatusFilter] = useState<ContentStatusValue | ''>('');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');
  const [isLoading, setIsLoading] = useState(true);
  const [listError, setListError] = useState<string | null>(null);

  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<AnnouncementRequest>(emptyForm);
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isFormOpen, setIsFormOpen] = useState(false);

  async function loadAnnouncements() {
    setIsLoading(true);
    setListError(null);
    try {
      const data = await announcementsApi.listAnnouncements(statusFilter || undefined, sortOrder);
      setItems(data);
    } catch (error) {
      setListError(errorMessage(error, 'Failed to load announcements.'));
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadAnnouncements();
    // eslint-disable-next-line react-hooks/exhaustive-deps -- reload only when filters change
  }, [statusFilter, sortOrder]);

  function openCreateForm() {
    setEditingId(null);
    setForm(emptyForm);
    setFormError(null);
    setIsFormOpen(true);
  }

  function openEditForm(item: AnnouncementDto) {
    setEditingId(item.id);
    setForm({
      title: item.title,
      body: item.body,
      effectiveDateUtc: toDateTimeLocal(item.effectiveDateUtc),
      status: item.status,
    });
    setFormError(null);
    setIsFormOpen(true);
  }

  function closeForm() {
    setIsFormOpen(false);
    setEditingId(null);
    setForm(emptyForm);
    setFormError(null);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFormError(null);

    if (!form.title.trim() || !form.body.trim() || !form.effectiveDateUtc) {
      setFormError('Title, body and effective date are required.');
      return;
    }

    const payload: AnnouncementRequest = {
      ...form,
      effectiveDateUtc: new Date(form.effectiveDateUtc).toISOString(),
    };

    setIsSubmitting(true);
    try {
      if (editingId === null) {
        await announcementsApi.createAnnouncement(payload);
      } else {
        await announcementsApi.updateAnnouncement(editingId, payload);
      }
      closeForm();
      await loadAnnouncements();
    } catch (error) {
      setFormError(errorMessage(error, 'Failed to save the announcement.'));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDelete(item: AnnouncementDto) {
    if (!window.confirm(`Delete "${item.title}"? This cannot be undone.`)) {
      return;
    }
    try {
      await announcementsApi.deleteAnnouncement(item.id);
      await loadAnnouncements();
    } catch (error) {
      setListError(errorMessage(error, 'Failed to delete the announcement.'));
    }
  }

  return (
    <div className="content-page">
      <div className="content-page-header">
        <h2>Department Announcements</h2>
        <button onClick={openCreateForm}>+ New Announcement</button>
      </div>

      <div className="content-filters">
        <label htmlFor="announcement-status-filter">Status</label>
        <select
          id="announcement-status-filter"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value as ContentStatusValue | '')}
        >
          <option value="">All</option>
          {CONTENT_STATUSES.map((status) => (
            <option key={status} value={status}>
              {status}
            </option>
          ))}
        </select>

        <label htmlFor="announcement-sort">Sort by date</label>
        <select
          id="announcement-sort"
          value={sortOrder}
          onChange={(e) => setSortOrder(e.target.value as 'asc' | 'desc')}
        >
          <option value="desc">Newest first</option>
          <option value="asc">Oldest first</option>
        </select>
      </div>

      {listError && <div className="form-error" role="alert">{listError}</div>}

      {isFormOpen && (
        <form className="content-form" onSubmit={handleSubmit}>
          <h3>{editingId === null ? 'New Announcement' : 'Edit Announcement'}</h3>

          {formError && <div className="form-error" role="alert">{formError}</div>}

          <label htmlFor="announcement-title">Title</label>
          <input
            id="announcement-title"
            value={form.title}
            onChange={(e) => setForm({ ...form, title: e.target.value })}
            disabled={isSubmitting}
          />

          <label htmlFor="announcement-body">Body</label>
          <textarea
            id="announcement-body"
            rows={6}
            value={form.body}
            onChange={(e) => setForm({ ...form, body: e.target.value })}
            disabled={isSubmitting}
          />

          <label htmlFor="announcement-effective-date">Effective date</label>
          <input
            id="announcement-effective-date"
            type="datetime-local"
            value={form.effectiveDateUtc}
            onChange={(e) => setForm({ ...form, effectiveDateUtc: e.target.value })}
            disabled={isSubmitting}
          />

          <label htmlFor="announcement-status">Status</label>
          <select
            id="announcement-status"
            value={form.status}
            onChange={(e) => setForm({ ...form, status: e.target.value as ContentStatusValue })}
            disabled={isSubmitting}
          >
            {CONTENT_STATUSES.map((status) => (
              <option key={status} value={status}>
                {status}
              </option>
            ))}
          </select>

          <div className="content-form-actions">
            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Saving...' : 'Save'}
            </button>
            <button type="button" onClick={closeForm} disabled={isSubmitting} className="secondary">
              Cancel
            </button>
          </div>
        </form>
      )}

      {isLoading ? (
        <p>Loading...</p>
      ) : items.length === 0 ? (
        <p className="dashboard-placeholder">No announcements yet.</p>
      ) : (
        <table className="content-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Status</th>
              <th>Effective date</th>
              <th>Author</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr key={item.id}>
                <td>{item.title}</td>
                <td>
                  <span className={`status-badge status-${item.status.toLowerCase()}`}>{item.status}</span>
                </td>
                <td>{item.effectiveDateUtc ? new Date(item.effectiveDateUtc).toLocaleString() : ''}</td>
                <td>{item.createdByName}</td>
                <td className="content-table-actions">
                  <button onClick={() => openEditForm(item)}>Edit</button>
                  <button onClick={() => handleDelete(item)} className="danger">
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
