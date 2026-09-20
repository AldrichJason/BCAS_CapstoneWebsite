import { apiClient } from './client';
import type { AdminAccountDto, CreateAccountRequestDto } from '../types/admin';

export async function listAccounts(): Promise<AdminAccountDto[]> {
  const response = await apiClient.get<AdminAccountDto[]>('/admin/accounts');
  return response.data;
}

export async function createAccount(request: CreateAccountRequestDto): Promise<AdminAccountDto> {
  const response = await apiClient.post<AdminAccountDto>('/admin/accounts', request);
  return response.data;
}

export async function setAccountActive(id: number, isActive: boolean): Promise<void> {
  await apiClient.patch(`/admin/accounts/${id}/status`, { isActive });
}
