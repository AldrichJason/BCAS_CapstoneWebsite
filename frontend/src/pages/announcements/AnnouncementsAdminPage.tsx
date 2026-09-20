import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import * as announcementsAdminApi from '../../api/announcementsAdminApi';
import * as departmentsApi from '../../api/departmentsApi';
import { CONTENT_STATUSES } from '../../types/news';
import type { ContentStatusValue } from '../../types/news';
import type { AnnouncementDto, AnnouncementRequest } from '../../types/announcement';
import type { DepartmentDto } from '../../types/department';
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

const SCHOOL_WIDE_FILTER = 'schoolwide';

const emptyForm: AnnouncementRequest = {
  title: '',
  body: '',
  effectiveDateUtc: '',
  status: 'Draft',
};

const inputClass =
  'rounded-lg border border-neutral-300 px-2.5 py-2 text-sm focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent/30 disabled:bg-neutral-50';
const labelClass = 'mb-1 text-xs font-semibold';
const primaryButtonClass =
  'rounded-lg bg-primary px-5 py-2.5 font-semibold text-white transition-colors hover:bg-primary-light disabled:cursor-not-allowed disabled:bg-neutral-400';
const secondaryButtonClass =
  'rounded-lg border border-neutral-300 px-5 py-2.5 font-semibold text-neutral-700 transition-colors hover:bg-neutral-50 disabled:cursor-not-allowed disabled:text-neutral-400';

export function AnnouncementsAdminPage() {
  const [items, setItems] = useState<AnnouncementDto[]>([]);
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [departmentFilter, setDepartmentFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState<ContentStatusValue | ''>('');
  const [isLoading, setIsLoading] = useState(true);
  const [listError, setListError] = useState<string | null>(null);

  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<AnnouncementRequest>(emptyForm);
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isFormOpen, setIsFormOpen] = useState(false);

  useEffect(() => {
    async function loadDepartments() {
      try {
        const data = await departmentsApi.listDepartments();
        setDepartments(data);
      } catch {
        setDepartments([]);
      }
    }
    loadDepartments();
  }, []);

  async function loadAnnouncements() {
    setIsLoading(true);
    setListError(null);
    try {
      const params: { departmentId?: number; status?: string } = {};
      if (departmentFilter === SCHOOL_WIDE_FILTER) {
        params.departmentId = 0;
      } else if (departmentFilter) {
        params.departmentId = Number(departmentFilter);
      }
      if (statusFilter) {
        params.status = statusFilter;
      }
      const data = await announcementsAdminApi.listAllAnnouncements(params);
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
  }, [departmentFilter, statusFilter]);

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
        await announcementsAdminApi.createSchoolWideAnnouncement(payload);
      } else {
        await announcementsAdminApi.updateSchoolWideAnnouncement(editingId, payload);
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
      await announcementsAdminApi.deleteSchoolWideAnnouncement(item.id);
      await loadAnnouncements();
    } catch (error) {
      setListError(errorMessage(error, 'Failed to delete the announcement.'));
    }
  }

  return (
    <div className="mx-auto max-w-4xl px-6 py-8">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-primary">School Announcements (All Departments)</h1>
        <button onClick={openCreateForm} className={primaryButtonClass}>
          + New School-Wide Announcement
        </button>
      </div>

      <div className="mb-4 flex flex-wrap items-center gap-4">
        <div className="flex items-center gap-2">
          <label htmlFor="department-filter" className="text-sm font-semibold text-neutral-700">
            Department
          </label>
          <select
            id="department-filter"
            value={departmentFilter}
            onChange={(e) => setDepartmentFilter(e.target.value)}
            className={inputClass}
          >
            <option value="">All</option>
            <option value={SCHOOL_WIDE_FILTER}>School-wide only</option>
            {departments.map((dept) => (
              <option key={dept.id} value={dept.id}>
                {dept.name}
              </option>
            ))}
          </select>
        </div>

        <div className="flex items-center gap-2">
          <label htmlFor="status-filter" className="text-sm font-semibold text-neutral-700">
            Status
          </label>
          <select
            id="status-filter"
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
          <h2 className="mb-1 mt-0 text-lg font-semibold text-primary">
            {editingId === null ? 'New School-Wide Announcement' : 'Edit School-Wide Announcement'}
          </h2>
          <p className="mb-4 text-xs text-neutral-500">
            School-wide announcements are visible to every department. Department-specific
            Announcements are managed by each department's Academic Head.
          </p>

          {formError && (
            <div className="mb-4 rounded-lg bg-red-50 px-3 py-2.5 text-sm text-red-800" role="alert">
              {formError}
            </div>
          )}

          <div className="mb-3 flex flex-col">
            <label htmlFor="admin-announcement-title" className={labelClass}>
              Title
            </label>
            <input
              id="admin-announcement-title"
              value={form.title}
              onChange={(e) => setForm({ ...form, title: e.target.value })}
              disabled={isSubmitting}
              className={inputClass}
            />
          </div>

          <div className="mb-3 flex flex-col">
            <label htmlFor="admin-announcement-body" className={labelClass}>
              Body
            </label>
            <textarea
              id="admin-announcement-body"
              rows={6}
              value={form.body}
              onChange={(e) => setForm({ ...form, body: e.target.value })}
              disabled={isSubmitting}
              className={inputClass}
            />
          </div>

          <div className="mb-3 flex gap-4">
            <div className="flex flex-1 flex-col">
              <label htmlFor="admin-announcement-effective-date" className={labelClass}>
                Effective date
              </label>
              <input
                id="admin-announcement-effective-date"
                type="datetime-local"
                value={form.effectiveDateUtc}
                onChange={(e) => setForm({ ...form, effectiveDateUtc: e.target.value })}
                disabled={isSubmitting}
                className={inputClass}
              />
            </div>

            <div className="flex flex-1 flex-col">
              <label htmlFor="admin-announcement-status" className={labelClass}>
                Status
              </label>
              <select
                id="admin-announcement-status"
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
        <p className="text-neutral-500">No announcements found.</p>
      ) : (
        <table className="w-full border-collapse overflow-hidden rounded-xl bg-white shadow-md">
          <thead>
            <tr>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Title
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Scope
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Status
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Effective date
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Author
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5"></th>
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr key={item.id}>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">{item.title}</td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">
                  {item.isSchoolWide ? (
                    <span className="rounded-full bg-accent-tint px-2.5 py-0.5 text-xs font-semibold text-accent-dark">
                      School-wide
                    </span>
                  ) : (
                    <span className="rounded-full bg-primary-tint px-2.5 py-0.5 text-xs font-semibold text-primary">
                      {item.departmentName}
                    </span>
                  )}
                </td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">
                  <StatusBadge status={item.status} />
                </td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">
                  {item.effectiveDateUtc ? new Date(item.effectiveDateUtc).toLocaleString() : ''}
                </td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">{item.createdByName}</td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">
                  {item.isSchoolWide ? (
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
                  ) : (
                    <span className="text-xs italic text-neutral-400">Managed by department</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
