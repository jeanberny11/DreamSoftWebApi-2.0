using MediatR;

namespace DreamSoft.Application.Features.Auth.MenuOption;
public record MenuQuery(int UserId) : IRequest<MenuResponse>;