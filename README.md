# BCAS Capstone Website

Admin portal for BCAS: ASP.NET Core Web API (C#) + Dapper backend, React + TypeScript
frontend, SQL Server database.

## Repository structure

```
BCAS_CapstoneWebsite/
├── backend/
│   └── BCAS.Api/
│       ├── Controllers/          # API endpoints
│       ├── Helper/               # Stateless helpers (password hashing, JWT generation)
│       ├── Middleware/           # Cross-cutting request pipeline logic
│       ├── Model/                # Domain models
│       │   └── DTOs/             # Request/response contracts exposed by controllers
│       ├── Repository/           # Dapper data access
│       │   ├── Interfaces/
│       │   └── Utilities/        # DB connection factory and shared data-access helpers
│       └── Service/               # Business logic, orchestrates repositories
│           └── Interfaces/
├── frontend/
│   └── src/
│       ├── api/                  # HTTP client + typed API calls
│       ├── components/           # Shared/reusable components
│       ├── context/              # React context providers (auth state, etc.)
│       ├── pages/                # Route-level views
│       └── types/                # Shared TypeScript types
├── database/
│   ├── schema.sql                # Full schema, repeatable from empty
│   └── seed.sql                  # Departments, roles, initial Super Admin
└── .vscode/                      # Shared editor recommendations/settings
```

### Backend folder conventions

- **Controllers** – thin HTTP endpoints; validate input, call a `Service`, return a DTO.
- **Helper** – small, stateless, reusable utilities (hashing, token generation) with no DB access.
- **Model** – POCOs that mirror the database shape. `Model/DTOs` holds the request/response
  contracts controllers actually expose, so internal model shape can change without breaking
  the API surface.
- **Repository** – all Dapper SQL lives here, one repository per aggregate, behind an interface
  in `Repository/Interfaces` so services can be unit tested against a mock. `Repository/Utilities`
  holds the `IDbConnectionFactory` and any other data-access plumbing (paging helpers, etc.).
- **Service** – business rules and orchestration between one or more repositories. Controllers
  never call repositories directly.
- **Middleware** – pipeline components that need to run on every request (e.g. rejecting
  requests carrying a revoked or deactivated-account token).

This mirrors the layering the ticket asked for (Controllers → Service → Repository → DB), with
`Helper` and `Model/DTOs` as supporting folders. As the codebase grows, suggested additions that
fit this same structure: a `Validators/` folder (FluentValidation) next to `Model`, an
`Extensions/` folder for `IServiceCollection`/`IApplicationBuilder` setup extracted out of
`Program.cs`, and a `Common/` (or `Exceptions/`) folder for shared exception types once more than
one feature needs them.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm
- SQL Server (local, Docker, or Azure SQL) reachable from your machine

## Backend setup

```bash
cd backend/BCAS.Api
cp appsettings.Development.json.example appsettings.Development.json
# edit appsettings.Development.json: connection string + a real Jwt:Key
dotnet restore
dotnet run
```

`appsettings.Development.json` is git-ignored — secrets and local connection strings never get
committed. `appsettings.json` only holds empty placeholders.

## Database setup

Run against your SQL Server instance, in order:

```bash
sqlcmd -S <server> -d master -Q "CREATE DATABASE BcasCapstone"
sqlcmd -S <server> -d BcasCapstone -i database/schema.sql
sqlcmd -S <server> -d BcasCapstone -i database/seed.sql
```

Seed data creates the four departments (rename them in `seed.sql` to match the school's actual
departments), the four roles, and an initial Super Admin:

- Email: `superadmin@bcas.edu.ph`
- Password: `ChangeMe123!` — change this immediately after first login.

## Frontend setup

```bash
cd frontend
cp .env.example .env
npm install
npm run dev
```

The dev server runs at `http://localhost:5173` and expects the API at
`VITE_API_BASE_URL` (default `http://localhost:5000/api`).

## Code style / linting

- Backend: `.editorconfig` at the repo root enforces C# formatting; `dotnet format` before
  committing.
- Frontend: ESLint (`npm run lint`) + Prettier, configured in `frontend/.eslintrc.cjs` and
  `frontend/.prettierrc.json`.

## Branching convention

- `main` is always deployable.
- Work happens on `feature/<ticket-key>-short-description` branches cut from `main`
  (e.g. `feature/BW-10-login-jwt`).
- Open a pull request into `main`; at least one review is required before merging.
- Squash-merge feature branches to keep history linear.

## Current implementation status

Implements the full Sprint 1 authentication & account-management slice:

- **BW-8** – this scaffold (backend/frontend/database structure, linting, `.gitignore`).
- **BW-9** – `database/schema.sql` and `database/seed.sql`.
- **BW-10** – `POST /api/auth/login` (backend) + login page (frontend).
- **BW-11** – `POST /api/auth/logout`, server-side token revocation.
- **BW-12** – `GET /api/auth/session`, session restore on app load.
- **BW-13** – `POST /api/auth/forgot-password` / `POST /api/auth/reset-password`. Emails a
  single-use 6-digit code (hashed, 30 min expiry); the reset page is an email + code + new
  password + confirm form.
- **BW-14** – `POST /api/admin/accounts` (Super Admin only), role + department-scope validation,
  duplicate-email rejection, invite email with a 6-digit code to set the initial password (same
  reset-password page/flow as BW-13).
- **BW-15** – `PATCH /api/admin/accounts/{id}/status` (Super Admin only) toggles an account
  active/inactive; deactivated accounts are rejected on their next request via
  `ActiveSessionMiddleware`; a Super Admin can't deactivate their own account.

### Email in local development

`Smtp:Host` is empty by default, so `EmailSender` logs the reset/invite email (recipient,
subject, body with the code) to the console instead of sending it, and the API response also
includes the code as `devPreviewCode`/`inviteCode` (only when `Development` + no SMTP configured
— never in production) so the frontend can show it directly without checking the terminal. Set
`Smtp:Host`/`Port`/`Username`/`Password` in `appsettings.Development.json` to send through a real
SMTP provider (e.g. Gmail) instead.

Still open: role-specific dashboards beyond the placeholder in `DashboardPage.tsx`.
