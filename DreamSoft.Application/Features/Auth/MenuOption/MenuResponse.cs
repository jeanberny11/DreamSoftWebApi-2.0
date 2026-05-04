namespace DreamSoft.Application.Features.Auth.MenuOption;

public record OptionAction(
    string Code,
    string Name,
    string Description
);

public record MenuOption(
    string Code,
    string Name,
    string Description,
    string Icon,
    string Route,
    List<OptionAction> Actions
);

public record MenuGroup(
    string Code,
    string Name,
    string Description,
    string Icon,
    List<MenuOption> Options
);

public record MenuModule(
    string Code,
    string Name,
    string Description,
    string Icon,
    List<MenuGroup> Groups
);

public record MenuResponse(
    List<MenuModule> Modules
);