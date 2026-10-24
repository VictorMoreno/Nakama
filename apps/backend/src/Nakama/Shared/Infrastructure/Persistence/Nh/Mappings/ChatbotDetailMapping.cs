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
        this.Table("read_chatbot_detail");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        this.Property(x => x.Label);
        this.Property(x => x.Description, m => m.Column(c => c.SqlType("text")));
        this.Property(x => x.Status);
        this.Property(x => x.Context, m => m.Column(c => c.SqlType("text")));
        this.Property(x => x.Archived);
        this.Property(x => x.IconUrl);
        this.Property(x => x.Socratic);
        this.Property(x => x.ExplicaMas);
        this.Property(x => x.Provider);
        this.Property(x => x.Model);

        this.Property(x => x.Docs, m => { m.Column(c => { c.Name("docs_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<ChatbotDocumentRow>>>(); });
        this.Property(x => x.Team, m => { m.Column(c => { c.Name("team_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<TeamMemberRow>>>(); });
    }
}
