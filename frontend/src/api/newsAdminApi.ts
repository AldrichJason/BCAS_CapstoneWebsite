import { apiClient } from './client';
import type { NewsDto, NewsRequest } from '../types/news';

export interface NewsAdminListParams {
  departmentId?: number;
  status?: string;
}

export async function listAllNews(params: NewsAdminListParams): Promise<NewsDto[]> {
  const response = await apiClient.get<NewsDto[]>('/admin/news', { params });
  return response.data;
}

export async function createSchoolWideNews(request: NewsRequest): Promise<NewsDto> {
  const response = await apiClient.post<NewsDto>('/admin/news', request);
  return response.data;
}

export async function updateSchoolWideNews(id: number, request: NewsRequest): Promise<NewsDto> {
  const response = await apiClient.put<NewsDto>(`/admin/news/${id}`, request);
  return response.data;
}

export async function deleteSchoolWideNews(id: number): Promise<void> {
  await apiClient.delete(`/admin/news/${id}`);
}
