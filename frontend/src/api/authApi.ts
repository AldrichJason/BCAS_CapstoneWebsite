import { apiClient } from './client';
import type { LoginResponseDto, UserDto } from '../types/auth';

export async function login(email: string, password: string): Promise<LoginResponseDto> {
  const response = await apiClient.post<LoginResponseDto>('/auth/login', { email, password });
  return response.data;
}

export async function getSession(): Promise<UserDto> {
  const response = await apiClient.get<UserDto>('/auth/session');
  return response.data;
}

export async function logout(): Promise<void> {
  await apiClient.post('/auth/logout');
}
