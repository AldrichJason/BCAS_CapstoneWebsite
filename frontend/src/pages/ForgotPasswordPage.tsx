import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link } from 'react-router-dom';
import * as authApi from '../api/authApi';

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [fieldError, setFieldError] = useState<string | null>(null);
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

    setIsSubmitting(true);
    try {
      const response = await authApi.forgotPassword(email);
      setDevPreviewCode(response.devPreviewCode ?? null);
    } finally {
      setIsSubmitting(false);
      // Always show the same confirmation, regardless of outcome, so the
      // response can't be used to check which emails have accounts.
      setSubmitted(true);
    }
  }

  return (
    <div className="auth-page">
      <form className="auth-form" onSubmit={handleSubmit} noValidate>
        <h1>Forgot password</h1>
        <p className="auth-subtitle">We'll email you a code to reset it</p>

        {submitted ? (
          <>
            <div className="form-success" role="status">
              If an account exists for that email, a 6-digit code has been sent. It expires in 30
              minutes.
            </div>
            {devPreviewCode && (
              <div className="dev-preview">
                <strong>Dev preview</strong> (no email server configured — this would normally
                only arrive by email):
                <br />
                Your code is <span className="dev-preview-code">{devPreviewCode}</span>
              </div>
            )}
            <Link className="auth-link" to="/reset-password">
              Enter code
            </Link>
            <Link className="auth-link" to="/login">
              Back to sign in
            </Link>
          </>
        ) : (
          <>
            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={isSubmitting}
              autoComplete="username"
            />
            {fieldError && <span className="field-error">{fieldError}</span>}

            <button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Sending...' : 'Send reset code'}
            </button>
            <Link className="auth-link" to="/login">
              Back to sign in
            </Link>
          </>
        )}
      </form>
    </div>
  );
}
