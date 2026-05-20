using MediatR;

namespace DreamSoft.Application.Features.Apps.ErpApp.Auth.MenuOption;
public record MenuQuery(int UserId) : IRequest<MenuResponse>;