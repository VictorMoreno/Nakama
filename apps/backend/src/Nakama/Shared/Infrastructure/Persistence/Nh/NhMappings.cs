using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;

namespace Nakama.Shared.Infrastructure.Persistence.Nh;

/// <summary>
/// Centralizes every mapping-by-code registration in one place per agents.md §6 ("Mapping NH
/// en Persistence/Nh/Mappings"), rather than scattering a mapping file per module. Both the
/// write and read <see cref="Configuration"/> pull from the same set - the write side only
/// ever touches the aggregate mappings (Chatbot/Conversation/Formation), the read side only
/// the *ReadModel mappings, but there is no harm in both configurations knowing about all of
/// them for this scope.
/// </summary>
internal static class NhMappings
{
    public static void AddMappings(Configuration configuration)
    {
        var mapper = new ModelMapper();

        mapper.AddMapping<Mappings.ChatbotMapping>();
        mapper.AddMapping<Mappings.ConversationMapping>();
        mapper.AddMapping<Mappings.FormationMapping>();

        mapper.AddMapping<Mappings.ChatbotsListMapping>();
        mapper.AddMapping<Mappings.ChatbotDetailMapping>();
        mapper.AddMapping<Mappings.ChatPageMapping>();
        mapper.AddMapping<Mappings.FormationsListMapping>();
        mapper.AddMapping<Mappings.FormationDetailMapping>();
        mapper.AddMapping<Mappings.ChatbotLabelRefMapping>();
        mapper.AddMapping<Mappings.AnalyticsMapping>();

        var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
        configuration.AddMapping(mapping);
    }
}
