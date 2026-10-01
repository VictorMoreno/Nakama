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
        Table("read_formation_detail");
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        Property(x => x.Title);
        Property(x => x.Audience);
        Property(x => x.Description, m => m.Column(c => c.SqlType("text")));
        Property(x => x.Status);
        Property(x => x.Archived);
        Property(x => x.IconUrl);
        Property(x => x.LinkedChatbotKey, m => m.Column("linked_chatbot_key"));
        Property(x => x.LinkedChatbotLabel, m => m.Column("linked_chatbot_label"));

        Property(x => x.Modules, m => { m.Column(c => { c.Name("modules_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<FormationModuleRow>>>(); });
        Property(x => x.Team, m => { m.Column(c => { c.Name("team_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<FormationMemberRow>>>(); });
    }
}

public sealed class ChatbotLabelRefMapping : ClassMapping<ChatbotLabelRef>
{
    public ChatbotLabelRefMapping()
    {
        Table("read_formation_detail_chatbot_label_ref");
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        Property(x => x.Label);
    }
}
