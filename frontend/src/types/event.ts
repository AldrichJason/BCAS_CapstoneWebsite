import type { ContentStatusValue } from './news';

export interface EventDto {
  id: number;
  title: string;
  description: string;
  eventStartUtc: string;
  eventEndUtc: string | null;
  venue: string | null;
  departmentId: number | null;
  departmentName: string | null;
  isSchoolWide: boolean;
  status: ContentStatusValue;
  createdByName: string | null;
  createdAt: string;
  updatedByName: string | null;
  updatedAt: string | null;
}

export interface EventRequest {
  title: string;
  description: string;
  eventStartUtc: string;
  eventEndUtc: string | null;
  venue: string;
  status: ContentStatusValue;
}
