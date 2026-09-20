import { apiClient } from './client';
import type { EventDto, EventRequest } from '../types/event';

export async function listEvents(status?: string): Promise<EventDto[]> {
  const response = await apiClient.get<EventDto[]>('/events', {
    params: status ? { status } : undefined,
  });
  return response.data;
}

export async function createEvent(request: EventRequest): Promise<EventDto> {
  const response = await apiClient.post<EventDto>('/events', request);
  return response.data;
}

export async function updateEvent(id: number, request: EventRequest): Promise<EventDto> {
  const response = await apiClient.put<EventDto>(`/events/${id}`, request);
  return response.data;
}

export async function deleteEvent(id: number): Promise<void> {
  await apiClient.delete(`/events/${id}`);
}
