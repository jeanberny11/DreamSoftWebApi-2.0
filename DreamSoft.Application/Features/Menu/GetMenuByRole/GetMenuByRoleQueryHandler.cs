using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Menu.GetMenuByRole;

public class GetMenuByRoleQueryHandler(
    ICurrentUserService currentUserService,
    IRoleRepository roleRepository,
    IRoleMenuOptionRepository roleMenuOptionRepository)
    : IRequestHandler<GetMenuByRoleQuery, GetMenuByRoleResponse>
{
    public async Task<GetMenuByRoleResponse> Handle(
        GetMenuByRoleQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve tenant + solution from JWT
        var tenantId = currentUserService.TenantId
            ?? throw new UnauthorizedException();
        var solutionId = currentUserService.SolutionId
            ?? throw new UnauthorizedException();

        // 2. Load the role
        var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.RoleNotFound, request.RoleId);

        // 3. Validate the role belongs to the current tenant + solution
        if (role.TenantId != tenantId || role.SolutionId != solutionId)
            throw new ForbiddenException(ForbiddenErrorCodes.RoleNotAuthorized, ErrorMessageKeys.Forbidden);

        // 4. Load role's menu options with full navigation (MenuOption → Module, MenuGroup)
        var roleMenuOptions = await roleMenuOptionRepository
            .GetByRoleIdWithMenuDataAsync(request.RoleId, cancellationToken);

        // 5. Build Module > MenuGroup > MenuOption hierarchy
        var modules = roleMenuOptions
            .Select(rm => rm.MenuOption)
            .GroupBy(mo => mo.Module)
            .OrderBy(g => g.Key.SortOrder)
            .Select(modGrp => new MenuModule(
                modGrp.Key.Code,
                modGrp.Key.Name,
                modGrp.Key.Description,
                modGrp.Key.Icon,
                modGrp.Key.SortOrder,
                modGrp
                    .GroupBy(mo => mo.MenuGroup)
                    .OrderBy(g => g.Key.SortOrder)
                    .Select(grpGrp => new MenuGroup(
                        grpGrp.Key.Code,
                        grpGrp.Key.Name,
                        grpGrp.Key.Icon,
                        grpGrp.Key.SortOrder,
                        grpGrp
                            .OrderBy(o => o.SortOrder)
                            .Select(o => new MenuOption(
                                o.Code, o.Name, o.Description,
                                o.Route, o.Icon, o.SortOrder))
                    ))
            ));

        // 6. Return response
        return new GetMenuByRoleResponse(role.Code, role.Name, modules);
    }
}
