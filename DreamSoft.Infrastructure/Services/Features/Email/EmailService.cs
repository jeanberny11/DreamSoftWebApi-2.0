using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DreamSoft.Application.Common;
using DreamSoft.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace DreamSoft.Infrastructure.Services.Features.Email;

/// <summary>
/// Service for sending emails using Resend API
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Resend");

        var apiKey = configuration["Resend:ResendApiKey"]
            ?? throw new InvalidOperationException("Resend API Key not configured");
        _fromEmail = configuration["Resend:FromEmail"]
            ?? throw new InvalidOperationException("Resend FromEmail not configured");
        _fromName = configuration["Resend:FromName"] ?? "DreamSoft ERP";

        _httpClient.BaseAddress = new Uri("https://api.resend.com");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    // ── SendVerificationCodeAsync ─────────────────────────────────────────

    public async Task<EmailSendResult> SendVerificationCodeAsync(
        string toEmail,
        string code,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var emailRequest = new ResendEmailRequest
            {
                From    = $"{_fromName} <{_fromEmail}>",
                To      = new[] { toEmail },
                Subject = "Código de Verificación - DreamSoft",
                Html    = GetVerificationEmailHtml(code)
            };

            var response = await _httpClient.PostAsJsonAsync("/emails", emailRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Verification email sent successfully to: {Email}", toEmail);
                return EmailSendResult.Success();
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Failed to send verification email to {Email}. Status: {StatusCode}, Error: {Error}",
                toEmail, response.StatusCode, errorContent);

            return EmailSendResult.Failure($"HTTP {(int)response.StatusCode}: {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification email to: {Email}", toEmail);
            return EmailSendResult.Failure(ex.Message);
        }
    }

    // ── SendWelcomeEmailAsync ─────────────────────────────────────────────

    public async Task SendWelcomeEmailAsync(
        string toEmail,
        string firstName,
        string companyName,
        string subdomain,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var emailRequest = new ResendEmailRequest
            {
                From    = $"{_fromName} <{_fromEmail}>",
                To      = new[] { toEmail },
                Subject = $"¡Bienvenido a DreamSoft - {companyName}!",
                Html    = GetWelcomeEmailHtml(firstName, companyName, subdomain)
            };

            var response = await _httpClient.PostAsJsonAsync("/emails", emailRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
                _logger.LogInformation("Welcome email sent successfully to: {Email}", toEmail);
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to send welcome email to {Email}. Status: {StatusCode}, Error: {Error}",
                    toEmail, response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to: {Email}", toEmail);
        }
    }

    // ── SendPasswordResetEmailAsync ───────────────────────────────────────

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var emailRequest = new ResendEmailRequest
            {
                From    = $"{_fromName} <{_fromEmail}>",
                To      = new[] { toEmail },
                Subject = "Restablecer Contraseña - DreamSoft",
                Html    = GetPasswordResetEmailHtml(resetToken)
            };

            var response = await _httpClient.PostAsJsonAsync("/emails", emailRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
                _logger.LogInformation("Password reset email sent successfully to: {Email}", toEmail);
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to send password reset email to {Email}. Status: {StatusCode}, Error: {Error}",
                    toEmail, response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to: {Email}", toEmail);
            throw;
        }
    }

    // ── SendSubscriptionConfirmationAsync ─────────────────────────────────

    public async Task SendSubscriptionConfirmationAsync(
        string toEmail,
        string firstName,
        string companyName,
        string subdomain,
        string planName,
        string billingCycle,
        decimal amount,
        string currency,
        string invoiceNumber,
        DateTime paidAt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var emailRequest = new ResendEmailRequest
            {
                From    = $"{_fromName} <{_fromEmail}>",
                To      = new[] { toEmail },
                Subject = $"¡Pago confirmado! Tu suscripción a DreamSoft está activa - {companyName}",
                Html    = GetSubscriptionConfirmationHtml(
                              firstName, companyName, subdomain,
                              planName, billingCycle, amount, currency,
                              invoiceNumber, paidAt)
            };

            var response = await _httpClient.PostAsJsonAsync("/emails", emailRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
                _logger.LogInformation(
                    "Subscription confirmation email sent to: {Email}", toEmail);
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to send subscription confirmation to {Email}. Status: {StatusCode}, Error: {Error}",
                    toEmail, response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            // Never throw — subscription is already active, email is informational
            _logger.LogError(ex, "Error sending subscription confirmation email to: {Email}", toEmail);
        }
    }

    // ── SendPaymentFailedAsync ────────────────────────────────────────────

    public async Task SendPaymentFailedAsync(
        string toEmail,
        string firstName,
        string companyName,
        string planName,
        string? failureReason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var emailRequest = new ResendEmailRequest
            {
                From    = $"{_fromName} <{_fromEmail}>",
                To      = new[] { toEmail },
                Subject = $"Problema con tu pago - DreamSoft",
                Html    = GetPaymentFailedHtml(firstName, companyName, planName, failureReason)
            };

            var response = await _httpClient.PostAsJsonAsync("/emails", emailRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
                _logger.LogInformation(
                    "Payment failed email sent to: {Email}", toEmail);
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to send payment failed email to {Email}. Status: {StatusCode}, Error: {Error}",
                    toEmail, response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            // Never throw — status is already updated, email is informational
            _logger.LogError(ex, "Error sending payment failed email to: {Email}", toEmail);
        }
    }

    // ── HTML Templates ────────────────────────────────────────────────────

    private static string GetVerificationEmailHtml(string verificationCode) => $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .container {{ background-color: #f9f9f9; border-radius: 10px; padding: 30px; border: 1px solid #e0e0e0; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ font-size: 24px; font-weight: bold; color: #14b8a6; }}
        .code-container {{ background-color: #ffffff; border: 2px solid #14b8a6; border-radius: 8px; padding: 20px; text-align: center; margin: 30px 0; }}
        .code {{ font-size: 32px; font-weight: bold; color: #14b8a6; letter-spacing: 8px; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'><div class='logo'>DreamSoft</div><h2>Verificación de Correo Electrónico</h2></div>
        <p>Hola,</p>
        <p>¡Gracias por registrarte en DreamSoft! Para completar tu registro, por favor usa el código de verificación a continuación:</p>
        <div class='code-container'><div class='code'>{verificationCode}</div></div>
        <p><strong>Este código expirará en 5 minutos.</strong></p>
        <p>Si no solicitaste este código, por favor ignora este correo electrónico.</p>
        <div class='footer'><p>© 2025 DreamSoft. Todos los derechos reservados.</p></div>
    </div>
</body>
</html>";

    private static string GetWelcomeEmailHtml(string firstName, string companyName, string subdomain) => $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .container {{ background-color: #f9f9f9; border-radius: 10px; padding: 30px; border: 1px solid #e0e0e0; }}
        .logo {{ font-size: 24px; font-weight: bold; color: #14b8a6; }}
        .content {{ background-color: #ffffff; border-radius: 8px; padding: 20px; margin: 20px 0; }}
        .button {{ display: inline-block; background-color: #14b8a6; color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div style='text-align:center;margin-bottom:30px;'><div class='logo'>DreamSoft</div><h2>¡Bienvenido a DreamSoft!</h2></div>
        <p>Hola {firstName},</p>
        <p>¡Felicitaciones! Tu cuenta de DreamSoft ha sido creada exitosamente para <strong>{companyName}</strong>.</p>
        <div class='content'>
            <p><strong>Detalles de tu cuenta:</strong></p>
            <ul><li>Empresa: {companyName}</li><li>Subdominio: {subdomain}.dreamsoft.com</li></ul>
        </div>
        <div style='text-align:center;'><a href='https://{subdomain}.dreamsoft.com/login' class='button'>Iniciar Sesión</a></div>
        <div class='footer'><p>© 2025 DreamSoft. Todos los derechos reservados.</p></div>
    </div>
</body>
</html>";

    private static string GetPasswordResetEmailHtml(string resetToken) => $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .container {{ background-color: #f9f9f9; border-radius: 10px; padding: 30px; border: 1px solid #e0e0e0; }}
        .logo {{ font-size: 24px; font-weight: bold; color: #14b8a6; }}
        .button {{ display: inline-block; background-color: #14b8a6; color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div style='text-align:center;margin-bottom:30px;'><div class='logo'>DreamSoft</div><h2>Restablecer Contraseña</h2></div>
        <p>Hola,</p>
        <p>Recibimos una solicitud para restablecer tu contraseña. Haz clic en el botón a continuación para crear una nueva contraseña:</p>
        <div style='text-align:center;'><a href='https://dreamsoft.com/reset-password?token={resetToken}' class='button'>Restablecer Contraseña</a></div>
        <p><strong>Este enlace expirará en 1 hora.</strong></p>
        <p>Si no solicitaste restablecer tu contraseña, puedes ignorar este correo electrónico de forma segura.</p>
        <div class='footer'><p>© 2025 DreamSoft. Todos los derechos reservados.</p></div>
    </div>
</body>
</html>";

    private static string GetSubscriptionConfirmationHtml(
        string firstName,
        string companyName,
        string subdomain,
        string planName,
        string billingCycle,
        decimal amount,
        string currency,
        string invoiceNumber,
        DateTime paidAt) => $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .container {{ background-color: #f9f9f9; border-radius: 10px; padding: 30px; border: 1px solid #e0e0e0; }}
        .logo {{ font-size: 24px; font-weight: bold; color: #14b8a6; }}
        .success-banner {{ background-color: #d1fae5; border: 1px solid #6ee7b7; border-radius: 8px; padding: 16px; text-align: center; margin: 20px 0; color: #065f46; font-weight: bold; font-size: 16px; }}
        .section {{ background-color: #ffffff; border-radius: 8px; padding: 20px; margin: 16px 0; border: 1px solid #e5e7eb; }}
        .section h3 {{ margin: 0 0 12px 0; color: #14b8a6; font-size: 14px; text-transform: uppercase; letter-spacing: 0.05em; }}
        .invoice-table {{ width: 100%; border-collapse: collapse; }}
        .invoice-table td {{ padding: 8px 0; font-size: 14px; }}
        .invoice-table td:last-child {{ text-align: right; font-weight: bold; }}
        .invoice-table tr.total td {{ border-top: 2px solid #e5e7eb; padding-top: 12px; font-size: 16px; color: #14b8a6; }}
        .subdomain-box {{ background-color: #f0fdfa; border: 2px solid #14b8a6; border-radius: 8px; padding: 16px; text-align: center; margin: 20px 0; }}
        .subdomain-url {{ font-size: 18px; font-weight: bold; color: #14b8a6; }}
        .button {{ display: inline-block; background-color: #14b8a6; color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
        .label {{ color: #6b7280; }}
    </style>
</head>
<body>
    <div class='container'>
        <div style='text-align:center;margin-bottom:24px;'>
            <div class='logo'>DreamSoft</div>
            <h2 style='color:#065f46;'>¡Pago Confirmado!</h2>
        </div>

        <p>Hola <strong>{firstName}</strong>,</p>
        <p>Tu pago ha sido procesado exitosamente y tu suscripción a DreamSoft ERP para <strong>{companyName}</strong> está ahora activa.</p>

        <div class='success-banner'>
            ✓ Suscripción activa
        </div>

        <div class='section'>
            <h3>Tu acceso</h3>
            <p style='margin:0 0 8px 0;font-size:14px;color:#6b7280;'>Puedes acceder a tu plataforma en la siguiente dirección:</p>
            <div class='subdomain-box'>
                <div class='subdomain-url'>{subdomain}.dreamsoft.com</div>
            </div>
            <div style='text-align:center;'>
                <a href='https://{subdomain}.dreamsoft.com/login' class='button'>Ir a mi plataforma</a>
            </div>
        </div>

        <div class='section'>
            <h3>Factura</h3>
            <table class='invoice-table'>
                <tr>
                    <td class='label'>Número de factura</td>
                    <td>{invoiceNumber}</td>
                </tr>
                <tr>
                    <td class='label'>Fecha de pago</td>
                    <td>{paidAt:dd/MM/yyyy HH:mm} UTC</td>
                </tr>
                <tr>
                    <td class='label'>Empresa</td>
                    <td>{companyName}</td>
                </tr>
                <tr>
                    <td class='label'>Plan</td>
                    <td>{planName}</td>
                </tr>
                <tr>
                    <td class='label'>Ciclo de facturación</td>
                    <td>{billingCycle}</td>
                </tr>
                <tr class='total'>
                    <td>Total pagado</td>
                    <td>{amount:F2} {currency}</td>
                </tr>
            </table>
        </div>

        <p style='font-size:13px;color:#6b7280;'>Si tienes alguna pregunta sobre tu factura o suscripción, no dudes en contactarnos respondiendo a este correo.</p>

        <div class='footer'>
            <p>© 2025 DreamSoft. Todos los derechos reservados.</p>
            <p>Este es un correo electrónico automatizado, por favor no respondas directamente.</p>
        </div>
    </div>
</body>
</html>";

    private static string GetPaymentFailedHtml(
        string firstName,
        string companyName,
        string planName,
        string? failureReason) => $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .container {{ background-color: #f9f9f9; border-radius: 10px; padding: 30px; border: 1px solid #e0e0e0; }}
        .logo {{ font-size: 24px; font-weight: bold; color: #14b8a6; }}
        .error-banner {{ background-color: #fee2e2; border: 1px solid #fca5a5; border-radius: 8px; padding: 16px; text-align: center; margin: 20px 0; color: #991b1b; font-weight: bold; font-size: 16px; }}
        .section {{ background-color: #ffffff; border-radius: 8px; padding: 20px; margin: 16px 0; border: 1px solid #e5e7eb; }}
        .section h3 {{ margin: 0 0 12px 0; color: #374151; font-size: 14px; text-transform: uppercase; letter-spacing: 0.05em; }}
        .reason-box {{ background-color: #fff7ed; border-left: 4px solid #fb923c; border-radius: 4px; padding: 12px 16px; margin: 12px 0; font-size: 14px; color: #9a3412; }}
        .steps ol {{ margin: 8px 0; padding-left: 20px; font-size: 14px; }}
        .steps ol li {{ margin-bottom: 8px; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div style='text-align:center;margin-bottom:24px;'>
            <div class='logo'>DreamSoft</div>
            <h2 style='color:#991b1b;'>Problema con tu Pago</h2>
        </div>

        <p>Hola <strong>{firstName}</strong>,</p>
        <p>Lamentamos informarte que no pudimos procesar el pago de tu suscripción a DreamSoft ERP para <strong>{companyName}</strong>.</p>

        <div class='error-banner'>
            ✗ Pago no procesado
        </div>

        <div class='section'>
            <h3>Detalles</h3>
            <p style='font-size:14px;margin:0 0 8px 0;'><strong>Plan:</strong> {planName}</p>
            <p style='font-size:14px;margin:0 0 8px 0;'><strong>Estado de la suscripción:</strong> Pago pendiente</p>
            {(string.IsNullOrWhiteSpace(failureReason) ? "" : $@"
            <p style='font-size:14px;margin:8px 0 4px 0;'><strong>Motivo del error:</strong></p>
            <div class='reason-box'>{failureReason}</div>")}
        </div>

        <div class='section steps'>
            <h3>¿Qué puedes hacer?</h3>
            <ol>
                <li>Inicia sesión en tu cuenta de DreamSoft.</li>
                <li>Ve a la sección <strong>Suscripciones</strong> en tu panel de control.</li>
                <li>Selecciona la suscripción pendiente y haz clic en <strong>Reintentar pago</strong>.</li>
                <li>Verifica que los datos de tu tarjeta sean correctos o usa un método de pago diferente.</li>
            </ol>
            <p style='font-size:13px;color:#6b7280;'>Si el problema persiste, por favor comunícate con nosotros respondiendo a este correo y te ayudaremos a resolver la situación.</p>
        </div>

        <p style='font-size:13px;color:#6b7280;'>Tu información de cuenta y configuración están seguras. Solo necesitas completar el pago para activar tu suscripción.</p>

        <div class='footer'>
            <p>© 2025 DreamSoft. Todos los derechos reservados.</p>
            <p>Este es un correo electrónico automatizado. Para soporte, responde a este mensaje.</p>
        </div>
    </div>
</body>
</html>";
}

/// <summary>
/// Resend API email request model
/// </summary>
internal class ResendEmailRequest
{
    [JsonPropertyName("from")]
    public string From { get; set; } = null!;

    [JsonPropertyName("to")]
    public string[] To { get; set; } = null!;

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = null!;

    [JsonPropertyName("html")]
    public string Html { get; set; } = null!;
}
