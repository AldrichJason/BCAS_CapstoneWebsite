import { apiClient } from './client';
import type { DepartmentDto } from '../types/department';

export async function listDepartments(): Promise<DepartmentDto[]> {
  const response = await apiClient.get<DepartmentDto[]>('/departments');
  return response.data;
}
