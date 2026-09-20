import { apiClient } from './client';
import type { NewsDto, NewsRequest } from '../types/news';

export async function listNews(status?: string): Promise<NewsDto[]> {
  const response = await apiClient.get<NewsDto[]>('/news', {
    params: status ? { status } : undefined,
  });
  return response.data;
}

export async function createNews(request: NewsRequest): Promise<NewsDto> {
  const response = await apiClient.post<NewsDto>('/news', request);
  return response.data;
}

export async function updateNews(id: number, request: NewsRequest): Promise<NewsDto> {
  const response = await apiClient.put<NewsDto>(`/news/${id}`, request);
  return response.data;
}

export async function deleteNews(id: number): Promise<void> {
  await apiClient.delete(`/news/${id}`);
}
