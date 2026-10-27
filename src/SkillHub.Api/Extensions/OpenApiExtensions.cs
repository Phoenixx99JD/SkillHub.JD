using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SkillHub.Api.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddSkillHubOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
        return services;
    }

    /// <summary>Declares the JWT bearer scheme so Scalar can send the token.</summary>
    private sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken ct)
        {
            document.Info = new OpenApiInfo
            {
                Title = "SkillHub API",
                Version = "v1",
                Description = "Workshops and enrollments management for SkillHub. " +
                              "Log in with POST /api/v1/auth/login and use the token as Bearer authentication."
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT obtained from POST /api/v1/auth/login"
            };
            document.Security ??= [];
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
            return Task.CompletedTask;
        }
    }
}
