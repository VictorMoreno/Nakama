using Nakama.FormationDetail.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

public sealed class FormationDetailMapping : ClassMapping<FormationDetail>
{
    public FormationDetailMapping()
    {
        this.Table("read_formation_detail");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        this.Property(x => x.Title);
        this.Property(x => x.Audience);
        this.Property(x => x.Description, m => m.Column(c => c.SqlType("text")));
        this.Property(x => x.Status);
        this.Property(x => x.Archived);
        this.Property(x => x.IconUrl);
        this.Property(x => x.LinkedChatbotKey, m => m.Column("linked_chatbot_key"));
        this.Property(x => x.LinkedChatbotLabel, m => m.Column("linked_chatbot_label"));

        this.Property(x => x.Modules, m => { m.Column(c => { c.Name("modules_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<FormationModuleRow>>>(); });
        this.Property(x => x.Team, m => { m.Column(c => { c.Name("team_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<FormationMemberRow>>>(); });
    }
}

public sealed class ChatbotLabelRefMapping : ClassMapping<ChatbotLabelRef>
{
    public ChatbotLabelRefMapping()
    {
        this.Table("read_formation_detail_chatbot_label_ref");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        this.Property(x => x.Label);
    }
}
