import { apiClient } from './client';
import type { AnnouncementDto, AnnouncementRequest } from '../types/announcement';

export interface AnnouncementAdminListParams {
  departmentId?: number;
  status?: string;
}

export async function listAllAnnouncements(params: AnnouncementAdminListParams): Promise<AnnouncementDto[]> {
  const response = await apiClient.get<AnnouncementDto[]>('/admin/announcements', { params });
  return response.data;
}

export async function createSchoolWideAnnouncement(request: AnnouncementRequest): Promise<AnnouncementDto> {
  const response = await apiClient.post<AnnouncementDto>('/admin/announcements', request);
  return response.data;
}

export async function updateSchoolWideAnnouncement(id: number, request: AnnouncementRequest): Promise<AnnouncementDto> {
  const response = await apiClient.put<AnnouncementDto>(`/admin/announcements/${id}`, request);
  return response.data;
}

export async function deleteSchoolWideAnnouncement(id: number): Promise<void> {
  await apiClient.delete(`/admin/announcements/${id}`);
}
