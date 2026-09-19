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
    <div className="auth-page">
      <form className="auth-form" onSubmit={handleSubmit} noValidate>
        <h1>BCAS Admin Portal</h1>
        <p className="auth-subtitle">Sign in to continue</p>

        {formError && <div className="form-error" role="alert">{formError}</div>}

        <label htmlFor="emailOrUsername">Email or username</label>
        <input
          id="emailOrUsername"
          type="text"
          value={emailOrUsername}
          onChange={(e) => setEmailOrUsername(e.target.value)}
          disabled={isSubmitting}
          autoComplete="username"
        />
        {fieldErrors.emailOrUsername && (
          <span className="field-error">{fieldErrors.emailOrUsername}</span>
        )}

        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          disabled={isSubmitting}
          autoComplete="current-password"
        />
        {fieldErrors.password && <span className="field-error">{fieldErrors.password}</span>}

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Signing in...' : 'Sign in'}
        </button>
        <Link className="auth-link" to="/forgot-password">
          Forgot password?
        </Link>
      </form>
    </div>
  );
}
