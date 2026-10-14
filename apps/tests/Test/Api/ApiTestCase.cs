using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Nakama.Authentication.Application;
using Nakama.Users.Domain;
using Test.Api.Support;
using Test.Modules.Shared;
using Test.Modules.Shared.Infrastructure;
using Test.Modules.Users.Domain;

namespace Test.Api;

[Category("E2E")]
public abstract class ApiTestCase
{
    private TestDatabase _database = null!;

    private NakamaApiFactory _factory = null!;

    protected HttpClient Client { get; private set; } = null!;

    protected FakeLlmClient Llm => this._factory.Llm;

    [SetUp]
    public async Task ApiSetUp()
    {
        this._database = await PostgresContainer.CreateDatabase();
        this._factory = new NakamaApiFactory(this._database);
        this.Client = this._factory.CreateClient(new()
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = false
        });
    }

    [TearDown]
    public async Task ApiTearDown()
    {
        this.Client.Dispose();
        await this._factory.DisposeAsync();
        await PostgresContainer.DropDatabase(this._database);
    }

    protected async Task<string> SignIn(bool isAdmin)
    {
        UserCredentials credentials = await this.RegisterUser(isAdmin);
        HttpResponseMessage login = await this.Client.LogIn(credentials);
        TokenResponse? token = await login.Content.ReadFromJsonAsync<TokenResponse>();

        return token!.AccessToken;
    }

    protected async Task<UserCredentials> RegisterUser(bool isAdmin)
    {
        User user = UserMother.With(isAdmin: isAdmin);
        string password = PasswordMother.Valid();

        using IServiceScope scope = this._factory.Services.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        IdentityResult result = await userManager.CreateAsync(user, password);

        Assert.That(result.Succeeded, Is.True, string.Join(", ", result.Errors.Select(error => error.Description)));

        return new UserCredentials(user.Email, password);
    }
}
