using DreamSoft.Domain.Entities;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;

public static class ModuleDtoMappings
{
    public static ModuleDto ToDto(this Module module, string language)
        => module.ToDto(module.MenuOptions, language);

    public static ModuleDto ToDto(this Module module, IEnumerable<MenuOption> menuOptions, string language)
        => new(
            ModuleId: module.Id,
            Code: module.Code,
            Name: module.Translations.GetNameOrFallback(language, module.Name),
            Description: module.Translations.GetDescriptionOrFallback(language, module.Description),
            Icon: module.Icon,
            SortOrder: module.SortOrder,
            Groups: [.. menuOptions
                .GroupBy(mo => mo.MenuGroup)
                .Select(group => new MenuGroupDto(
                    MenuGroupId: group.Key.Id,
                    Code: group.Key.Code,
                    Name: group.Key.Translations.GetNameOrFallback(language, group.Key.Name),
                    Description: group.Key.Translations.GetDescriptionOrFallback(language, group.Key.Description),
                    Icon: group.Key.Icon,
                    SortOrder: group.Key.SortOrder,
                    Options: [.. group
                        .Select(option => new MenuOptionDto(
                            MenuOptionId: option.Id,
                            Code: option.Code,
                            Name: option.Translations.GetNameOrFallback(language, option.Name),
                            Description: option.Translations.GetDescriptionOrFallback(language, option.Description),
                            Icon: option.Icon,
                            SortOrder: option.SortOrder))
                        .OrderBy(o => o.SortOrder)]))
                .OrderBy(g => g.SortOrder)]);
}
