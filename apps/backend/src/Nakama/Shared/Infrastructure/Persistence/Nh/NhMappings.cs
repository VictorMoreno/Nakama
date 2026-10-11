using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;

namespace Nakama.Shared.Infrastructure.Persistence.Nh;

internal static class NhMappings
{
    public static void AddMappings(Configuration configuration)
    {
        ModelMapper mapper = new();

        mapper.AddMapping<Mappings.ChatbotMapping>();
        mapper.AddMapping<Mappings.ConversationMapping>();
        mapper.AddMapping<Mappings.FormationMapping>();

        mapper.AddMapping<Mappings.ChatbotSummariesMapping>();
        mapper.AddMapping<Mappings.ChatbotDetailMapping>();
        mapper.AddMapping<Mappings.ChatPageMapping>();
        mapper.AddMapping<Mappings.FormationsListMapping>();
        mapper.AddMapping<Mappings.FormationDetailMapping>();
        mapper.AddMapping<Mappings.ChatbotLabelRefMapping>();
        mapper.AddMapping<Mappings.AnalyticsMapping>();
        mapper.AddMapping<Mappings.UserMapping>();
        mapper.AddMapping<Mappings.UserActivityEntryMapping>();
        mapper.AddMapping<Mappings.ConversationAuditLogMapping>();
        mapper.AddMapping<Mappings.ConversationHistoryMapping>();
        mapper.AddMapping<Mappings.FormationEnrollmentMapping>();
        mapper.AddMapping<Mappings.LlmProviderConfigMapping>();

        HbmMapping mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
        configuration.AddMapping(mapping);
    }
}
