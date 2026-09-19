import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import * as authApi from '../api/authApi';
import type { ErrorResponseDto } from '../types/auth';

const PASSWORD_POLICY = /^(?=.*[A-Za-z])(?=.*\d).{8,}$/;

function isAxiosErrorResponse(error: unknown): error is { response?: { data?: ErrorResponseDto } } {
  return typeof error === 'object' && error !== null && 'response' in error;
}

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') ?? '';

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [fieldErrors, setFieldErrors] = useState<{ newPassword?: string; confirmPassword?: string }>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [succeeded, setSucceeded] = useState(false);

  if (!token) {
    return (
      <div className="auth-page">
        <div className="auth-form">
          <h1>Reset password</h1>
          <div className="form-error" role="alert">
            This link is missing its reset token. Please use the link from your email, or request
            a new one.
          </div>
          <Link className="auth-link" to="/forgot-password">
            Request a new link
          </Link>
        </div>
      </div>
    );
  }

  function validate(): boolean {
    const errors: { newPassword?: string; confirmPassword?: string } = {};
    if (!PASSWORD_POLICY.test(newPassword)) {
      errors.newPassword = 'Must be at least 8 characters and include a letter and a number.';
    }
    if (confirmPassword !== newPassword) {
      errors.confirmPassword = 'Passwords do not match.';
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
      await authApi.resetPassword(token, newPassword, confirmPassword);
      setSucceeded(true);
    } catch (error) {
      const message = isAxiosErrorResponse(error) ? error.response?.data?.message : undefined;
      setFormError(message ?? 'This reset link is invalid or has expired.');
    } finally {
      setIsSubmitting(false);
    }
  }

  if (succeeded) {
    return (
      <div className="auth-page">
        <div className="auth-form">
          <h1>Password reset</h1>
          <div className="form-success" role="status">
            Your password has been changed. You can now sign in.
          </div>
          <Link className="auth-link" to="/login">
            Go to sign in
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-page">
      <form className="auth-form" onSubmit={handleSubmit} noValidate>
        <h1>Reset password</h1>
        <p className="auth-subtitle">Choose a new password</p>

        {formError && (
          <>
            <div className="form-error" role="alert">
              {formError}
            </div>
            <Link className="auth-link" to="/forgot-password">
              Request a new link
            </Link>
          </>
        )}

        <label htmlFor="newPassword">New password</label>
        <input
          id="newPassword"
          type="password"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
          disabled={isSubmitting}
          autoComplete="new-password"
        />
        {fieldErrors.newPassword && <span className="field-error">{fieldErrors.newPassword}</span>}

        <label htmlFor="confirmPassword">Confirm new password</label>
        <input
          id="confirmPassword"
          type="password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          disabled={isSubmitting}
          autoComplete="new-password"
        />
        {fieldErrors.confirmPassword && (
          <span className="field-error">{fieldErrors.confirmPassword}</span>
        )}

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Resetting...' : 'Reset password'}
        </button>
      </form>
    </div>
  );
}
