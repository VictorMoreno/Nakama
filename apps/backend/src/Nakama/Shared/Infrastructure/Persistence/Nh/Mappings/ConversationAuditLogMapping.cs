using Nakama.ConversationAudit.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class ConversationAuditLogMapping : ClassMapping<ConversationAuditLog>
{
    public ConversationAuditLogMapping()
    {
        Table("read_conversation_audit");
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        Property(x => x.UserId, m => m.Column("user_id"));
        Property(x => x.BotKey, m => m.Column("bot_key"));
        Property(x => x.Title);
        Property(x => x.StartedAt);
        Property(x => x.Messages, m => { m.Column(c => { c.Name("messages_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<AuditMessageRow>>>(); });
    }
}
