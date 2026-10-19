using Nakama.ChatbotMembers.Domain;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class ChatbotMembersMapping : ClassMapping<ChatbotMember>
{
    public ChatbotMembersMapping()
    {
        this.Table("read_chatbot_members");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        this.Property(x => x.ChatbotId, m => m.Column("chatbot_id"));
        this.Property(x => x.UserId, m => m.Column("user_id"));
    }
}
