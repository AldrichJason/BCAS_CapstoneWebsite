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

  const [fullName, setFullName] = useState('');
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
    setFullName('');
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
        fullName,
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

  return (
    <div className="dashboard-page">
      <h1>Admin accounts</h1>

      <form className="admin-form" onSubmit={handleCreate}>
        <h2>Create account</h2>
        {formError && <div className="form-error" role="alert">{formError}</div>}
        {formSuccess && <div className="form-success" role="status">{formSuccess}</div>}
        {devPreviewCode && (
          <div className="dev-preview">
            <strong>Dev preview</strong> (no email server configured): invite code is{' '}
            <span className="dev-preview-code">{devPreviewCode}</span>
          </div>
        )}

        <div className="admin-form-row">
          <div>
            <label htmlFor="fullName">Full name</label>
            <input
              id="fullName"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              disabled={isSubmitting}
              required
            />
          </div>
          <div>
            <label htmlFor="username">Username</label>
            <input
              id="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              disabled={isSubmitting}
              required
            />
          </div>
        </div>

        <div className="admin-form-row">
          <div>
            <label htmlFor="newAccountEmail">Email</label>
            <input
              id="newAccountEmail"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={isSubmitting}
              required
            />
          </div>
          <div>
            <label htmlFor="role">Role</label>
            <select
              id="role"
              value={role}
              onChange={(e) => {
                setRole(e.target.value);
                setDepartmentId('');
              }}
              disabled={isSubmitting}
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
          <div className="admin-form-row">
            <div>
              <label htmlFor="departmentId">Department</label>
              <select
                id="departmentId"
                value={departmentId}
                onChange={(e) => setDepartmentId(e.target.value ? Number(e.target.value) : '')}
                disabled={isSubmitting}
                required
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

        <p className="admin-form-hint">
          Optional: set the initial password yourself. Leave both blank to email the new user a
          code to set their own password instead.
        </p>
        <div className="admin-form-row">
          <div>
            <label htmlFor="newAccountPassword">Password (optional)</label>
            <input
              id="newAccountPassword"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              disabled={isSubmitting}
              autoComplete="new-password"
            />
          </div>
          <div>
            <label htmlFor="newAccountConfirmPassword">Confirm password</label>
            <input
              id="newAccountConfirmPassword"
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              disabled={isSubmitting}
              autoComplete="new-password"
            />
          </div>
        </div>
        {passwordFieldError && <span className="field-error">{passwordFieldError}</span>}

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Creating...' : 'Create account'}
        </button>
      </form>

      <h2>All accounts</h2>
      {listError && <div className="form-error" role="alert">{listError}</div>}
      {isLoading ? (
        <p>Loading...</p>
      ) : (
        <table className="admin-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Username</th>
              <th>Email</th>
              <th>Role</th>
              <th>Department</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {accounts.map((account) => (
              <tr key={account.id}>
                <td>{account.fullName}</td>
                <td>{account.username}</td>
                <td>{account.email}</td>
                <td>{account.role}</td>
                <td>{account.departmentName ?? '—'}</td>
                <td>{account.isActive ? 'Active' : 'Inactive'}</td>
                <td>
                  <button
                    onClick={() => handleToggleActive(account)}
                    disabled={account.id === currentUser?.id && account.isActive}
                    title={
                      account.id === currentUser?.id && account.isActive
                        ? 'You cannot deactivate your own account'
                        : undefined
                    }
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
