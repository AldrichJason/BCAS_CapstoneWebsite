import type { ContentStatusValue } from './news';

export interface AnnouncementDto {
  id: number;
  title: string;
  body: string;
  departmentId: number | null;
  departmentName: string | null;
  status: ContentStatusValue;
  effectiveDateUtc: string | null;
  createdByName: string | null;
  createdAt: string;
  updatedByName: string | null;
  updatedAt: string | null;
}

export interface AnnouncementRequest {
  title: string;
  body: string;
  effectiveDateUtc: string;
  status: ContentStatusValue;
}
