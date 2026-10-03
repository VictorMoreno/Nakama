using Nakama.LlmProviders.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class LlmProviderConfigMapping : ClassMapping<LlmProviderConfig>
{
    public LlmProviderConfigMapping()
    {
        Table("llm_provider_configs");
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
            m.Type<GuidValueObjectUserType<LlmProviderConfigId>>();
        });

        Property(x => x.Provider, m => m.Column("provider"));
        Property(x => x.ApiKey, m => m.Column(c => { c.Name("api_key"); c.SqlType("text"); }));
        Property(x => x.Enabled, m => m.Column("enabled"));
        Property("_models", m =>
        {
            m.Column(c => { c.Name("models_json"); c.SqlType("text"); });
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<LlmModelOption>>>();
        });
        Property(x => x.UpdatedAt, m => m.Column("updated_at"));
    }
}
