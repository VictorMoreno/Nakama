export function readPositiveInt(name: string, raw: string | undefined): number {
  const value = Number(raw);
  if (!Number.isInteger(value) || value <= 0) {
    throw new Error(`Missing or invalid environment variable ${name}: expected a positive integer.`);
  }
  return value;
}

export function readList(name: string, raw: string | undefined): string[] {
  const items = (raw ?? "")
    .split(",")
    .map((item) => item.trim().toLowerCase())
    .filter((item) => item !== "");
  if (items.length === 0) {
    throw new Error(`Missing or invalid environment variable ${name}: expected a comma-separated list.`);
  }
  return items;
}
