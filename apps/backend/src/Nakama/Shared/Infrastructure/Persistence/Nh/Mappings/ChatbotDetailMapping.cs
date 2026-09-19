using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

public sealed class ChatbotDetailMapping : ClassMapping<ChatbotDetail>
{
    public ChatbotDetailMapping()
    {
        Table("read_chatbot_detail");

        Id(x => x.Id, m =>
        {
            m.Column("key");
            m.Generator(Generators.Assigned);
        });

        Property(x => x.Label);
        Property(x => x.Description);
        Property(x => x.Status);
        Property(x => x.LastUsed);
        Property(x => x.Context);
        Property(x => x.General);
        Property(x => x.Archived);
        Property(x => x.IconUrl);
        Property(x => x.Socratic);
        Property(x => x.ExplicaMas);
        Property(x => x.Provider);

        Property(x => x.Docs, m => { m.Column("docs_json"); m.Type<JsonUserType<List<ChatbotDocumentRow>>>(); });
        Property(x => x.Team, m => { m.Column("team_json"); m.Type<JsonUserType<List<TeamMemberRow>>>(); });
        Property(x => x.Gaps, m => { m.Column("gaps_json"); m.Type<JsonUserType<List<GapQuestionRow>>>(); });
        Property(x => x.TopQuestions, m => { m.Column("top_questions_json"); m.Type<JsonUserType<List<GapQuestionRow>>>(); });
        Property(x => x.Stats, m => { m.Column("stats_json"); m.Type<JsonUserType<List<StatTileRow>>>(); });
    }
}
