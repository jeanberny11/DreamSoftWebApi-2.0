using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Auth;

public class AdminLogoutCommandHandler(
    IAdminRefreshTokenRepository adminRefreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentAdminService currentAdminService)
    : IRequestHandler<AdminLogoutCommand>
{
    public async Task Handle(AdminLogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await adminRefreshTokenRepository
            .GetActiveTokenAsync(request.RefreshToken, cancellationToken);

        if (token is null) return; // Already revoked or expired — no-op

        token.Revoke(currentAdminService.IpAddress);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
