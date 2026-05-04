using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class TenantSubdomain : AuditableEntity
{
    public int TenantId { get; private set; }
    public int SolutionId { get; private set; }
    public string Subdomain { get; private set; } = null!;

    // Navigation properties
    public Tenant Tenant { get; private set; } = null!;
    public Solution Solution { get; private set; } = null!;

    private TenantSubdomain() { }

    public static TenantSubdomain Create(int tenantId, int solutionId, string subdomain)
    {
        if (tenantId <= 0)
            throw new ArgumentException("Tenant ID must be greater than zero", nameof(tenantId));

        if (solutionId <= 0)
            throw new ArgumentException("Solution ID must be greater than zero", nameof(solutionId));

        if (string.IsNullOrWhiteSpace(subdomain))
            throw new ArgumentException("Subdomain is required", nameof(subdomain));

        var ts = new TenantSubdomain
        {
            TenantId = tenantId,
            SolutionId = solutionId,
            Subdomain = subdomain.ToLower().Trim(),
            IsActive = true
        };

        ts.InitializeAudit();
        return ts;
    }
}
