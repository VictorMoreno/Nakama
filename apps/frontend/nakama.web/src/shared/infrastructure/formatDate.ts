const DATE_LOCALE = "es-ES";
const DATE_FORMAT: Intl.DateTimeFormatOptions = { day: "2-digit", month: "2-digit", year: "numeric" };
const DATE_TIME_FORMAT: Intl.DateTimeFormatOptions = { ...DATE_FORMAT, hour: "2-digit", minute: "2-digit", hour12: false };

function format(value: Date | string | undefined, options: Intl.DateTimeFormatOptions): string {
  if (!value) return "";
  const date: Date = typeof value === "string" ? new Date(value) : value;
  return Number.isNaN(date.getTime()) ? "" : date.toLocaleString(DATE_LOCALE, options);
}

export function formatDate(value: Date | string | undefined): string {
  return format(value, DATE_FORMAT);
}

export function formatDateTime(value: Date | string | undefined): string {
  return format(value, DATE_TIME_FORMAT).replace(", ", " ");
}
