using Microsoft.AspNetCore.Authentication.Cookies;
using SkillHub.Application;
using SkillHub.Application.Common;
using SkillHub.Infrastructure;
using SkillHub.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

// The web app is another presentation layer: it calls the application services directly.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ParticipantOnly", p => p.RequireRole(Roles.Participant));

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Enrollments", "ParticipantOnly");
});

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
