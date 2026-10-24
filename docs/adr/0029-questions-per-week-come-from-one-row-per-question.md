# ADR-29: questions per week come from one row per question

- **Status:** Accepted
- **Area:** Analytics — read models

The "Preguntas por semana" chart on the Analytics page was a hard-coded series, and `Analytics.AnsweredThisWeek` could not
back it: it is a counter that only grows (it is never reset, so it is really a total) and it keeps no time. A new bounded
context, `QuestionActivity`, records **one row per question** — `read_question_activity (id, botkey, userid, occurredat)`
— fed by a handler on `ConversationExchangeAppended`, the same append-only pattern as `read_user_activity` (ADR-23, ADR-28).
Append-only rows need no read-modify-write, so two questions answered at the same time cannot lose an update, and the
data stays queryable by day, week or chatbot without a schema change. The baseline creates the table and an
index on `occurredat`.

The event had no time, so `ConversationExchangeAppended` now carries `OccurredAt` and `Conversation.AppendExchange` takes
the `ISystemClock`, as `Conversation.Start` already did. The handler stores that value instead of reading the clock itself,
so what is recorded is the moment the exchange was added.

**Reading it.** `GET /api/analytics/question-activity?weeks=` (administrators only) returns one row per week, oldest first,
with the last row being the current week, plus the questions of the current week. Weeks start on Monday and are bucketed in
`Reporting:TimeZone` (`Europe/Madrid`, the one shared with the other analytics, see below), so a question at 23:30 on a Sunday belongs to that week and not to the next
one in UTC. Empty weeks are zero-filled. `Reporting:QuestionActivity:DefaultWeeks` (8) and `MaxWeeks` (26) bound the request (422
`nakama.question_activity.invalid_weeks` outside `1..MaxWeeks`) and are mirrored in the frontend as
`VITE_QUESTION_ACTIVITY_DEFAULT_WEEKS` and `VITE_QUESTION_ACTIVITY_WEEK_OPTIONS`.

**One time zone.** Every analytics read (user activity, questions per week, per chatbot and unhelpful questions) takes
"today", the start of a local day and the Monday of a week from `IReportingCalendar`, whose only setting is
`Reporting:TimeZone`. Each module used to carry its own copy of the time zone and of the date arithmetic; with one value
the cards of the page cannot disagree on what "this week" means.

**One figure, one source.** "Preguntas respondidas" in `GET /api/analytics` now comes from the same table (the current week),
as the active users already come from `UserActivity` (ADR-28), so the card and the chart cannot disagree. The card says
"esta semana" instead of "en total".

"Preguntas resueltas por chatbot" (`usageByBot` of the same response) is read from this table too: the questions of the
current calendar month in `Reporting:TimeZone`, grouped by `botkey`, from the most to the least asked, with ties broken by
chatbot so the order is stable. Only chatbots with a question that month appear; the chart already has an empty state. The
JSON contract (`botKey`, `value`) is unchanged, so the generated client is too. The baseline no longer has the old
`answeredthisweek` and `usage_by_bot_json` columns of `read_analytics` and the aggregate no longer reacts to
`ConversationExchangeAppended`: `Analytics` keeps only the chatbot count and the feedback counters. The totals accumulated
before the table existed are lost, as with the weekly series. A deleted or archived chatbot keeps its rows, so its key
may show instead of its name until the month changes.

**Not included.** The chart had a second series, "escaladas a un senior". Nothing in the application models an escalation
and the useful/not-useful feedback is global, not tied to a question, so there is no honest way to compute it and it was
removed. There is no backfill: the series starts on the day the table exists, because past exchanges kept no time.

**Consequences.** Like every read-model handler it runs in the request unit of work (ADR-27), so a question and its row
commit together or not at all. The finder loads the rows of the window and groups them in memory, which is enough for the
range the page offers; revisit it if the table grows beyond that.
