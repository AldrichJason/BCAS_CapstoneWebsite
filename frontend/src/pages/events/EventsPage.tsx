import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import * as eventsApi from '../../api/eventsApi';
import { CONTENT_STATUSES } from '../../types/news';
import type { ContentStatusValue } from '../../types/news';
import type { EventDto, EventRequest } from '../../types/event';
import type { ErrorResponseDto } from '../../types/auth';
import { StatusBadge } from '../../components/StatusBadge';

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

const inputClass =
  'rounded-lg border border-neutral-300 px-2.5 py-2 text-sm focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent/30 disabled:bg-neutral-50';
const labelClass = 'mb-1 text-xs font-semibold';
const primaryButtonClass =
  'rounded-lg bg-primary px-5 py-2.5 font-semibold text-white transition-colors hover:bg-primary-light disabled:cursor-not-allowed disabled:bg-neutral-400';
const secondaryButtonClass =
  'rounded-lg border border-neutral-300 px-5 py-2.5 font-semibold text-neutral-700 transition-colors hover:bg-neutral-50 disabled:cursor-not-allowed disabled:text-neutral-400';

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
    <div className="mx-auto max-w-4xl px-6 py-8">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-primary">Department Events</h1>
        <button onClick={openCreateForm} className={primaryButtonClass}>
          + New Event
        </button>
      </div>

      <div className="mb-4 flex items-center gap-2">
        <label htmlFor="event-status-filter" className="text-sm font-semibold text-neutral-700">
          Status
        </label>
        <select
          id="event-status-filter"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value as ContentStatusValue | '')}
          className={inputClass}
        >
          <option value="">All</option>
          {CONTENT_STATUSES.map((status) => (
            <option key={status} value={status}>
              {status}
            </option>
          ))}
        </select>
      </div>

      {listError && (
        <div className="mb-4 rounded-lg bg-red-50 px-3 py-2.5 text-sm text-red-800" role="alert">
          {listError}
        </div>
      )}

      {isFormOpen && (
        <form
          className="mb-8 flex flex-col rounded-xl border-t-4 border-accent bg-white p-6 shadow-md"
          onSubmit={handleSubmit}
        >
          <h2 className="mb-3 mt-0 text-lg font-semibold text-primary">
            {editingId === null ? 'New Event' : 'Edit Event'}
          </h2>

          {formError && (
            <div className="mb-4 rounded-lg bg-red-50 px-3 py-2.5 text-sm text-red-800" role="alert">
              {formError}
            </div>
          )}

          <div className="mb-3 flex flex-col">
            <label htmlFor="event-title" className={labelClass}>
              Title
            </label>
            <input
              id="event-title"
              value={form.title}
              onChange={(e) => setForm({ ...form, title: e.target.value })}
              disabled={isSubmitting}
              className={inputClass}
            />
          </div>

          <div className="mb-3 flex flex-col">
            <label htmlFor="event-description" className={labelClass}>
              Description
            </label>
            <textarea
              id="event-description"
              rows={5}
              value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
              disabled={isSubmitting}
              className={inputClass}
            />
          </div>

          <div className="mb-3 flex gap-4">
            <div className="flex flex-1 flex-col">
              <label htmlFor="event-start" className={labelClass}>
                Start date/time
              </label>
              <input
                id="event-start"
                type="datetime-local"
                value={form.eventStartUtc}
                onChange={(e) => setForm({ ...form, eventStartUtc: e.target.value })}
                disabled={isSubmitting}
                className={inputClass}
              />
            </div>

            <div className="flex flex-1 flex-col">
              <label htmlFor="event-end" className={labelClass}>
                End date/time (optional)
              </label>
              <input
                id="event-end"
                type="datetime-local"
                value={form.eventEndUtc ?? ''}
                onChange={(e) => setForm({ ...form, eventEndUtc: e.target.value })}
                disabled={isSubmitting}
                className={inputClass}
              />
              {fieldErrors.eventEndUtc && (
                <span className="mt-1 text-xs text-red-700">{fieldErrors.eventEndUtc}</span>
              )}
            </div>
          </div>

          <div className="mb-3 flex gap-4">
            <div className="flex flex-1 flex-col">
              <label htmlFor="event-venue" className={labelClass}>
                Venue
              </label>
              <input
                id="event-venue"
                value={form.venue}
                onChange={(e) => setForm({ ...form, venue: e.target.value })}
                disabled={isSubmitting}
                className={inputClass}
              />
            </div>

            <div className="flex flex-1 flex-col">
              <label htmlFor="event-status" className={labelClass}>
                Status
              </label>
              <select
                id="event-status"
                value={form.status}
                onChange={(e) => setForm({ ...form, status: e.target.value as ContentStatusValue })}
                disabled={isSubmitting}
                className={inputClass}
              >
                {CONTENT_STATUSES.map((status) => (
                  <option key={status} value={status}>
                    {status}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="mt-2 flex gap-3">
            <button type="submit" disabled={isSubmitting} className={primaryButtonClass}>
              {isSubmitting ? 'Saving...' : 'Save'}
            </button>
            <button type="button" onClick={closeForm} disabled={isSubmitting} className={secondaryButtonClass}>
              Cancel
            </button>
          </div>
        </form>
      )}

      {isLoading ? (
        <p>Loading...</p>
      ) : items.length === 0 ? (
        <p className="text-neutral-500">No events yet.</p>
      ) : (
        <table className="w-full border-collapse overflow-hidden rounded-xl bg-white shadow-md">
          <thead>
            <tr>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Title
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Status
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Starts
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Ends
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Venue
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5"></th>
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr key={item.id}>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">{item.title}</td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">
                  <StatusBadge status={item.status} />
                </td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">
                  {new Date(item.eventStartUtc).toLocaleString()}
                </td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">
                  {item.eventEndUtc ? new Date(item.eventEndUtc).toLocaleString() : ''}
                </td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">{item.venue}</td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">
                  <div className="flex gap-2">
                    <button
                      onClick={() => openEditForm(item)}
                      className="rounded-md border border-neutral-300 px-3 py-1.5 font-semibold text-neutral-700 transition-colors hover:bg-neutral-50"
                    >
                      Edit
                    </button>
                    <button
                      onClick={() => handleDelete(item)}
                      className="rounded-md border border-red-300 px-3 py-1.5 font-semibold text-red-700 transition-colors hover:bg-red-50"
                    >
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
