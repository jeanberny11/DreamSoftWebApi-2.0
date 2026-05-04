using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Auth;

public class AdminLogoutCommandHandler(
    IAdminRefreshTokenRepository adminRefreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<AdminLogoutCommand>
{
    public async Task Handle(AdminLogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await adminRefreshTokenRepository
            .GetActiveTokenAsync(request.RefreshToken, cancellationToken);

        if (token is null) return; // Already revoked or expired — no-op

        token.Revoke(currentUserService.IpAddress);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
