import type { ContentStatusValue } from '../types/news';

const STATUS_STYLES: Record<ContentStatusValue, string> = {
  Draft: 'bg-neutral-100 text-neutral-600',
  Scheduled: 'bg-accent-tint text-accent-dark',
  Published: 'bg-primary-tint text-primary',
  Archived: 'bg-neutral-200 text-neutral-500',
};

export function StatusBadge({ status }: { status: ContentStatusValue }) {
  return (
    <span className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${STATUS_STYLES[status]}`}>
      {status}
    </span>
  );
}
