using Microsoft.AspNetCore.Authorization;
using Nakama.ChatbotMembers.Application;
using Nakama.ChatbotMembers.Application.Searcher;
using Nakama.Shared.Domain;

namespace Api.Configuration;

public sealed class ChatbotMemberAuthorizationHandler(ICurrentUser currentUser, IQueryBus queryBus)
    : AuthorizationHandler<ChatbotMemberRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ChatbotMemberRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        if (currentUser.IsAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is not HttpContext httpContext)
        {
            return;
        }

        object? routeValue = httpContext.GetRouteValue("id") ?? httpContext.GetRouteValue("botKey");
        if (!Guid.TryParse(routeValue?.ToString(), out Guid chatbotId))
        {
            return;
        }

        IReadOnlyCollection<ChatbotMemberResponse> memberships = await queryBus.Ask(
            new ChatbotMemberSearcher.Query(currentUser.Id),
            httpContext.RequestAborted);

        if (memberships.Any(membership => membership.ChatbotId == chatbotId))
        {
            context.Succeed(requirement);
        }
    }
}
