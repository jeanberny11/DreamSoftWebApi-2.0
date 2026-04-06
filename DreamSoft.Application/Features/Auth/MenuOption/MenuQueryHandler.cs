using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Auth.MenuOption;

public class MenuQueryHandler(
    IUserRepository userRepository,
    IPlanMenuOptionRepository planMenuOptionRepository,
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    IRoleMenuOptionRepository roleMenuOptionRepository)
    : IRequestHandler<MenuQuery, MenuResponse>
{
    public async Task<MenuResponse> Handle(MenuQuery request, CancellationToken cancellationToken)
    {
        // 1. Load the user with the userId.
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
             ?? throw new NotFoundException(ErrorMessageKeys.UserNotFound, request.UserId);

        // 2. Load the tenant subscription with the tenantId.
        var tenantSubscriptions = await tenantSubscriptionRepository.GetActiveByTenantAsync(user.TenantId, cancellationToken);
        var tenantSubscription = tenantSubscriptions.FirstOrDefault()
             ?? throw new NotFoundException(ErrorMessageKeys.SubscriptionNotActive);

        // 3. Load the menu options by the user role if is a regular user, otherwise load all menu options
        //    available for the tenant's plan if is an admin.
        List<Domain.Entities.MenuOption> menuOptions;
        if (user.IsAdmin)
        {
            var planMenuOptions = await planMenuOptionRepository.GetByPlanIdAsync(
                tenantSubscription.SubscriptionPlanId, cancellationToken);
            menuOptions = [.. planMenuOptions.Select(pmo => pmo.MenuOption)];
        }
        else
        {
            if (user.RoleId is null)
                throw new NotFoundException(ErrorMessageKeys.UserRoleNotAssigned);

            var roleMenuOptions = await roleMenuOptionRepository.GetByRoleIdAsync(user.RoleId.Value, cancellationToken);
            menuOptions = [.. roleMenuOptions.Select(rmo => rmo.MenuOption)];
        }

        // TODO: map menuOptions into MenuModule > MenuGroup > MenuOption hierarchy
        throw new NotImplementedException("Menu response mapping is not yet implemented.");
    }
}
