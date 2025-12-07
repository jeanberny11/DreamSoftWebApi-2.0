using DreamSoft.Application;
using DreamSoft.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register Application layer services (MediatR, FluentValidation, AutoMapper, Behaviors)
builder.Services.AddApplication();

// Register Infrastructure layer services (DbContext, Repositories, Services)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DreamSoft ERP API",
        Version = "v1",
        Description = "Multi-tenant ERP System API",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "DreamSoft",
            Email = "support@dreamsoft.com"
        }
    });
});

// Add CORS (configure based on your needs)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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

// CORS
app.UseCors("AllowAll");

// Authentication & Authorization (we'll configure JWT later)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
