# ADR-30: the rating of an answer is kept per exchange, and the analytics are read from it

- **Status:** Accepted
- **Area:** Chatbots / Analytics — feedback

The thumbs up/down of a chat answer lived in React state and went to `POST /api/analytics/feedback` as a bare boolean that
two global counters of `Analytics` added up. Nothing tied a rating to a question or an answer, so reloading the chat showed
the buttons unmarked (and let the person vote again), and the page could not tell which questions had gone badly: the block
"Preguntas sin buena respuesta" read `ChatbotDetail.Gaps`, a field nothing ever wrote.

**The rating belongs to the exchange.** `ConversationExchange` gains `bool? Useful` (null means not rated; old JSON rows load
unchanged, as with `Attachments`) and `Conversation.RateExchange(index, useful)` sets it and raises
`ConversationExchangeRated`. The command `ConversationExchangeRater` loads the conversation through
`ConversationFinderById`, so only its owner can rate it, and rejects a conversation of another chatbot or an index that does
not exist (404 `nakama.conversations.exchange.not_found`). The endpoint is
`PUT /api/chatbots/{botKey}/conversations/{conversationId}/exchanges/{exchangeIndex}/feedback` with `{ useful }`, under the
same `ChatbotMember` policy as asking. It is idempotent and **the last rating wins**, so a person can change their mind.
`POST /api/analytics/feedback` is removed.

**Reloading the chat.** The conversation history read model keeps `Useful` on each exchange row
(`ConversationHistoryUpdatedOnConversationExchangeRated`), and the frontend derives the pressed button from that value
instead of local state. The buttons stay disabled until the answer has finished streaming, because the exchange does not
exist before it is persisted. The frontend updates the thread optimistically and undoes it with a toast if the call fails.

**Analytics.** A new context, `AnswerFeedback`, keeps `read_answer_feedback`: one row per rated exchange, with the chatbot,
the question text, the last rating and its time. Its id is a hash of the conversation id and the exchange index, so rating
again updates the row instead of adding one, and the question is copied so the row survives deleting the conversation. No
person is stored. Two queries read it:

- `AnswerFeedbackSummaryFinder`: useful and total ratings, which now feed `usefulFeedbackCount` and `totalFeedbackCount` of
  `GET /api/analytics`. The two counters of the `Analytics` aggregate are gone (migration 5 drops the columns), so the useful
  rate has a single source and changing a vote moves the rate instead of counting twice.
- `UnhelpfulQuestionsFinder`: `GET /api/analytics/unhelpful-questions` (administrators), the questions rated as not useful
  since Monday of the current week in `Reporting:TimeZone`, grouped by chatbot and question ignoring case and
  surrounding spaces, most repeated first, capped by `Reporting:AnswerFeedback:UnhelpfulQuestionsLimit` (10).

**Consequences.** The useful/not-useful totals accumulated before this change are lost: they had no exchange to attach to.
The list shows question text, which is what the page was always meant to show; it is aggregated and carries no author. The
per-chatbot analytics tab still reads `ChatbotDetail.Gaps`, which stays empty until it is fed from this table or removed.
