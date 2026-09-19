import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import * as adminApi from '../api/adminApi';
import { useAuth } from '../context/AuthContext';
import { ROLE_OPTIONS } from '../types/admin';
import type { AdminAccountDto } from '../types/admin';
import type { ErrorResponseDto } from '../types/auth';

const PASSWORD_POLICY = /^(?=.*[A-Za-z])(?=.*\d).{8,}$/;

function isAxiosErrorResponse(error: unknown): error is { response?: { data?: ErrorResponseDto } } {
  return typeof error === 'object' && error !== null && 'response' in error;
}

const DEPARTMENTS = [
  { id: 1, name: 'College of Computer Studies' },
  { id: 2, name: 'College of Business Administration' },
  { id: 3, name: 'College of Engineering' },
  { id: 4, name: 'College of Arts and Sciences' },
];

export function AdminAccountsPage() {
  const { user: currentUser } = useAuth();
  const [accounts, setAccounts] = useState<AdminAccountDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [listError, setListError] = useState<string | null>(null);

  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [role, setRole] = useState<string>(ROLE_OPTIONS[1].value);
  const [departmentId, setDepartmentId] = useState<number | ''>('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [passwordFieldError, setPasswordFieldError] = useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [formSuccess, setFormSuccess] = useState<string | null>(null);
  const [devPreviewCode, setDevPreviewCode] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function loadAccounts() {
    setIsLoading(true);
    setListError(null);
    try {
      setAccounts(await adminApi.listAccounts());
    } catch {
      setListError('Could not load accounts.');
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadAccounts();
  }, []);

  function resetForm() {
    setFirstName('');
    setLastName('');
    setUsername('');
    setEmail('');
    setRole(ROLE_OPTIONS[1].value);
    setDepartmentId('');
    setPassword('');
    setConfirmPassword('');
  }

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFormError(null);
    setFormSuccess(null);
    setDevPreviewCode(null);
    setPasswordFieldError(null);

    // Password is optional: leave both blank to email an invite code instead.
    if (password || confirmPassword) {
      if (!PASSWORD_POLICY.test(password)) {
        setPasswordFieldError('Must be at least 8 characters and include a letter and a number.');
        return;
      }
      if (password !== confirmPassword) {
        setPasswordFieldError('Passwords do not match.');
        return;
      }
    }

    setIsSubmitting(true);
    try {
      const created = await adminApi.createAccount({
        firstName,
        lastName,
        username,
        email,
        role,
        departmentId: role === 'AcademicHead' ? (departmentId === '' ? null : departmentId) : null,
        password: password || undefined,
        confirmPassword: confirmPassword || undefined,
      });
      setFormSuccess(
        password
          ? `${email} was created with the password you set.`
          : `${email} was created and sent an invite code to set their password.`,
      );
      setDevPreviewCode(created.inviteCode ?? null);
      resetForm();
      await loadAccounts();
    } catch (error) {
      const message = isAxiosErrorResponse(error) ? error.response?.data?.message : undefined;
      setFormError(message ?? 'Could not create the account.');
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleToggleActive(account: AdminAccountDto) {
    try {
      await adminApi.setAccountActive(account.id, !account.isActive);
      await loadAccounts();
    } catch (error) {
      const message = isAxiosErrorResponse(error) ? error.response?.data?.message : undefined;
      setListError(message ?? 'Could not update that account.');
    }
  }

  const inputClass =
    'rounded-lg border border-neutral-300 px-2.5 py-2 text-sm focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent/30';
  const labelClass = 'mb-1 text-xs font-semibold';

  return (
    <div className="mx-auto max-w-4xl px-6 py-8">
      <h1 className="mb-6 text-2xl font-bold text-primary">Admin accounts</h1>

      <form
        className="mb-8 max-w-2xl rounded-xl border-t-4 border-accent bg-white p-6 shadow-md"
        onSubmit={handleCreate}
      >
        <h2 className="mb-3 mt-0 text-lg font-semibold text-primary">Create account</h2>
        {formError && (
          <div className="mb-4 rounded-lg bg-red-50 px-3 py-2.5 text-sm text-red-800" role="alert">
            {formError}
          </div>
        )}
        {formSuccess && (
          <div className="mb-4 rounded-lg bg-primary-tint px-3 py-2.5 text-sm text-primary" role="status">
            {formSuccess}
          </div>
        )}
        {devPreviewCode && (
          <div className="mb-4 rounded-lg border-l-4 border-accent bg-accent-tint px-3 py-2.5 text-xs leading-relaxed text-amber-900">
            <strong>Dev preview</strong> (no email server configured): invite code is{' '}
            <span className="font-mono text-lg font-bold tracking-widest text-accent-dark">
              {devPreviewCode}
            </span>
          </div>
        )}

        <div className="mb-3 flex gap-4">
          <div className="flex flex-1 flex-col">
            <label htmlFor="firstName" className={labelClass}>
              First name
            </label>
            <input
              id="firstName"
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              disabled={isSubmitting}
              required
              className={inputClass}
            />
          </div>
          <div className="flex flex-1 flex-col">
            <label htmlFor="lastName" className={labelClass}>
              Last name
            </label>
            <input
              id="lastName"
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              disabled={isSubmitting}
              required
              className={inputClass}
            />
          </div>
        </div>

        <div className="mb-3 flex gap-4">
          <div className="flex flex-1 flex-col">
            <label htmlFor="username" className={labelClass}>
              Username
            </label>
            <input
              id="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              disabled={isSubmitting}
              required
              className={inputClass}
            />
          </div>
          <div className="flex flex-1 flex-col">
            <label htmlFor="newAccountEmail" className={labelClass}>
              Email
            </label>
            <input
              id="newAccountEmail"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={isSubmitting}
              required
              className={inputClass}
            />
          </div>
        </div>

        <div className="mb-3 flex gap-4">
          <div className="flex flex-1 flex-col">
            <label htmlFor="role" className={labelClass}>
              Role
            </label>
            <select
              id="role"
              value={role}
              onChange={(e) => {
                setRole(e.target.value);
                setDepartmentId('');
              }}
              disabled={isSubmitting}
              className={inputClass}
            >
              {ROLE_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        {role === 'AcademicHead' && (
          <div className="mb-3 flex gap-4">
            <div className="flex flex-1 flex-col">
              <label htmlFor="departmentId" className={labelClass}>
                Department
              </label>
              <select
                id="departmentId"
                value={departmentId}
                onChange={(e) => setDepartmentId(e.target.value ? Number(e.target.value) : '')}
                disabled={isSubmitting}
                required
                className={inputClass}
              >
                <option value="">Select a department</option>
                {DEPARTMENTS.map((dept) => (
                  <option key={dept.id} value={dept.id}>
                    {dept.name}
                  </option>
                ))}
              </select>
            </div>
          </div>
        )}

        <p className="my-2 text-xs text-neutral-500">
          Optional: set the initial password yourself. Leave both blank to email the new user a
          code to set their own password instead.
        </p>
        <div className="mb-3 flex gap-4">
          <div className="flex flex-1 flex-col">
            <label htmlFor="newAccountPassword" className={labelClass}>
              Password (optional)
            </label>
            <input
              id="newAccountPassword"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              disabled={isSubmitting}
              autoComplete="new-password"
              className={inputClass}
            />
          </div>
          <div className="flex flex-1 flex-col">
            <label htmlFor="newAccountConfirmPassword" className={labelClass}>
              Confirm password
            </label>
            <input
              id="newAccountConfirmPassword"
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              disabled={isSubmitting}
              autoComplete="new-password"
              className={inputClass}
            />
          </div>
        </div>
        {passwordFieldError && <span className="text-xs text-red-700">{passwordFieldError}</span>}

        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-lg bg-primary px-5 py-2.5 font-semibold text-white transition-colors hover:bg-primary-light disabled:cursor-not-allowed disabled:bg-neutral-400"
        >
          {isSubmitting ? 'Creating...' : 'Create account'}
        </button>
      </form>

      <h2 className="mb-3 text-lg font-semibold text-primary">All accounts</h2>
      {listError && (
        <div className="mb-4 rounded-lg bg-red-50 px-3 py-2.5 text-sm text-red-800" role="alert">
          {listError}
        </div>
      )}
      {isLoading ? (
        <p>Loading...</p>
      ) : (
        <table className="w-full border-collapse overflow-hidden rounded-xl bg-white shadow-md">
          <thead>
            <tr>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Name
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Username
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Email
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Role
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Department
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary">
                Status
              </th>
              <th className="border-b border-neutral-200 bg-primary-tint px-4 py-2.5 text-left text-sm font-semibold text-primary"></th>
            </tr>
          </thead>
          <tbody>
            {accounts.map((account) => (
              <tr key={account.id}>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">
                  {account.firstName} {account.lastName}
                </td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">{account.username}</td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">{account.email}</td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">{account.role}</td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">
                  {account.departmentName ?? '—'}
                </td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">
                  {account.isActive ? 'Active' : 'Inactive'}
                </td>
                <td className="border-b border-neutral-200 px-4 py-2.5 text-sm">
                  <button
                    onClick={() => handleToggleActive(account)}
                    disabled={account.id === currentUser?.id && account.isActive}
                    title={
                      account.id === currentUser?.id && account.isActive
                        ? 'You cannot deactivate your own account'
                        : undefined
                    }
                    className="rounded-md border border-accent-dark px-3 py-1.5 font-semibold text-accent-dark transition-colors hover:bg-accent-tint disabled:cursor-not-allowed disabled:border-neutral-300 disabled:text-neutral-400"
                  >
                    {account.isActive ? 'Deactivate' : 'Reactivate'}
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
