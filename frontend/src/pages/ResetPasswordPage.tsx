import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link } from 'react-router-dom';
import * as authApi from '../api/authApi';
import type { ErrorResponseDto } from '../types/auth';

const PASSWORD_POLICY = /^(?=.*[A-Za-z])(?=.*\d).{8,}$/;
const CODE_POLICY = /^\d{6}$/;

function isAxiosErrorResponse(error: unknown): error is { response?: { data?: ErrorResponseDto } } {
  return typeof error === 'object' && error !== null && 'response' in error;
}

export function ResetPasswordPage() {
  const [email, setEmail] = useState('');
  const [code, setCode] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [fieldErrors, setFieldErrors] = useState<{
    email?: string;
    code?: string;
    newPassword?: string;
    confirmPassword?: string;
  }>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [succeeded, setSucceeded] = useState(false);

  function validate(): boolean {
    const errors: typeof fieldErrors = {};
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      errors.email = 'Enter a valid email address.';
    }
    if (!CODE_POLICY.test(code)) {
      errors.code = 'Enter the 6-digit code from your email.';
    }
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
      await authApi.resetPassword(email, code, newPassword, confirmPassword);
      setSucceeded(true);
    } catch (error) {
      const message = isAxiosErrorResponse(error) ? error.response?.data?.message : undefined;
      setFormError(message ?? 'That code is invalid or has expired.');
    } finally {
      setIsSubmitting(false);
    }
  }

  const inputClass =
    'mb-1 rounded-lg border border-neutral-300 px-3 py-2.5 text-base focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent/30 disabled:bg-neutral-50';
  const labelClass = 'mb-1 text-sm font-semibold';
  const fieldErrorClass = 'mb-3 text-xs text-red-700';

  if (succeeded) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="flex w-full max-w-sm flex-col rounded-xl border-t-4 border-accent bg-white p-10 shadow-lg">
          <h1 className="mb-1 text-2xl font-bold text-primary">Password reset</h1>
          <div className="mb-4 rounded-lg bg-primary-tint px-3 py-2.5 text-sm text-primary" role="status">
            Your password has been changed. You can now sign in.
          </div>
          <Link
            className="mt-4 text-center text-sm font-semibold text-primary hover:text-accent-dark hover:underline"
            to="/login"
          >
            Go to sign in
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen items-center justify-center">
      <form
        className="flex w-full max-w-sm flex-col rounded-xl border-t-4 border-accent bg-white p-10 shadow-lg"
        onSubmit={handleSubmit}
        noValidate
      >
        <h1 className="mb-1 text-2xl font-bold text-primary">Enter your code</h1>
        <p className="mb-6 text-neutral-500">Check your email for a 6-digit code</p>

        {formError && (
          <>
            <div className="mb-4 rounded-lg bg-red-50 px-3 py-2.5 text-sm text-red-800" role="alert">
              {formError}
            </div>
            <Link
              className="mb-4 text-center text-sm font-semibold text-primary hover:text-accent-dark hover:underline"
              to="/forgot-password"
            >
              Request a new code
            </Link>
          </>
        )}

        <label htmlFor="resetEmail" className={labelClass}>
          Email
        </label>
        <input
          id="resetEmail"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          disabled={isSubmitting}
          autoComplete="username"
          className={inputClass}
        />
        {fieldErrors.email && <span className={fieldErrorClass}>{fieldErrors.email}</span>}

        <label htmlFor="code" className={labelClass}>
          6-digit code
        </label>
        <input
          id="code"
          inputMode="numeric"
          maxLength={6}
          value={code}
          onChange={(e) => setCode(e.target.value.replace(/\D/g, ''))}
          disabled={isSubmitting}
          className={inputClass}
        />
        {fieldErrors.code && <span className={fieldErrorClass}>{fieldErrors.code}</span>}

        <label htmlFor="newPassword" className={labelClass}>
          New password
        </label>
        <input
          id="newPassword"
          type="password"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
          disabled={isSubmitting}
          autoComplete="new-password"
          className={inputClass}
        />
        {fieldErrors.newPassword && <span className={fieldErrorClass}>{fieldErrors.newPassword}</span>}

        <label htmlFor="confirmPassword" className={labelClass}>
          Confirm new password
        </label>
        <input
          id="confirmPassword"
          type="password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          disabled={isSubmitting}
          autoComplete="new-password"
          className={inputClass}
        />
        {fieldErrors.confirmPassword && (
          <span className={fieldErrorClass}>{fieldErrors.confirmPassword}</span>
        )}

        <button
          type="submit"
          disabled={isSubmitting}
          className="mt-4 rounded-lg bg-primary py-2.5 font-semibold text-white transition-colors hover:bg-primary-light disabled:cursor-not-allowed disabled:bg-neutral-400"
        >
          {isSubmitting ? 'Resetting...' : 'Reset password'}
        </button>
      </form>
    </div>
  );
}
