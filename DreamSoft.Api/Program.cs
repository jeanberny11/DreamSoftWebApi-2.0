using System.Text;
using DreamSoft.Api.Middleware;
using DreamSoft.Application;
using DreamSoft.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register Application layer services (MediatR, FluentValidation, AutoMapper, Behaviors)
builder.Services.AddApplication();

// Register Infrastructure layer services (DbContext, Repositories, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Configure Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "es" };

    options.SetDefaultCulture("en") // English as default
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);

    // Read from Accept-Language header
    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
    };
});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DreamSoft ERP API",
        Version = "v1",
        Description = "Multi-tenant ERP System API with Subdomain-based Multi-tenancy",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "DreamSoft",
            Email = "support@dreamsoft.com"
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS for subdomain support
builder.Services.AddCors(options =>
{
    options.AddPolicy("SubdomainPolicy", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                // Allow all subdomains of dreamsoft.com
                // Examples: https://acme.dreamsoft.com, https://techcorp.dreamsoft.com
                var uri = new Uri(origin);
                return uri.Host.EndsWith(".dreamsoft.com") ||
                       uri.Host == "localhost" || // For development
                       uri.Host == "dreamsoft.com"; // For main domain
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Required for HTTP-only cookies
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DreamSoft ERP API v1");
        options.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

app.UseHttpsRedirection();

// Use Request Localization Middleware
app.UseRequestLocalization();

// EXCEPTION HANDLING - Must be early in pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// CORS - must be before Authentication/Authorization
app.UseCors("SubdomainPolicy");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
