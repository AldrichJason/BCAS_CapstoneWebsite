import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link } from 'react-router-dom';
import * as authApi from '../api/authApi';
import type { ErrorResponseDto } from '../types/auth';

function isAxiosErrorResponse(error: unknown): error is { response?: { data?: ErrorResponseDto } } {
  return typeof error === 'object' && error !== null && 'response' in error;
}

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [fieldError, setFieldError] = useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [devPreviewCode, setDevPreviewCode] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      setFieldError('Enter a valid email address.');
      return;
    }
    setFieldError(null);
    setFormError(null);

    setIsSubmitting(true);
    try {
      const response = await authApi.forgotPassword(email);
      setDevPreviewCode(response.devPreviewCode ?? null);
      // This success message intentionally shows regardless of whether the
      // account exists, so the response can't be used to enumerate accounts.
      // A real send/server failure (below) is a different case and IS shown.
      setSubmitted(true);
    } catch (error) {
      const message = isAxiosErrorResponse(error) ? error.response?.data?.message : undefined;
      setFormError(message ?? 'Something went wrong sending the reset code. Please try again.');
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
        <h1 className="mb-1 text-2xl font-bold text-primary">Forgot password</h1>
        <p className="mb-6 text-neutral-500">We'll email you a code to reset it</p>

        {submitted ? (
          <>
            <div className="mb-4 rounded-lg bg-primary-tint px-3 py-2.5 text-sm text-primary" role="status">
              If an account exists for that email, a 6-digit code has been sent. It expires in 30
              minutes.
            </div>
            {devPreviewCode && (
              <div className="mb-4 rounded-lg border-l-4 border-accent bg-accent-tint px-3 py-2.5 text-xs leading-relaxed text-amber-900">
                <strong>Dev preview</strong> (no email server configured — this would normally
                only arrive by email):
                <br />
                Your code is{' '}
                <span className="font-mono text-lg font-bold tracking-widest text-accent-dark">
                  {devPreviewCode}
                </span>
              </div>
            )}
            <Link
              className="mt-4 text-center text-sm font-semibold text-primary hover:text-accent-dark hover:underline"
              to="/reset-password"
            >
              Enter code
            </Link>
            <Link
              className="mt-4 text-center text-sm font-semibold text-primary hover:text-accent-dark hover:underline"
              to="/login"
            >
              Back to sign in
            </Link>
          </>
        ) : (
          <>
            {formError && (
              <div className="mb-4 rounded-lg bg-red-50 px-3 py-2.5 text-sm text-red-800" role="alert">
                {formError}
              </div>
            )}
            <label htmlFor="email" className="mb-1 text-sm font-semibold">
              Email
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={isSubmitting}
              autoComplete="username"
              className="mb-1 rounded-lg border border-neutral-300 px-3 py-2.5 text-base focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent/30 disabled:bg-neutral-50"
            />
            {fieldError && <span className="mb-3 text-xs text-red-700">{fieldError}</span>}

            <button
              type="submit"
              disabled={isSubmitting}
              className="mt-4 rounded-lg bg-primary py-2.5 font-semibold text-white transition-colors hover:bg-primary-light disabled:cursor-not-allowed disabled:bg-neutral-400"
            >
              {isSubmitting ? 'Sending...' : 'Send reset code'}
            </button>
            <Link
              className="mt-4 text-center text-sm font-semibold text-primary hover:text-accent-dark hover:underline"
              to="/login"
            >
              Back to sign in
            </Link>
          </>
        )}
      </form>
    </div>
  );
}
