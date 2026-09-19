export interface AdminAccountDto {
  id: number;
  firstName: string;
  lastName: string;
  username: string;
  email: string;
  role: string;
  departmentId: number | null;
  departmentName: string | null;
  isActive: boolean;
  createdAt: string;
  /** Local-dev-only convenience: present when no SMTP server is configured. */
  inviteCode?: string | null;
}

export interface CreateAccountRequestDto {
  firstName: string;
  lastName: string;
  username: string;
  email: string;
  role: string;
  departmentId: number | null;
  /** Optional: leave both blank to email an invite code instead. */
  password?: string;
  confirmPassword?: string;
}

export const ROLE_OPTIONS = [
  { value: 'SuperAdmin', label: 'Super Admin' },
  { value: 'AcademicHead', label: 'Academic Head' },
  { value: 'AdminOfficeRegistrar', label: 'Admin Office / Registrar' },
  { value: 'VpOfOperations', label: 'VP of Operations' },
] as const;
