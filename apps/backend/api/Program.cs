using Api.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();
builder.Services.AddApplication();

WebApplication app = builder.Build();

app.Configure();

app.UseHttpsRedirection();
app.Run();

public partial class Program;
