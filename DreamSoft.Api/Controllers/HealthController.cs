using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DreamSoft.Infrastructure.Persistence;

namespace DreamSoft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check called at {Time}", DateTime.UtcNow);

        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "DreamSoft ERP API",
            Version = "1.0.0"
        });
    }

    /// <summary>
    /// Check database connection
    /// </summary>
    [HttpGet("database")]
    public async Task<IActionResult> CheckDatabase(
        [FromServices] ApplicationDbContext dbContext)
    {
        try
        {
            await dbContext.Database.CanConnectAsync();

            return Ok(new
            {
                Status = "Database Connected",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection failed");

            return StatusCode(500, new
            {
                Status = "Database Connection Failed",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Test reading data from existing tables
    /// </summary>
    [HttpGet("database/test")]
    public async Task<IActionResult> TestDatabaseRead(
        [FromServices] ApplicationDbContext dbContext)
    {
        try
        {
            // Try to read from existing lookup tables
            var tenantStatusCount = await dbContext.TenantStatuses.CountAsync();
            var genderCount = await dbContext.Genders.CountAsync();
            var languageCount = await dbContext.Languages.CountAsync();

            // Try to read from main tables
            var tenantCount = await dbContext.Tenants.CountAsync();
            var userCount = await dbContext.Users.CountAsync();

            // Get some sample data
            var tenantStatuses = await dbContext.TenantStatuses
                .Take(5)
                .Select(ts => new
                {
                    ts.Id,
                    ts.Code,
                    NameEs = ts.Name.Spanish,
                    NameEn = ts.Name.English
                })
                .ToListAsync();

            return Ok(new
            {
                Status = "Database Connected and Reading Data",
                TableCounts = new
                {
                    TenantStatuses = tenantStatusCount,
                    Genders = genderCount,
                    Languages = languageCount,
                    Tenants = tenantCount,
                    Users = userCount
                },
                SampleTenantStatuses = tenantStatuses,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database test failed");

            return StatusCode(500, new
            {
                Status = "Database Test Failed",
                Error = ex.Message,
                InnerError = ex.InnerException?.Message,
                StackTrace = ex.StackTrace,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}