namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetSolutionByCode;

using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;
using MediatR;

public record GetSolutionByCodeQuery(string Code, string? Language = null)
    : IRequest<SolutionDto>;
