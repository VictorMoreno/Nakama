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

        Id(x => x.Key, m => m.Generator(Generators.Assigned));

        Property(x => x.Title);
        Property(x => x.Audience);
        Property(x => x.Description);
        Property(x => x.Status);
        Property(x => x.Archived);
        Property(x => x.IconUrl);
        Property(x => x.LinkedChatbotKey, m => m.Column("linked_chatbot_key"));
        Property(x => x.LinkedChatbotLabel, m => m.Column("linked_chatbot_label"));

        Property(x => x.Modules, m => { m.Column("modules_json"); m.Type<JsonUserType<List<FormationModuleRow>>>(); });
        Property(x => x.Team, m => { m.Column("team_json"); m.Type<JsonUserType<List<FormationMemberRow>>>(); });
        Property(x => x.CompletedModuleIds, m => { m.Column("completed_module_ids_json"); m.Type<JsonUserType<List<string>>>(); });
        Property(x => x.ExamScores, m => { m.Column("exam_scores_json"); m.Type<JsonUserType<Dictionary<string, int>>>(); });
    }
}

/// <summary>Backing mapping for FormationDetail's local Chatbot-label lookup - see <see cref="ChatbotLabelRef"/>.</summary>
public sealed class ChatbotLabelRefMapping : ClassMapping<ChatbotLabelRef>
{
    public ChatbotLabelRefMapping()
    {
        Table("read_formation_detail_chatbot_label_ref");

        Id(x => x.BotKey, m => m.Generator(Generators.Assigned));

        Property(x => x.Label);
    }
}
