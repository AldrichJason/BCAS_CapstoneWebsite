export interface AdminAccountDto {
  id: number;
  fullName: string;
  email: string;
  role: string;
  departmentId: number | null;
  departmentName: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface CreateAccountRequestDto {
  fullName: string;
  email: string;
  role: string;
  departmentId: number | null;
}

export const ROLE_OPTIONS = [
  { value: 'SuperAdmin', label: 'Super Admin' },
  { value: 'AcademicHead', label: 'Academic Head' },
  { value: 'AdminOfficeRegistrar', label: 'Admin Office / Registrar' },
  { value: 'VpOfOperations', label: 'VP of Operations' },
] as const;
