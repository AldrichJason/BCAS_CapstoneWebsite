import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import * as adminApi from '../api/adminApi';
import { useAuth } from '../context/AuthContext';
import { ROLE_OPTIONS } from '../types/admin';
import type { AdminAccountDto } from '../types/admin';
import type { ErrorResponseDto } from '../types/auth';

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
  const [email, setEmail] = useState('');
  const [role, setRole] = useState<string>(ROLE_OPTIONS[1].value);
  const [departmentId, setDepartmentId] = useState<number | ''>('');
  const [formError, setFormError] = useState<string | null>(null);
  const [formSuccess, setFormSuccess] = useState<string | null>(null);
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

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFormError(null);
    setFormSuccess(null);
    setIsSubmitting(true);

    try {
      await adminApi.createAccount({
        fullName,
        email,
        role,
        departmentId: role === 'AcademicHead' ? (departmentId === '' ? null : departmentId) : null,
      });
      setFormSuccess(`${email} was created and sent an invite to set their password.`);
      setFullName('');
      setEmail('');
      setRole(ROLE_OPTIONS[1].value);
      setDepartmentId('');
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
        </div>

        <div className="admin-form-row">
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
          {role === 'AcademicHead' && (
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
          )}
        </div>

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
