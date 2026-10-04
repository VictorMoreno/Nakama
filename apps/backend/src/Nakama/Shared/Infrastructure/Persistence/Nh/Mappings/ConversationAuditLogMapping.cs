using Nakama.ConversationAudit.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class ConversationAuditLogMapping : ClassMapping<ConversationAuditLog>
{
    public ConversationAuditLogMapping()
    {
        this.Table("read_conversation_audit");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        this.Property(x => x.UserId, m => m.Column("user_id"));
        this.Property(x => x.BotKey, m => m.Column("bot_key"));
        this.Property(x => x.Title);
        this.Property(x => x.StartedAt);
        this.Property(x => x.Messages, m => { m.Column(c => { c.Name("messages_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<AuditMessageRow>>>(); });
    }
}
