import { readPositiveInt, readPositiveIntList } from "./EnvReaders";

const weekOptions = readPositiveIntList("VITE_QUESTION_ACTIVITY_WEEK_OPTIONS", import.meta.env.VITE_QUESTION_ACTIVITY_WEEK_OPTIONS);
const defaultWeeks = readPositiveInt("VITE_QUESTION_ACTIVITY_DEFAULT_WEEKS", import.meta.env.VITE_QUESTION_ACTIVITY_DEFAULT_WEEKS);

if (!weekOptions.includes(defaultWeeks)) {
  throw new Error("Invalid environment variable VITE_QUESTION_ACTIVITY_DEFAULT_WEEKS: it must be one of VITE_QUESTION_ACTIVITY_WEEK_OPTIONS.");
}

export const questionActivityConfig = { defaultWeeks, weekOptions } as const;
