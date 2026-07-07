using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.AppFeatures.GetMainFeatures;

public record GetMainFeaturesQuery(string? Language = null):IRequest<List<ModuleDto>>;