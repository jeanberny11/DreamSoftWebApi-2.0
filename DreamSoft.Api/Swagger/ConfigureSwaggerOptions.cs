using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DreamSoft.Api.Swagger;

public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "DreamSoft ERP API",
                Version = description.ApiVersion.ToString(),
                Description = "Multi-tenant ERP System API with Subdomain-based Multi-tenancy",
                Contact = new OpenApiContact
                {
                    Name = "DreamSoft",
                    Email = "support@dreamsoft.com"
                }
            });
        }
    }
}
