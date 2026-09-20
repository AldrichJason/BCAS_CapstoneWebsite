import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import * as eventsApi from '../../api/eventsApi';
import { CONTENT_STATUSES } from '../../types/news';
import type { ContentStatusValue } from '../../types/news';
import type { EventDto, EventRequest } from '../../types/event';
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

const emptyForm: EventRequest = {
  title: '',
  description: '',
  eventStartUtc: '',
  eventEndUtc: '',
  venue: '',
  status: 'Draft',
};

export function EventsPage() {
  const [items, setItems] = useState<EventDto[]>([]);
  const [statusFilter, setStatusFilter] = useState<ContentStatusValue | ''>('');
  const [isLoading, setIsLoading] = useState(true);
  const [listError, setListError] = useState<string | null>(null);

  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<EventRequest>(emptyForm);
  const [fieldErrors, setFieldErrors] = useState<{ eventEndUtc?: string }>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isFormOpen, setIsFormOpen] = useState(false);

  async function loadEvents() {
    setIsLoading(true);
    setListError(null);
    try {
      const data = await eventsApi.listEvents(statusFilter || undefined);
      setItems(data);
    } catch (error) {
      setListError(errorMessage(error, 'Failed to load events.'));
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadEvents();
    // eslint-disable-next-line react-hooks/exhaustive-deps -- reload only when the status filter changes
  }, [statusFilter]);

  function openCreateForm() {
    setEditingId(null);
    setForm(emptyForm);
    setFieldErrors({});
    setFormError(null);
    setIsFormOpen(true);
  }

  function openEditForm(item: EventDto) {
    setEditingId(item.id);
    setForm({
      title: item.title,
      description: item.description,
      eventStartUtc: toDateTimeLocal(item.eventStartUtc),
      eventEndUtc: toDateTimeLocal(item.eventEndUtc),
      venue: item.venue ?? '',
      status: item.status,
    });
    setFieldErrors({});
    setFormError(null);
    setIsFormOpen(true);
  }

  function closeForm() {
    setIsFormOpen(false);
    setEditingId(null);
    setForm(emptyForm);
    setFieldErrors({});
    setFormError(null);
  }

  function validate(): boolean {
    const errors: { eventEndUtc?: string } = {};
    if (form.eventEndUtc && new Date(form.eventEndUtc) < new Date(form.eventStartUtc)) {
      errors.eventEndUtc = 'End date/time must not precede the start date/time.';
    }
    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFormError(null);

    if (!form.title.trim() || !form.description.trim() || !form.eventStartUtc) {
      setFormError('Title, description and start date/time are required.');
      return;
    }

    if (!validate()) {
      return;
    }

    const payload: EventRequest = {
      ...form,
      eventStartUtc: new Date(form.eventStartUtc).toISOString(),
      eventEndUtc: form.eventEndUtc ? new Date(form.eventEndUtc).toISOString() : null,
    };

    setIsSubmitting(true);
    try {
      if (editingId === null) {
        await eventsApi.createEvent(payload);
      } else {
        await eventsApi.updateEvent(editingId, payload);
      }
      closeForm();
      await loadEvents();
    } catch (error) {
      setFormError(errorMessage(error, 'Failed to save the event.'));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleDelete(item: EventDto) {
    if (!window.confirm(`Delete "${item.title}"? This cannot be undone.`)) {
      return;
    }
    try {
      await eventsApi.deleteEvent(item.id);
      await loadEvents();
    } catch (error) {
      setListError(errorMessage(error, 'Failed to delete the event.'));
    }
  }

  return (
    <div className="content-page">
      <div className="content-page-header">
        <h2>Department Events</h2>
        <button onClick={openCreateForm}>+ New Event</button>
      </div>

      <div className="content-filters">
        <label htmlFor="event-status-filter">Status</label>
        <select
          id="event-status-filter"
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
          <h3>{editingId === null ? 'New Event' : 'Edit Event'}</h3>

          {formError && <div className="form-error" role="alert">{formError}</div>}

          <label htmlFor="event-title">Title</label>
          <input
            id="event-title"
            value={form.title}
            onChange={(e) => setForm({ ...form, title: e.target.value })}
            disabled={isSubmitting}
          />

          <label htmlFor="event-description">Description</label>
          <textarea
            id="event-description"
            rows={5}
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            disabled={isSubmitting}
          />

          <label htmlFor="event-start">Start date/time</label>
          <input
            id="event-start"
            type="datetime-local"
            value={form.eventStartUtc}
            onChange={(e) => setForm({ ...form, eventStartUtc: e.target.value })}
            disabled={isSubmitting}
          />

          <label htmlFor="event-end">End date/time (optional)</label>
          <input
            id="event-end"
            type="datetime-local"
            value={form.eventEndUtc ?? ''}
            onChange={(e) => setForm({ ...form, eventEndUtc: e.target.value })}
            disabled={isSubmitting}
          />
          {fieldErrors.eventEndUtc && <span className="field-error">{fieldErrors.eventEndUtc}</span>}

          <label htmlFor="event-venue">Venue</label>
          <input
            id="event-venue"
            value={form.venue}
            onChange={(e) => setForm({ ...form, venue: e.target.value })}
            disabled={isSubmitting}
          />

          <label htmlFor="event-status">Status</label>
          <select
            id="event-status"
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
        <p className="dashboard-placeholder">No events yet.</p>
      ) : (
        <table className="content-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Status</th>
              <th>Starts</th>
              <th>Ends</th>
              <th>Venue</th>
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
                <td>{new Date(item.eventStartUtc).toLocaleString()}</td>
                <td>{item.eventEndUtc ? new Date(item.eventEndUtc).toLocaleString() : ''}</td>
                <td>{item.venue}</td>
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
