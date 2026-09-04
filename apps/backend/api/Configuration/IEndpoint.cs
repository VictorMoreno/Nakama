namespace Api.Configuration;

public interface IEndpoint
{
    void MapEndpoints(IEndpointRouteBuilder app);
}