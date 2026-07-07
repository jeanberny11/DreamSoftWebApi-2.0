using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.AppFeatures.GetAllFeatures;

public record GetAllFeaturesQuery(string? Language = null) : IRequest<List<ModuleDto>>;
