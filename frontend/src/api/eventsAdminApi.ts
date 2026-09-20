import { apiClient } from './client';
import type { EventDto, EventRequest } from '../types/event';

export interface EventAdminListParams {
  departmentId?: number;
  status?: string;
}

export async function listAllEvents(params: EventAdminListParams): Promise<EventDto[]> {
  const response = await apiClient.get<EventDto[]>('/admin/events', { params });
  return response.data;
}

export async function createSchoolWideEvent(request: EventRequest): Promise<EventDto> {
  const response = await apiClient.post<EventDto>('/admin/events', request);
  return response.data;
}

export async function updateSchoolWideEvent(id: number, request: EventRequest): Promise<EventDto> {
  const response = await apiClient.put<EventDto>(`/admin/events/${id}`, request);
  return response.data;
}

export async function deleteSchoolWideEvent(id: number): Promise<void> {
  await apiClient.delete(`/admin/events/${id}`);
}
