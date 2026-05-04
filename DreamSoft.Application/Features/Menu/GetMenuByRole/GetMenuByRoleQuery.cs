namespace DreamSoft.Application.Features.Menu.GetMenuByRole;
using MediatR;

public record GetMenuByRoleQuery(int RoleId)
    : IRequest<GetMenuByRoleResponse>;

public record GetMenuByRoleResponse(
    string RoleCode,
    string RoleName,
    IEnumerable<MenuModule> MenuModules
);

public record MenuOption(
    string Code,
    string Name,
    string Description,
    string Route,
    string Icon,
    int SortOrder
);

public record MenuGroup(
    string Code,
    string Name,
    string Icon,
    int SortOrder,
    IEnumerable<MenuOption> MenuOptions
);

public record MenuModule(
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    IEnumerable<MenuGroup> MenuGroups
);