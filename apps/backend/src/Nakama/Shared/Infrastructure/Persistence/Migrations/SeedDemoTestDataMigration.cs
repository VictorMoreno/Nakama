using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using FluentMigrator;
using Microsoft.AspNetCore.Identity;
using Nakama.Analytics.Domain;
using Nakama.ChatbotDetail.Domain;
using Nakama.Chatbots.Domain;
using Nakama.ChatPage.Domain;
using Nakama.ConversationAudit.Domain;
using Nakama.ConversationHistory.Domain;
using Nakama.FormationDetail.Domain;
using Nakama.FormationEnrollment.Domain;
using Nakama.Formations.Domain;
using Nakama.LlmProviders.Domain;
using Nakama.Users.Domain;

namespace Nakama.Shared.Infrastructure.Persistence.Migrations;

// "Analytics" collides with the sibling namespace Nakama.Analytics (same footgun as
// ChatPage/ChatbotDetail/FormationDetail/ConversationHistory elsewhere in this codebase) -
// aliased for the one bare reference to Analytics.SingletonKey below.
using Analytics = Nakama.Analytics.Domain.Analytics;

// ============================================================================================
// TEST-DATA SEED MIGRATION - NOT real/production content.
//
// Purpose: populate the app with a "considerable" (light-tier) volume of realistic-looking but
// entirely fake demo data - users, chatbots, formations, conversations - so the team can see how
// lists, pagination-adjacent UI, and the N+1-per-list-item fetch patterns (documented inline at
// ApiChatbotsRepository/ApiFormationsRepository/ApiConversationsRepository) behave under load
// heavier than the handful of records created by hand during development.
//
// Every seeded user's email lives under the @seed.nakama.test domain specifically so this batch
// is trivially identifiable later. There is no automatic Down() - see the comment above Down()
// below for the exact manual cleanup queries and why an automatic rollback isn't a good fit here.
//
// Every write-side aggregate table AND every read-model projection table it feeds is populated
// together in the same pass, reusing the exact same domain row types (and the exact same
// System.Text.Json options JsonUserType<T> uses in production) that the real application would
// have produced through its normal command/event pipeline - so the JSON shape can't drift from
// what NHibernate actually expects to read back.
[Migration(3)]
[SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded")]
[SuppressMessage("Major Vulnerability", "S2068:Credentials should not be hard-coded")]
[SuppressMessage("Major Vulnerability", "S2245:Pseudorandom number generators (PRNGs) should not be used in security contexts")]
public sealed class SeedDemoTestDataMigration : Migration
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly string[] FirstNames =
    [
        "Lucía", "Mateo", "Sofía", "Hugo", "Martina", "Daniel", "Valentina", "Pablo", "Carla", "Álvaro",
        "Noa", "Diego", "Elena", "Marco", "Paula", "Iván", "Claudia", "Rubén", "Alba", "Sergio"
    ];

    private static readonly string[] LastNames =
    [
        "García", "Martínez", "López", "Sánchez", "Pérez", "Gómez", "Fernández", "Ruiz", "Díaz", "Moreno",
        "Álvarez", "Romero", "Navarro", "Torres", "Domínguez", "Vázquez", "Ramos", "Gil", "Serrano", "Blanco"
    ];

    private static readonly string[] ChatbotTopics =
    [
        "Backend .NET", "Frontend React", "Infraestructura Cloud", "Seguridad de la Información",
        "RRHH y Onboarding", "Soporte al Cliente", "Ventas B2B", "Finanzas y Facturación",
        "Legal y Compliance", "Producto y Diseño", "Datos y Analítica", "DevOps y CI/CD",
        "Calidad y Testing", "Marketing Digital", "Atención Postventa"
    ];

    private static readonly LlmModelOption[] AnthropicModels =
    [
        new("claude-opus-5-5", "Claude Opus 5.5"),
        new("claude-sonnet-5-5", "Claude Sonnet 5.5"),
        new("claude-fable-5-1", "Claude Fable 5.1"),
        new("claude-haiku-4-5-20251001", "Claude Haiku 4.5")
    ];

    private static readonly string[] FormationTopics =
    [
        "Onboarding de Ingeniería", "Certificación en Seguridad", "Fundamentos de Cloud",
        "Buenas Prácticas de Código", "Gestión de Incidentes", "Atención al Cliente Nivel 1",
        "Ventas Consultivas", "Compliance y RGPD", "Liderazgo de Equipos", "Diseño de Producto"
    ];

    private static readonly string[] FormationAudiences =
    [
        "Todo el equipo de ingeniería", "Nuevas incorporaciones", "Equipo de soporte", "Equipo comercial",
        "Managers y leads", "Diseñadores de producto", "Equipo de datos", "Toda la empresa",
        "Equipo de operaciones", "Equipo legal"
    ];

    // Real, publicly reachable URLs (verified via web search before writing this migration, not
    // guessed) - one per FormationTopics entry, same index. The previous revision pointed every
    // formation's Document/Video module at the same two fake https://intranet.local/... links,
    // which never resolved to anything; these are genuine pages/videos from real organizations,
    // topically matched, so "Abrir documento"/"Ver vídeo" actually opens something real.
    private static readonly string[] FormationDocUrls =
    [
        "https://handbook.gitlab.com/handbook/engineering/development/onboarding/", // Onboarding de Ingeniería
        "https://owasp.org/www-project-top-ten/", // Certificación en Seguridad
        "https://aws.amazon.com/getting-started/", // Fundamentos de Cloud
        "https://google.github.io/styleguide/", // Buenas Prácticas de Código
        "https://www.atlassian.com/incident-management/handbook", // Gestión de Incidentes
        "https://www.zendesk.com/", // Atención al Cliente Nivel 1
        "https://blog.hubspot.com/sales", // Ventas Consultivas
        "https://gdpr.eu/", // Compliance y RGPD
        "https://www.gallup.com/services/176708/leadership.aspx", // Liderazgo de Equipos
        "https://www.nngroup.com/articles/" // Diseño de Producto
    ];

    private static readonly string[] FormationVideoUrls =
    [
        "https://www.youtube.com/@freecodecamp/featured", // Onboarding de Ingeniería
        "https://www.professormesser.com/sy0-401/", // Certificación en Seguridad (free video course)
        "https://aws.amazon.com/training/digital/", // Fundamentos de Cloud (AWS Skill Builder)
        "https://www.youtube.com/@freecodecamp/featured", // Buenas Prácticas de Código
        "https://www.atlassian.com/incident-management/tutorials", // Gestión de Incidentes
        "https://www.zendesk.com/blog/customer-service-videos/", // Atención al Cliente Nivel 1
        "https://academy.hubspot.com/", // Ventas Consultivas
        "https://20years.edps.europa.eu/en/initiatives/data-protection-explained", // Compliance y RGPD (EDPS)
        "https://www.ted.com/talks/simon_sinek_how_great_leaders_inspire_action", // Liderazgo de Equipos
        "https://www.nngroup.com/videos/focus-results-not-perfect-ux-don-norman/" // Diseño de Producto
    ];

    public override void Up()
    {
        Random rng = new(20260930);
        DateTime now = DateTime.UtcNow;

        this.Insert.IntoTable("llm_provider_configs").Row(new
        {
            id = Guid.NewGuid(),
            provider = (int)LlmProvider.Anthropic,
            api_key = "",
            enabled = false,
            models_json = Json(AnthropicModels.ToList()),
            updated_at = now
        });

        // ---- Users --------------------------------------------------------------------------
        PasswordHasher<User> hasher = new();
        const string seedPassword = "Seed1234.";
        List<Guid> userIds = new();
        List<(Guid id, Guid userId, string type, DateTime occurredAt)> userActivityRows = new();

        for (int i = 0; i < 20; i++)
        {
            string first = FirstNames[i];
            string last = LastNames[rng.Next(LastNames.Length)];
            string email = $"{Normalize(first)}.{Normalize(last)}@seed.nakama.test";
            bool isAdmin = i < 2;
            DateTime createdAt = now.AddDays(-rng.Next(30, 400));

            User user = User.Create(UserId.From(Guid.NewGuid()), new Email(email), $"{first} {last}", isAdmin,
                new SystemClock());
            userIds.Add(user.Id.Value);

            int loginCount = rng.Next(1, 4);
            DateTime? lastActiveAt = null;
            for (int l = 0; l < loginCount; l++)
            {
                DateTime occurredAt = now.AddDays(-rng.Next(0, 14)).AddHours(-rng.Next(0, 24));
                if (lastActiveAt is null || occurredAt > lastActiveAt) lastActiveAt = occurredAt;
                userActivityRows.Add((Guid.NewGuid(), user.Id.Value, "Login", occurredAt));
            }

            this.Insert.IntoTable("users").Row(new
            {
                id = user.Id.Value,
                email = user.Email,
                normalizedemail = user.Email.ToUpperInvariant(),
                name = user.Name,
                isadmin = user.IsAdmin,
                passwordhash = hasher.HashPassword(user, seedPassword),
                lastactiveat = (object?)lastActiveAt ?? DBNull.Value,
                createdat = createdAt
            });
        }

        foreach ((Guid id, Guid userId, string type, DateTime occurredAt) in userActivityRows)
        {
            this.Insert.IntoTable("read_user_activity").Row(new { id, userid = userId, type, occurredat = occurredAt });
        }

        List<Guid> conversationAuthorIds = new(userIds)
        {
            Guid.Parse("48D7EAD2-BAB5-468C-B7C5-CEF4D4A3842F"), // admin@nakama.com
            Guid.Parse("EFB5209F-13C5-40B4-8E72-35AFA54CBEF8"), // user@nakama.com
        };

        // ---- Chatbots -------------------------------------------------------------------------
        List<Guid> chatbotIds = new();
        List<UsageByBotRow> usageByBot = new();
        int totalExchanges = 0;

        for (int i = 0; i < ChatbotTopics.Length; i++)
        {
            string topic = ChatbotTopics[i];
            string label = $"{topic} Bot";
            Guid botId = Guid.NewGuid();
            chatbotIds.Add(botId);

            ChatbotStatus status = (ChatbotStatus)(rng.Next(0, 10) < 7 ? 1 : rng.Next(0, 2) == 0 ? 0 : 2); // mostly Active
            LlmProvider seedProvider = (LlmProvider)rng.Next(0, 3);
            string seedModel = seedProvider == LlmProvider.Anthropic ? AnthropicModels[rng.Next(AnthropicModels.Length)].Id : string.Empty;
            ChatbotSettings settings = new(Socratic: rng.Next(0, 2) == 0, ExplicaMas: rng.Next(0, 2) == 0,
                Provider: seedProvider, Model: seedModel);
            string context =
                $"Chatbot entrenado con la documentación interna de {topic}. Datos de prueba generados por SeedDemoTestDataMigration.";
            string description = $"Resuelve dudas frecuentes sobre {topic}.";
            DateTime lastUsed = now.AddDays(-rng.Next(0, 30));

            // Team: 1-2 members from the seeded user pool
            int teamSize = rng.Next(1, 3);
            List<Guid> teamUserIds = PickDistinct(rng, userIds, teamSize);
            List<TeamMember> writeTeam = teamUserIds
                .Select((uid, idx) => new TeamMember(uid, idx == 0 ? TeamRole.Owner : TeamRole.Editor)).ToList();
            List<TeamMemberRow> detailTeam = writeTeam.Select(m => new TeamMemberRow { UserId = m.UserId, Role = m.Role.ToString() })
                .ToList();

            // Documents: 2 metadata rows per bot - no real file on disk, status always Pending
            // (matches ChatbotDocumentAdder - this app has no real indexing pipeline yet).
            List<ChatbotDocument> docs = new();
            for (int d = 0; d < 2; d++)
            {
                docs.Add(new ChatbotDocument(Guid.NewGuid(), $"Guía de {topic} #{d + 1}.pdf", "Seed", DocStatus.Pending,
                    "Pendiente",
                    now.AddDays(-rng.Next(1, 60)), $"seed-{botId}-{d}.pdf"));
            }

            List<ChatbotDocumentRow> detailDocs = docs.Select(doc => new ChatbotDocumentRow
            {
                Id = doc.Id, Name = doc.Name, Origin = doc.Origin, Status = doc.Status.ToString(),
                StatusLabel = doc.StatusLabel, Updated = doc.Updated, FileName = doc.FileName
            }).ToList();

            ChatExampleRow opening = new()
            {
                Question = $"¿Cómo empiezo con {topic}?",
                Answer =
                    $"Te recomiendo arrancar por la guía de {topic} en la documentación interna. Si te trabás en algún paso, preguntame acá mismo.",
                Citation = $"Guía de {topic} #1.pdf",
                MoreExplanation = $"El equipo de {topic} mantiene esta documentación actualizada semanalmente.",
                SocraticVariant = $"¿Qué parte de {topic} te genera más dudas hoy?"
            };

            this.Insert.IntoTable("chatbots").Row(new
            {
                id = botId,
                label,
                description,
                status = (int)status,
                context,
                archived = false,
                iconurl = (string?)null,
                settings = Json(settings),
                lastused = lastUsed,
                documents_json = Json(docs),
                team_json = Json(writeTeam)
            });

            this.Insert.IntoTable("read_chatbot_summaries").Row(new
            {
                id = botId,
                label,
                description,
                status = status.ToString(),
                docscount = docs.Count,
                teamcount = writeTeam.Count,
                archived = false,
                iconurl = (string?)null
            });

            this.Insert.IntoTable("read_chatbot_detail").Row(new
            {
                id = botId,
                label,
                description,
                status = status.ToString(),
                lastused = lastUsed,
                context,
                archived = false,
                iconurl = (string?)null,
                socratic = settings.Socratic,
                explicamas = settings.ExplicaMas,
                provider = settings.Provider.ToString(),
                model = settings.Model,
                docs_json = Json(detailDocs),
                team_json = Json(detailTeam),
                gaps_json = Json(new List<GapQuestionRow>
                {
                    new() { Question = $"¿Cómo escalo un incidente de {topic}?", Count = rng.Next(2, 15) },
                    new() { Question = $"¿Cuál es el SLA de {topic}?", Count = rng.Next(1, 10) }
                }),
                top_questions_json = Json(new List<GapQuestionRow>
                {
                    new() { Question = $"¿Cómo empiezo con {topic}?", Count = rng.Next(10, 60) },
                    new() { Question = $"¿Dónde encuentro la documentación de {topic}?", Count = rng.Next(5, 40) }
                }),
                stats_json = Json(new List<StatTileRow>
                {
                    new()
                    {
                        Label = "Tiempo hasta productividad", Value = $"{rng.Next(1, 5)} días",
                        Unit = "objetivo <3 días", Bar = rng.Next(30, 95)
                    },
                    new()
                    {
                        Label = "Preguntas directas a seniors / semana", Value = rng.Next(1, 10).ToString(),
                        Unit = "antes vs ahora", Bar = rng.Next(20, 80)
                    },
                    new()
                    {
                        Label = "Respuestas útiles del bot", Value = $"{rng.Next(60, 98)}%",
                        Unit = "sobre el total de feedback", Bar = rng.Next(60, 98)
                    },
                    new()
                    {
                        Label = "Documentos ingestados", Value = docs.Count.ToString(), Unit = "objetivo >20",
                        Bar = Math.Min(100, docs.Count * 5)
                    }
                })
            });

            this.Insert.IntoTable("read_chat_page").Row(new
            {
                id = botId,
                label,
                context,
                socratic = settings.Socratic,
                explicamas = settings.ExplicaMas,
                provider = settings.Provider.ToString(),
                opening_json = Json(opening)
            });

            // ---- Conversations for this bot (write + audit + history) -------------------------
            int conversationCount = rng.Next(3, 8);
            List<ConversationSummaryRow> historyEntries = new();

            for (int c = 0; c < conversationCount; c++)
            {
                Guid convId = Guid.NewGuid();
                Guid author = (i < 3, c) switch
                {
                    (true, 0) => Guid.Parse("48D7EAD2-BAB5-468C-B7C5-CEF4D4A3842F"), // admin@nakama.com
                    (true, 1) => Guid.Parse("EFB5209F-13C5-40B4-8E72-35AFA54CBEF8"), // user@nakama.com
                    _ => conversationAuthorIds[rng.Next(conversationAuthorIds.Count)]
                };
                string title = $"Duda sobre {topic.ToLowerInvariant()} #{c + 1}";
                DateTime startedAt = now.AddDays(-rng.Next(0, 60)).AddHours(-rng.Next(0, 24));

                int exchangeCount = rng.Next(2, 5);
                List<ConversationExchange> exchanges = new();
                List<AuditMessageRow> auditMessages = new();
                List<HistoryExampleRow> historyExchanges = new();

                for (int e = 0; e < exchangeCount; e++)
                {
                    (string q, string a) = QuestionAnswer(topic, e, rng);
                    string citation = $"Guía de {topic} #{(e % 2) + 1}.pdf";
                    string moreExplanation = $"Fuente: documentación interna de {topic}, revisada por el equipo.";
                    string socraticVariant = $"¿Qué probaste hasta ahora con {topic.ToLowerInvariant()}?";
                    DateTime recordedAt = startedAt.AddMinutes(e * rng.Next(2, 15));

                    exchanges.Add(new ConversationExchange(q, a, citation, moreExplanation, socraticVariant));
                    auditMessages.Add(new AuditMessageRow
                    {
                        Question = q, Answer = a, Citation = citation, MoreExplanation = moreExplanation,
                        SocraticVariant = socraticVariant, RecordedAt = recordedAt
                    });
                    historyExchanges.Add(new HistoryExampleRow
                    {
                        Question = q, Answer = a, Citation = citation, MoreExplanation = moreExplanation,
                        SocraticVariant = socraticVariant
                    });
                }

                totalExchanges += exchangeCount;

                this.Insert.IntoTable("conversations").Row(new
                {
                    id = convId,
                    bot_key = botId,
                    user_id = author,
                    title,
                    startedat = startedAt,
                    exchanges_json = Json(exchanges)
                });

                this.Insert.IntoTable("read_conversation_audit").Row(new
                {
                    id = convId,
                    user_id = author,
                    bot_key = botId,
                    title,
                    startedat = startedAt,
                    messages_json = Json(auditMessages)
                });

                historyEntries.Add(new ConversationSummaryRow
                {
                    Id = convId, BotKey = botId, UserId = author, Title = title, DateLabel = startedAt,
                    Exchanges = historyExchanges
                });
            }

            this.Insert.IntoTable("read_conversation_history").Row(new { id = botId, entries_json = Json(historyEntries) });

            usageByBot.Add(new UsageByBotRow { BotKey = botId, Value = historyEntries.Sum(h => h.Exchanges.Count) });
        }

        // ---- Formations -------------------------------------------------------------------------
        for (int i = 0; i < FormationTopics.Length; i++)
        {
            string topic = FormationTopics[i];
            Guid formId = Guid.NewGuid();
            FormationStatus status = (FormationStatus)(rng.Next(0, 10) < 7 ? 1 : rng.Next(0, 2) == 0 ? 0 : 2);
            string audience = FormationAudiences[i];
            string description = $"Currículo de formación sobre {topic.ToLowerInvariant()}.";
            Guid? linkedChatbotKey = i < 6 ? chatbotIds[i] : (Guid?)null;
            string? linkedChatbotLabel = linkedChatbotKey is not null ? $"{ChatbotTopics[i]} Bot" : null;

            ModuleId moduleId1 = ModuleId.From(Guid.NewGuid());
            ModuleId moduleId2 = ModuleId.From(Guid.NewGuid());
            ModuleId moduleId3 = ModuleId.From(Guid.NewGuid());
            List<QuizQuestion> quiz = new()
            {
                new($"¿Cuál es el primer paso al abordar {topic.ToLowerInvariant()}?",
                [
                    "Leer la guía introductoria", "Improvisar", "Preguntar en el canal general",
                    "Ignorar la documentación"
                ], 0),
                new($"¿Dónde se documenta el proceso de {topic.ToLowerInvariant()}?",
                [
                    "En un chat privado", "En la documentación interna del equipo", "No está documentado",
                    "En un email suelto"
                ], 1)
            };
            List<FormationModule> modules = new()
            {
                new(moduleId1, $"Introducción a {topic}", ModuleType.Document,
                    $"Documento base sobre {topic.ToLowerInvariant()}.", Gated: true, AiDraft: false,
                    ResourceUrl: FormationDocUrls[i]),
                new(moduleId2, $"Vídeo: recorrido por {topic}", ModuleType.Video,
                    $"Recorrido guiado en vídeo sobre {topic.ToLowerInvariant()}.", Gated: true, AiDraft: false,
                    ResourceUrl: FormationVideoUrls[i], DurationMin: rng.Next(8, 25)),
                new(moduleId3, $"Examen de {topic}", ModuleType.Exam,
                    $"Evaluación final de {topic.ToLowerInvariant()}.", Gated: false, AiDraft: false,
                    Quiz: quiz, PassingScore: 70)
            };
            List<FormationModuleRow> detailModules = modules.Select(m => new FormationModuleRow
            {
                Id = m.Id.Value, Title = m.Title, Type = m.Type.ToString(), Summary = m.Summary, Gated = m.Gated,
                AiDraft = m.AiDraft,
                ResourceUrl = m.ResourceUrl, DurationMin = m.DurationMin, PassingScore = m.PassingScore,
                Quiz = m.Quiz?.Select(q => new QuizQuestionRow
                    { Question = q.Question, Options = q.Options.ToList(), CorrectIndex = q.CorrectIndex }).ToList()
            }).ToList();

            // Each team member gets their OWN progress now (real FormationEnrollment rows) instead
            // of a single shared blob on the formation - this is the bug the whole feature fixes.
            int teamSize = rng.Next(2, 6);
            List<Guid> teamUserIds = PickDistinct(rng, userIds, teamSize);
            Guid[] moduleIds = new[] { moduleId1.Value, moduleId2.Value, moduleId3.Value };
            List<(Guid UserId, LearnerProgress Progress)> enrollments = teamUserIds.Select(uid =>
            {
                int completedCount = rng.Next(0, modules.Count + 1);
                LearnerProgress progress = moduleIds.Take(completedCount)
                    .Aggregate(LearnerProgress.Empty, (p, moduleId) => p.WithModuleCompleted(moduleId, null));
                return (UserId: uid, Progress: progress);
            }).ToList();
            List<FormationMemberRow> detailTeam = enrollments
                .Select(e => new FormationMemberRow
                    { UserId = e.UserId, Progress = e.Progress.CompletedModuleIds.Count * 100 / modules.Count })
                .ToList();

            this.Insert.IntoTable("formations").Row(new
            {
                id = formId,
                title = topic,
                audience,
                description,
                status = (int)status,
                linked_chatbot_key = (object?)linkedChatbotKey ?? DBNull.Value,
                archived = false,
                iconurl = (string?)null,
                modules_json = Json(modules)
            });

            foreach ((Guid UserId, LearnerProgress Progress) enrollment in enrollments)
            {
                this.Insert.IntoTable("formation_enrollments").Row(new
                {
                    id = Guid.NewGuid(),
                    formation_id = formId,
                    user_id = enrollment.UserId,
                    progress_json = Json(enrollment.Progress)
                });
            }

            this.Insert.IntoTable("read_formations_list").Row(new
            {
                id = formId,
                title = topic,
                audience,
                description,
                status = status.ToString(),
                archived = false,
                iconurl = (string?)null,
                modulescount = modules.Count
            });

            this.Insert.IntoTable("read_formation_detail").Row(new
            {
                id = formId,
                title = topic,
                audience,
                description,
                status = status.ToString(),
                archived = false,
                iconurl = (string?)null,
                linked_chatbot_key = (object?)linkedChatbotKey ?? DBNull.Value,
                linked_chatbot_label = (object?)linkedChatbotLabel ?? DBNull.Value,
                modules_json = Json(detailModules),
                team_json = Json(detailTeam)
            });

            if (linkedChatbotKey is not null)
            {
                this.Insert.IntoTable("read_formation_detail_chatbot_label_ref")
                    .Row(new { id = linkedChatbotKey.Value, label = linkedChatbotLabel });
            }
        }

        // ---- Analytics (singleton "global" row) - additive upsert -----------------------------
        // Raw upsert instead of Insert.IntoTable: a real "global" row may already exist from
        // genuine app usage before this migration ran, and blindly inserting would violate its
        // primary key. ON CONFLICT adds this batch's counts on top of whatever is already there,
        // and concatenates usage_by_bot_json as jsonb (safe here since every seeded botKey is a
        // freshly generated Guid, so it can never duplicate an existing array entry).
        int totalFeedback = (int)(totalExchanges * 0.4);
        int usefulFeedback = (int)(totalFeedback * 0.75);
        string usageByBotJson = Json(usageByBot);

        this.Execute.WithConnection((connection, transaction) =>
        {
            using IDbCommand cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = """
                              INSERT INTO read_analytics (id, activebotscount, answeredthisweek, usage_by_bot_json, usefulfeedbackcount, totalfeedbackcount)
                              VALUES (@id, @activebotscount, @answeredthisweek, @usage_by_bot_json, @usefulfeedbackcount, @totalfeedbackcount)
                              ON CONFLICT (id) DO UPDATE SET
                                  activebotscount = read_analytics.activebotscount + EXCLUDED.activebotscount,
                                  answeredthisweek = read_analytics.answeredthisweek + EXCLUDED.answeredthisweek,
                                  usage_by_bot_json = (COALESCE(read_analytics.usage_by_bot_json, '[]')::jsonb || EXCLUDED.usage_by_bot_json::jsonb)::text,
                                  usefulfeedbackcount = read_analytics.usefulfeedbackcount + EXCLUDED.usefulfeedbackcount,
                                  totalfeedbackcount = read_analytics.totalfeedbackcount + EXCLUDED.totalfeedbackcount
                              """;

            AddParam(cmd, "@id", Analytics.SingletonKey);
            AddParam(cmd, "@activebotscount", chatbotIds.Count);
            AddParam(cmd, "@answeredthisweek", totalExchanges);
            AddParam(cmd, "@usage_by_bot_json", usageByBotJson);
            AddParam(cmd, "@usefulfeedbackcount", usefulFeedback);
            AddParam(cmd, "@totalfeedbackcount", totalFeedback);

            cmd.ExecuteNonQuery();
        });
    }

    // No down migration - matches InitialSchemaMigration/SeedUsersMigration's own convention
    // (ADR-19: MigrationRunner.Run only ever executes MigrateUp() in Development, so a real
    // rollback path is never exercised by the app itself). It's a deliberately worse fit here
    // too: Up() additively upserts read_analytics.usage_by_bot_json on top of whatever numbers
    // real app usage already produced, so a symmetrical Down() would have to know how to
    // subtract this batch's contribution back out rather than just delete the row - not worth
    // the complexity for disposable seed data. To remove this batch by hand instead:
    //   delete from read_conversation_history;
    //   delete from read_conversation_audit;
    //   delete from conversations;
    //   delete from read_formation_detail_chatbot_label_ref;
    //   delete from read_formation_detail;
    //   delete from read_formations_list;
    //   delete from formations;
    //   delete from read_chat_page;
    //   delete from read_chatbot_detail;
    //   delete from read_chatbot_summaries;
    //   delete from chatbots;
    //   delete from read_user_activity where userid in (select id from users where email like '%@seed.nakama.test');
    //   delete from users where email like '%@seed.nakama.test';
    // (read_analytics is left alone - subtract this migration's own contribution by hand if needed.)
    public override void Down() => throw new NotSupportedException("No down migrations - see ADR entry.");

    private static string Normalize(string value) => value
        .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n")
        .ToLowerInvariant();

    private static string Json<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static List<Guid> PickDistinct(Random rng, List<Guid> pool, int count)
    {
        return pool.OrderBy(_ => rng.Next()).Take(Math.Min(count, pool.Count)).ToList();
    }

    private static void AddParam(System.Data.IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }

    private static (string Question, string Answer) QuestionAnswer(string topic, int index, Random rng)
    {
        string lower = topic.ToLowerInvariant();
        (string Q, string A)[] templates = new (string Q, string A)[]
        {
            ($"¿Cómo configuro el entorno de {lower}?",
                $"Seguí los pasos de la guía de {topic}: primero cloná el repositorio, después instalá las dependencias y por último corré el setup inicial."),
            ($"¿Cuál es el proceso para solicitar acceso a {lower}?",
                $"Completá el formulario de acceso a {topic} y el equipo lo aprueba habitualmente en 24-48h."),
            ($"¿Cómo reporto un incidente relacionado con {lower}?",
                $"Abrí un ticket en el canal de {topic} con el detalle del problema; si es urgente, escalalo directamente al equipo."),
            ($"¿Dónde está la documentación actualizada de {lower}?",
                $"En el repositorio interno, dentro de la carpeta de {topic}. Se revisa y actualiza cada sprint."),
            ($"¿Qué hago si algo de {lower} no funciona como esperaba?",
                $"Revisá primero la sección de errores comunes de {topic}; si no aparece tu caso, contactá al equipo responsable.")
        };
        (string q, string a) = templates[(index + rng.Next(0, templates.Length)) % templates.Length];
        return (q, a);
    }
}
