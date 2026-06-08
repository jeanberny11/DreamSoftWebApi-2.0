using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.Solutions.DTOs;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Solutions.CreateSolution;

public class CreateSolutionCommandHandler(
    ISolutionRepository solutionRepository,
    IRequestLanguageService languageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSolutionCommand, SolutionDto>
{
    public async Task<SolutionDto> Handle(
        CreateSolutionCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await solutionRepository.AnyAsync(
            s => s.Code == request.Code.ToUpper().Trim(), cancellationToken);

        if (exists)
            throw new ConflictException(ErrorMessageKeys.SolutionCodeAlreadyExists, request.Code);

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.Create(
                request.Translations.Spanish.Name,
                request.Translations.Spanish.Description),
            request.Translations.English is not null
                ? BaseTranslatedProperties.Create(
                    request.Translations.English.Name,
                    request.Translations.English.Description)
                : null);

        var solution = Solution.Create(
            request.Code,
            request.Name,
            translations,
            request.Description ?? string.Empty,
            request.Icon        ?? string.Empty,
            request.SortOrder);

        await solutionRepository.AddAsync(solution, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var language = languageService.Resolve();

        return new SolutionDto(
            solution.Id,
            solution.Code,
            solution.Translations.GetNameOrFallback(language, solution.Name),
            solution.Translations.GetDescriptionOrFallback(language, solution.Description),
            solution.Icon,
            solution.SortOrder,
            solution.IsActive);
    }
}
