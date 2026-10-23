import { readPositiveInt, readPositiveIntList } from "./EnvReaders";

const dayOptions = readPositiveIntList("VITE_USER_ACTIVITY_DAY_OPTIONS", import.meta.env.VITE_USER_ACTIVITY_DAY_OPTIONS);
const defaultDays = readPositiveInt("VITE_USER_ACTIVITY_DEFAULT_DAYS", import.meta.env.VITE_USER_ACTIVITY_DEFAULT_DAYS);

if (!dayOptions.includes(defaultDays)) {
  throw new Error("Invalid environment variable VITE_USER_ACTIVITY_DEFAULT_DAYS: it must be one of VITE_USER_ACTIVITY_DAY_OPTIONS.");
}

export const userActivityConfig = { defaultDays, dayOptions } as const;
