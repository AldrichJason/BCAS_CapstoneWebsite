import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import type { ErrorResponseDto } from '../types/auth';

function isAxiosErrorResponse(
  error: unknown,
): error is { response?: { status?: number; data?: ErrorResponseDto } } {
  return typeof error === 'object' && error !== null && 'response' in error;
}

export function LoginPage() {
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  const [emailOrUsername, setEmailOrUsername] = useState('');
  const [password, setPassword] = useState('');
  const [fieldErrors, setFieldErrors] = useState<{ emailOrUsername?: string; password?: string }>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  function validate(): boolean {
    const errors: { emailOrUsername?: string; password?: string } = {};
    if (!emailOrUsername.trim()) {
      errors.emailOrUsername = 'Email or username is required.';
    }
    if (!password) {
      errors.password = 'Password is required.';
    }
    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFormError(null);

    if (!validate()) {
      return;
    }

    setIsSubmitting(true);
    try {
      await login(emailOrUsername, password);
      navigate('/dashboard', { replace: true });
    } catch (error) {
      if (isAxiosErrorResponse(error)) {
        if (error.response?.status === 401) {
          setFormError(error.response.data?.message ?? 'Invalid credentials.');
        } else if (error.response) {
          setFormError(
            error.response.data?.message ??
              `Something went wrong (server returned ${error.response.status}). Please try again.`,
          );
        } else {
          setFormError('Could not reach the server. Is the backend running?');
        }
      } else {
        setFormError('Something went wrong. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center">
      <form
        className="flex w-full max-w-sm flex-col rounded-xl border-t-4 border-accent bg-white p-10 shadow-lg"
        onSubmit={handleSubmit}
        noValidate
      >
        <h1 className="mb-1 text-2xl font-bold text-primary">BCAS Admin Portal</h1>
        <p className="mb-6 text-neutral-500">Sign in to continue</p>

        {formError && (
          <div className="mb-4 rounded-lg bg-red-50 px-3 py-2.5 text-sm text-red-800" role="alert">
            {formError}
          </div>
        )}

        <label htmlFor="emailOrUsername" className="mb-1 text-sm font-semibold">
          Email or username
        </label>
        <input
          id="emailOrUsername"
          type="text"
          value={emailOrUsername}
          onChange={(e) => setEmailOrUsername(e.target.value)}
          disabled={isSubmitting}
          autoComplete="username"
          className="mb-1 rounded-lg border border-neutral-300 px-3 py-2.5 text-base focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent/30 disabled:bg-neutral-50"
        />
        {fieldErrors.emailOrUsername && (
          <span className="mb-3 text-xs text-red-700">{fieldErrors.emailOrUsername}</span>
        )}

        <label htmlFor="password" className="mb-1 text-sm font-semibold">
          Password
        </label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          disabled={isSubmitting}
          autoComplete="current-password"
          className="mb-1 rounded-lg border border-neutral-300 px-3 py-2.5 text-base focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent/30 disabled:bg-neutral-50"
        />
        {fieldErrors.password && (
          <span className="mb-3 text-xs text-red-700">{fieldErrors.password}</span>
        )}

        <button
          type="submit"
          disabled={isSubmitting}
          className="mt-4 rounded-lg bg-primary py-2.5 font-semibold text-white transition-colors hover:bg-primary-light disabled:cursor-not-allowed disabled:bg-neutral-400"
        >
          {isSubmitting ? 'Signing in...' : 'Sign in'}
        </button>
        <Link
          className="mt-4 text-center text-sm font-semibold text-primary hover:text-accent-dark hover:underline"
          to="/forgot-password"
        >
          Forgot password?
        </Link>
      </form>
    </div>
  );
}
