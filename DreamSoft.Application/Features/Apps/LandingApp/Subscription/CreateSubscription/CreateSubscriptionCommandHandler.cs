using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Subscription.CreateSubscription;

public partial class CreateSubscriptionCommandHandler(
    ITenantRepository tenantRepository,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    IPlanPriceRepository planPriceRepository,
    ISubscriptionStatusRepository subscriptionStatusRepository,
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    ITenantSubdomainRepository tenantSubdomainRepository,
    IRoleRepository roleRepository,
    IRoleTemplateRepository roleTemplateRepository,
    IPlanMenuOptionRepository planMenuOptionRepository,
    IRoleMenuOptionRepository roleMenuOptionRepository,
    IOptionActionRepository optionActionRepository,
    IRoleOptionActionRepository roleOptionActionRepository,
    IUserRepository userRepository,
    IPaymentGateway paymentGateway,
    IPaymentSettings paymentSettings,
    ICurrentTenantService currentTenantService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSubscriptionCommand, CreateSubscriptionResponse>
{
    public async Task<CreateSubscriptionResponse> Handle(
        CreateSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        // 1. Validate tenant exists
        var tenant = await tenantRepository.GetByIdAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        // 2. Validate plan exists
        var plan = await subscriptionPlanRepository.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.SubscriptionPlanNotFound, request.PlanId);

        if (!plan.IsActive)
            throw new ConflictException("SubscriptionPlanNotActive");

        // 3. Ensure the tenant does not already have a subscription for this solution
        var alreadySubscribed = await tenantSubscriptionRepository.ExistsForTenantAndSolutionAsync(
            tenantId, plan.SolutionId, cancellationToken);

        if (alreadySubscribed)
            throw new ConflictException("TenantAlreadySubscribedToSolution");

        // 4. Validate plan price exists and belongs to the given plan
        var planPrice = await planPriceRepository.GetByIdAsync(request.PlanPriceId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.PlanPriceNotFound, request.PlanPriceId);

        if (planPrice.PlanId != request.PlanId)
            throw new ConflictException("PlanPriceMismatch");

        if (!planPrice.IsActive)
            throw new ConflictException("PlanPriceNotActive");

        // 5. StripePriceId must be configured on the selected price
        if (string.IsNullOrWhiteSpace(planPrice.StripePriceId))
            throw new ConflictException("StripePriceIdNotConfigured");

        // 5b. The frontend base URL must be configured before we start creating
        // records — Stripe Checkout session creation (step 14) needs it to build
        // valid success/cancel URLs. Checking this now, before any writes happen,
        // avoids leaving an orphaned subscription/subdomain/admin user behind if
        // checkout session creation fails later.
        if (string.IsNullOrWhiteSpace(paymentSettings.FrontendBaseUrl))
            throw new ConflictException("PaymentGatewayNotConfigured");

        // 6. Resolve PROCESSING_PAYMENT status
        var status = await subscriptionStatusRepository
            .GetByCodeAsync(SubscriptionStatusCodes.ProcessingPayment, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "SubscriptionStatus", "Code = PROCESSING_PAYMENT");

        // 7. Create the subscription in PROCESSING_PAYMENT state
        var startDate = DateTime.UtcNow;
        DateTime? trialEndDate = plan.TrialDays > 0
            ? startDate.AddDays(plan.TrialDays)
            : null;

        var subscription = TenantSubscription.Create(
            tenantId:           tenantId,
            solutionId:         plan.SolutionId,
            subscriptionPlanId: request.PlanId,
            planPriceId:        request.PlanPriceId,
            statusId:           status.Id,
            startDate:          startDate,
            trialEndDate:       trialEndDate);

        await tenantSubscriptionRepository.AddAsync(subscription, cancellationToken);

        // 8. Create the subdomain based on the tenant's company name
        var subdomain = await BuildUniqueSubdomainAsync(tenant.CompanyName, tenantId, cancellationToken);
        var tenantSubdomain = TenantSubdomain.Create(tenantId, plan.SolutionId, subdomain);
        await tenantSubdomainRepository.AddAsync(tenantSubdomain, cancellationToken);

        // 9. Create the default admin role for this subscription
        var adminTemplate = (await roleTemplateRepository.GetByPlanIdAsync(request.PlanId, cancellationToken))
            .FirstOrDefault(t => t.Code == RoleCodes.Admin);

        var adminTranslations = TranslatedString.Create(
            BaseTranslatedProperties.Create("Administrador", "Rol de administrador del sistema"),
            BaseTranslatedProperties.Create("Administrator", "System administrator role"));

        var adminRole = Role.Create(
            tenantId:       tenantId,
            solutionId:     plan.SolutionId,
            code:           RoleCodes.Admin,
            name:           "Administrator",
            translations:   adminTranslations,
            description:    "Default admin role",
            roleTemplateId: adminTemplate?.Id,
            isCustom:       false);

        await roleRepository.AddAsync(adminRole, cancellationToken);

        // ── DB transaction ────────────────────────────────────────────────
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // 10. Create the admin user and assign the admin role
            var adminUser = User.Create(
                tenantId:     tenantId,
                solutionId:   plan.SolutionId,
                username:     tenant.Email,
                email:        tenant.Email,
                passwordHash: tenant.PasswordHash,
                firstName:    tenant.FirstName,
                lastName:     tenant.LastName,
                languageId:   tenant.LanguageId,
                isAdmin:      true);

            adminUser.AssignRole(adminRole.Id);
            await userRepository.AddAsync(adminUser, cancellationToken);

            // 11. Seed RoleMenuOptions from all plan menu options
            var menuOptionIds = await planMenuOptionRepository
                .GetMenuOptionIdsByPlanIdAsync(request.PlanId, cancellationToken);

            var roleMenuOptions = menuOptionIds
                .Select(id => RoleMenuOption.Create(adminRole.Id, id));

            await roleMenuOptionRepository.AddRangeAsync(roleMenuOptions, cancellationToken);

            // 12. Seed RoleOptionActions — all actions x all plan menu options
            var allActions = await optionActionRepository.GetAllAsync(cancellationToken);

            var roleOptionActions = menuOptionIds
                .SelectMany(menuOptionId => allActions
                    .Select(action => RoleOptionAction.Create(adminRole.Id, menuOptionId, action.Id)));

            await roleOptionActionRepository.AddRangeAsync(roleOptionActions, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
        // ── Transaction complete ──────────────────────────────────────────

        // 13. Create or retrieve the Stripe customer and persist the ID on the tenant
        var gatewayCustomerId = await paymentGateway.CreateOrGetCustomerAsync(
            tenantId:    tenantId,
            email:       tenant.Email,
            companyName: tenant.CompanyName,
            ct:          cancellationToken);

        if (string.IsNullOrWhiteSpace(tenant.StripeCustomerId))
        {
            tenant.SetStripeCustomerId(gatewayCustomerId);
            await tenantRepository.UpdateAsync(tenant, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // 14. Create the Stripe hosted checkout session.
        // URLs are composed per-request: success returns to the subscriptions
        // list carrying the session ID; cancel returns to the exact checkout
        // page the user left, ready to retry.
        var successUrl =
            $"{paymentSettings.FrontendBaseUrl}/account/subscriptions?checkout=success&session_id={{CHECKOUT_SESSION_ID}}";
        var cancelUrl =
            $"{paymentSettings.FrontendBaseUrl}/account/subscriptions/new/checkout?planId={request.PlanId}&planPriceId={request.PlanPriceId}&checkout=cancelled";

        var checkoutResult = await paymentGateway.CreateCheckoutSessionAsync(
            new CheckoutRequest(
                GatewayCustomerId: gatewayCustomerId,
                GatewayPriceId:    planPrice.StripePriceId,
                TenantId:          tenantId,
                TrialDays:         plan.TrialDays,
                SuccessUrl:        successUrl,
                CancelUrl:         cancelUrl),
            ct: cancellationToken);

        // 15. Stamp the StripeSessionId on the subscription so the webhook can
        //     find this exact record via a direct lookup — no ambiguity possible
        subscription.SetStripeSessionId(checkoutResult.GatewaySessionId);
        await tenantSubscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSubscriptionResponse(true, subscription.Id, checkoutResult.CheckoutUrl);
    }

    private static readonly System.Text.RegularExpressions.Regex SlugPattern =
        new(@"[^a-z0-9]+", System.Text.RegularExpressions.RegexOptions.Compiled);

    private async Task<string> BuildUniqueSubdomainAsync(
        string companyName,
        int tenantId,
        CancellationToken cancellationToken)
    {
        var slug = SlugPattern.Replace(companyName.ToLower().Trim(), "-").Trim('-');

        if (!await tenantSubdomainRepository.SubdomainExistsAsync(slug, cancellationToken))
            return slug;

        var candidate = $"{slug}-{tenantId}";
        if (!await tenantSubdomainRepository.SubdomainExistsAsync(candidate, cancellationToken))
            return candidate;

        return $"{slug}-{tenantId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
    }
}
