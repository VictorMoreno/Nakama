using System.Diagnostics.CodeAnalysis;
using FluentMigrator;
using Microsoft.AspNetCore.Identity;
using Nakama.Users.Domain;

namespace Nakama.Shared.Infrastructure.Persistence.Migrations;

[Migration(2)]
[SuppressMessage("Major Vulnerability", "S2068:Credentials should not be hard-coded")]
public sealed class SeedUsersMigration : Migration
{
    public override void Up()
    {
        var clock = new SystemClock();
        var hasher = new PasswordHasher<User>();
        const string password = "Admin123.";

        var admin = User.Create(UserId.From(Guid.Parse("48D7EAD2-BAB5-468C-B7C5-CEF4D4A3842F")),
            new Email("admin@nakama.com"), "Admin", isAdmin: true, clock);
        this.Insert.IntoTable("users").Row(new
        {
            id = admin.Id.Value,
            email = admin.Email,
            normalizedemail = admin.NormalizedEmail,
            name = admin.Name,
            isadmin = admin.IsAdmin,
            passwordhash = hasher.HashPassword(admin, password),
            createdat = admin.CreatedAt
        });

        var user = User.Create(UserId.From(Guid.Parse("EFB5209F-13C5-40B4-8E72-35AFA54CBEF8")),
            new Email("user@nakama.com"), "User", isAdmin: false, clock);
        this.Insert.IntoTable("users").Row(new
        {
            id = user.Id.Value,
            email = user.Email,
            normalizedemail = user.NormalizedEmail,
            name = user.Name,
            isadmin = user.IsAdmin,
            passwordhash = hasher.HashPassword(user, password),
            createdat = user.CreatedAt
        });
    }

    public override void Down() => throw new NotSupportedException("No down migrations - see ADR entry.");
}
