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
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("key");
            m.Generator(Generators.Assigned);
        });

        Property(x => x.Label);
        Property(x => x.Description, m => m.Column(c => c.SqlType("text")));
        Property(x => x.Status);
        Property(x => x.LastUsed);
        Property(x => x.Context, m => m.Column(c => c.SqlType("text")));
        Property(x => x.General);
        Property(x => x.Archived);
        Property(x => x.IconUrl);
        Property(x => x.Socratic);
        Property(x => x.ExplicaMas);
        Property(x => x.Provider);

        Property(x => x.Docs, m => { m.Column(c => { c.Name("docs_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<ChatbotDocumentRow>>>(); });
        Property(x => x.Team, m => { m.Column(c => { c.Name("team_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<TeamMemberRow>>>(); });
        Property(x => x.Gaps, m => { m.Column(c => { c.Name("gaps_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<GapQuestionRow>>>(); });
        Property(x => x.TopQuestions, m => { m.Column(c => { c.Name("top_questions_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<GapQuestionRow>>>(); });
        Property(x => x.Stats, m => { m.Column(c => { c.Name("stats_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<StatTileRow>>>(); });
    }
}
