using Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();

var app = builder.Build();

app.Configure();

app.UseHttpsRedirection();
app.Run();