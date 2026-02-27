using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Onboarding.CompleteOnboarding;

public class CompleteOnboardingCommandHandler(
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    ISolutionRepository solutionRepository,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    ISubscriptionStatusRepository subscriptionStatusRepository,
    ITenantStatusRepository tenantStatusRepository,
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    ISolutionMenuOptionRepository solutionMenuOptionRepository,
    IRoleRepository roleRepository,
    IRoleMenuOptionRepository roleMenuOptionRepository,
    IRoleTemplateRepository roleTemplateRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CompleteOnboardingCommand, CompleteOnboardingResponse>
{
    public async Task<CompleteOnboardingResponse> Handle(
        CompleteOnboardingCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        // 1. Load tenant — must be PENDING_SUBSCRIPTION
        var tenant = await tenantRepository.GetByIdWithStatusAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        if (tenant.Status.Code != TenantStatusCodes.PendingSubscription)
            throw new ConflictException("OnboardingAlreadyComplete");

        // 2. Load and validate solution
        var solution = await solutionRepository.GetByIdAsync(request.SolutionId, cancellationToken)
            ?? throw new NotFoundException("NotFound", "Solution");

        if (!solution.IsActive)
            throw new NotFoundException("NotFound", "Solution");

        // 3. Load and validate plan — must be active and belong to the requested solution
        var plan = await subscriptionPlanRepository.GetByIdWithBillingCycleAsync(
            request.SubscriptionPlanId, cancellationToken)
            ?? throw new NotFoundException("NotFound", "SubscriptionPlan");

        if (!plan.IsActive || plan.SolutionId != request.SolutionId)
            throw new ConflictException("PlanNotBelongToSolution");

        // 4. Determine subscription status — TRIAL if plan has trial days, otherwise ACTIVE
        var statusCode = plan.HasTrial()
            ? SubscriptionStatusCodes.Trial
            : SubscriptionStatusCodes.Active;

        var subscriptionStatus = await subscriptionStatusRepository.GetByCodeAsync(
            statusCode, cancellationToken)
            ?? throw new NotFoundException(
                "NotFound", $"SubscriptionStatus {statusCode} not in database.");

        // 5. Build subscription dates
        var startDate  = DateTime.UtcNow;
        DateTime? trialEnd = plan.HasTrial()
            ? startDate.AddDays(plan.TrialDays)
            : null;

        var subscription = TenantSubscription.Create(
            tenantId:           tenantId,
            solutionId:         request.SolutionId,
            subscriptionPlanId: request.SubscriptionPlanId,
            statusId:           subscriptionStatus.Id,
            startDate:          startDate,
            trialEndDate:       trialEnd);

        // 6. Load ACTIVE tenant status
        var activeStatus = await tenantStatusRepository.GetByCodeAsync(
            TenantStatusCodes.Active, cancellationToken)
            ?? throw new NotFoundException(
                "NotFound", "TenantStatus ACTIVE not in database.");

        // 7. Load solution menu options (includes MenuOption navigation property)
        var solutionMenuOptions = await solutionMenuOptionRepository
            .GetBySolutionAsync(solution.Id, cancellationToken);

        // 8. Load the admin user for this tenant
        var adminUser = await userRepository.GetAdminByTenantAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException("UserNotFound", tenantId);

        // 9. Load admin role template and build the admin role
        var adminRoleTemplate = await roleTemplateRepository.GetByCodeAsync(
            RoleCodes.Admin, cancellationToken)
            ?? throw new NotFoundException(
                "NotFound", "The administrator template role was not found in the database.");

        var adminRole = Role.Create(
            tenantId:         tenant.Id,
            code:             adminRoleTemplate.Code,
            name:             adminRoleTemplate.Name,
            description:      adminRoleTemplate.Description,
            translatedString: adminRoleTemplate.Translations,
            roleTemplateId:   adminRoleTemplate.Id,
            createdBy:        adminUser.Id);

        // ── Begin transaction ────────────────────────────────────────────────
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await tenantSubscriptionRepository.AddAsync(subscription, cancellationToken);
            tenant.TransitionStatus(activeStatus.Id);
            await roleRepository.AddAsync(adminRole, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken); // materialise adminRole.Id

            var roleMenuOptions = solutionMenuOptions
                .Select(smo => RoleMenuOption.Create(adminRole.Id, smo.MenuOption.Id));

            await roleMenuOptionRepository.AddRangeAsync(roleMenuOptions, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
        // ── Transaction complete ─────────────────────────────────────────────

        return new CompleteOnboardingResponse("/dashboard");
    }
}
