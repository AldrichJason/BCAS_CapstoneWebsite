export type ContentStatusValue = 'Draft' | 'Scheduled' | 'Published' | 'Archived';

export const CONTENT_STATUSES: ContentStatusValue[] = ['Draft', 'Scheduled', 'Published', 'Archived'];

export interface NewsDto {
  id: number;
  title: string;
  body: string;
  departmentId: number | null;
  departmentName: string | null;
  isSchoolWide: boolean;
  status: ContentStatusValue;
  publishAtUtc: string | null;
  createdByName: string | null;
  createdAt: string;
  updatedByName: string | null;
  updatedAt: string | null;
}

export interface NewsRequest {
  title: string;
  body: string;
  publishAtUtc: string;
  status: ContentStatusValue;
}
