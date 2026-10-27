using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using SkillHub.Application;
using SkillHub.Application.Common;
using SkillHub.Infrastructure;
using SkillHub.Infrastructure.Seeding;
using SkillHub.Web;
using SkillHub.Web.Infrastructure.Localization;

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

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Spanish is the main language. Only the language cookie can change it (not the browser language),
// and it is set from the language switch in the navbar.
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture(SupportedCultures.Spanish)
        .AddSupportedCultures(SupportedCultures.All)
        .AddSupportedUICultures(SupportedCultures.All);
    options.RequestCultureProviders = [new CookieRequestCultureProvider()];
});

builder.Services.AddRazorPages(options =>
    {
        options.Conventions.AuthorizeFolder("/Enrollments", "ParticipantOnly");
    })
    .AddMvcOptions(options => options.ModelMetadataDetailsProviders.Add(new LocalizedValidationMetadataProvider()))
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource)));

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRequestLocalization();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapGet("/culture/{culture}", (string culture, string? returnUrl, HttpContext http) =>
{
    if (SupportedCultures.All.Contains(culture))
    {
        http.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax });
    }

    // Only same-site paths: "/x" is local, "//host" and "/\host" are not.
    var isLocal = returnUrl is { Length: > 0 } && returnUrl[0] == '/' &&
                  (returnUrl.Length == 1 || (returnUrl[1] != '/' && returnUrl[1] != '\\'));
    return Results.LocalRedirect(isLocal ? returnUrl! : "/");
});
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
