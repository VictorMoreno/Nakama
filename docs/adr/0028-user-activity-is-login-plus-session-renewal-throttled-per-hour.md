# ADR-28: user activity is login plus session renewal, throttled to one entry per hour

- **Status:** Accepted
- **Area:** Users — analytics

`read_user_activity` (ADR-23) only recorded logins, and nothing read it. The admin Analytics page now shows it: the people
active today, in the last 7 days and in the last 30, and a daily chart of active people and logins over 7, 30 or 90 days
(`GET /api/analytics/user-activity?days=`). The same table is now the source of "active users this week" in
`GET /api/analytics`, so the headline figure and the chart cannot disagree; `User.LastActiveAt` stays but is no longer read
for that.

**What counts as activity.** A login alone undercounts: the session lives on a rotating refresh token, so a person who keeps
the app open for days never logs in again. `Authentication` therefore publishes `SessionRefreshed` after each successful
rotation, and `UserActivity` records a `Session` entry from it — but at most one per person per
`Reporting:UserActivity:SessionIntervalMinutes` (60). The access token lasts 15 minutes, so without the throttle an open tab would
write four rows an hour. The handler counts the person's entries since `occurredAt - interval` and saves only when there are
none. Logins are never throttled.

**Days are counted in a configured time zone.** `Reporting:TimeZone` (`Europe/Madrid`), one setting shared by every
analytics read behind `IReportingCalendar` (ADR-29, ADR-30), decides where a day starts, not UTC, so a login at 23:30
local time does not land on tomorrow's bar. Entries are bucketed in memory with `TimeZoneInfo`;
rows are zero-filled so the chart has one point per day, and daylight-saving changes are covered by a test. IANA ids on
Windows are resolved through the operating system's ICU; on Linux they need the `tzdata` package, so a slim or Alpine image
must include it (and not run with `InvariantGlobalization` on).

**Reading it.** `UserActivityFinder` loads the entries of the window (the larger of the requested days and 30, which the
summary cards need) with `UserActivityBetweenSpecification` and aggregates them in memory. At this scale that is simpler
than a grouped SQL query and keeps the time-zone logic in one testable place; revisit it if the table grows past what one
month of entries makes reasonable. The baseline creates indexes on `occurredat` and on `(userid, occurredat)` for the range
query and the throttle check.

**Limits.** `Reporting:UserActivity:DefaultDays` and `MaxDays` are mirrored in the frontend as `VITE_USER_ACTIVITY_DEFAULT_DAYS` and
`VITE_USER_ACTIVITY_DAY_OPTIONS`, and the endpoint answers 422 `nakama.user_activity.invalid_days` outside `1..MaxDays`.
A per-person "last access" list was left out on purpose: it is personal data and the page is meant for aggregate usage.

**Consequences.** Activity is recorded inside the request unit of work (ADR-27), so a failing refresh does not leave a
`Session` row behind. Activity of people who only use an access token that has not yet expired is seen at the next refresh,
at most 15 minutes late.
