import { apiClient } from './client';
import type { AnnouncementDto, AnnouncementRequest } from '../types/announcement';

export async function listAnnouncements(status?: string, sort: 'asc' | 'desc' = 'desc'): Promise<AnnouncementDto[]> {
  const response = await apiClient.get<AnnouncementDto[]>('/announcements', {
    params: { status: status || undefined, sort },
  });
  return response.data;
}

export async function createAnnouncement(request: AnnouncementRequest): Promise<AnnouncementDto> {
  const response = await apiClient.post<AnnouncementDto>('/announcements', request);
  return response.data;
}

export async function updateAnnouncement(id: number, request: AnnouncementRequest): Promise<AnnouncementDto> {
  const response = await apiClient.put<AnnouncementDto>(`/announcements/${id}`, request);
  return response.data;
}

export async function deleteAnnouncement(id: number): Promise<void> {
  await apiClient.delete(`/announcements/${id}`);
}
