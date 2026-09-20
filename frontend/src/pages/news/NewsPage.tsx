import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import * as newsApi from '../../api/newsApi';
import { CONTENT_STATUSES } from '../../types/news';
import type { ContentStatusValue, NewsDto, NewsRequest } from '../../types/news';
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

const emptyForm: NewsRequest = {
  title: '',
  body: '',
  publishAtUtc: '',
  status: 'Draft',
};

export function NewsPage() {
  const [items, setItems] = useState<NewsDto[]>([]);
  const [statusFilter, setStatusFilter] = useState<ContentStatusValue | ''>('');
  const [isLoading, setIsLoading] = useState(true);
  const [listError, setListError] = useState<string | null>(null);

  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<NewsRequest>(emptyForm);
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isFormOpen, setIsFormOpen] = useState(false);

  async function loadNews() {
    setIsLoading(true);
    setListError(null);
    try {
      const data = await newsApi.listNews(statusFilter || undefined);
      setItems(data);
    } catch (error) {
      setListError(errorMessage(error, 'Failed to load news.'));
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadNews();
    // eslint-disable-next-line react-hooks/exhaustive-deps -- reload only when the status filter changes
  }, [statusFilter]);

  function openCreateForm() {
    setEditingId(null);
    setForm(emptyForm);
    setFormError(null);
    setIsFormOpen(true);
  }

  function openEditForm(item: NewsDto) {
    setEditingId(item.id);
    setForm({
      title: item.title,
      body: item.body,
      publishAtUtc: toDateTimeLocal(item.publishAtUtc),
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

    if (!form.title.trim() || !form.body.trim() || !form.publishAtUtc) {
      setFormError('Title, body and publish date are required.');
      return;
    }

    const payload: NewsRequest = {
      ...form,
      publishAtUtc: new Date(form.publishAtUtc).toISOString(),
    };

    setIsSubmitting(true);
    try {
      if (editingId === null) {
        await newsApi.createNews(payload);
      } else {
        await newsApi.updateNews(editingId, payload);
      }
      closeForm();
      await loadNews();
    } catch (error) {
      setFormError(errorMessage(error, 'Failed to save the news article.'));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDelete(item: NewsDto) {
    if (!window.confirm(`Delete "${item.title}"? This cannot be undone.`)) {
      return;
    }
    try {
      await newsApi.deleteNews(item.id);
      await loadNews();
    } catch (error) {
      setListError(errorMessage(error, 'Failed to delete the news article.'));
    }
  }

  return (
    <div className="content-page">
      <div className="content-page-header">
        <h2>Department News</h2>
        <button onClick={openCreateForm}>+ New Article</button>
      </div>

      <div className="content-filters">
        <label htmlFor="status-filter">Status</label>
        <select
          id="status-filter"
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
      </div>

      {listError && <div className="form-error" role="alert">{listError}</div>}

      {isFormOpen && (
        <form className="content-form" onSubmit={handleSubmit}>
          <h3>{editingId === null ? 'New Article' : 'Edit Article'}</h3>

          {formError && <div className="form-error" role="alert">{formError}</div>}

          <label htmlFor="news-title">Title</label>
          <input
            id="news-title"
            value={form.title}
            onChange={(e) => setForm({ ...form, title: e.target.value })}
            disabled={isSubmitting}
          />

          <label htmlFor="news-body">Body</label>
          <textarea
            id="news-body"
            rows={6}
            value={form.body}
            onChange={(e) => setForm({ ...form, body: e.target.value })}
            disabled={isSubmitting}
          />

          <label htmlFor="news-publish">Publish date</label>
          <input
            id="news-publish"
            type="datetime-local"
            value={form.publishAtUtc}
            onChange={(e) => setForm({ ...form, publishAtUtc: e.target.value })}
            disabled={isSubmitting}
          />

          <label htmlFor="news-status">Status</label>
          <select
            id="news-status"
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
        <p className="dashboard-placeholder">No news articles yet.</p>
      ) : (
        <table className="content-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Status</th>
              <th>Author</th>
              <th>Last updated</th>
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
                <td>{item.createdByName}</td>
                <td>{new Date(item.updatedAt ?? item.createdAt).toLocaleString()}</td>
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
