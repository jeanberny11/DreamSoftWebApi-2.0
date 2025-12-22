using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DreamSoft.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DreamSoft.Infrastructure.Services.Features.Email;

/// <summary>
/// Service for sending emails using Resend API
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Resend");

        _apiKey = configuration["Resend:ResendApiKey"]
            ?? throw new InvalidOperationException("Resend API Key not configured");
        _fromEmail = configuration["Resend:FromEmail"]
            ?? throw new InvalidOperationException("Resend FromEmail not configured");
        _fromName = configuration["Resend:FromName"] ?? "DreamSoft ERP";

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri("https://api.resend.com");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    /// <summary>
    /// Sends verification code email
    /// </summary>
    public async Task<bool> SendVerificationCodeAsync(
        string toEmail,
        string code,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var emailRequest = new ResendEmailRequest
            {
                From = $"{_fromName} <{_fromEmail}>",
                To = new[] { toEmail },
                Subject = "Código de Verificación - DreamSoft",
                Html = GetVerificationEmailHtml(code)
            };

            var response = await _httpClient.PostAsJsonAsync("/emails", emailRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Verification email sent successfully to: {Email}", toEmail);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Failed to send verification email to {Email}. Status: {StatusCode}, Error: {Error}",
                toEmail, response.StatusCode, errorContent);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification email to: {Email}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// Sends welcome email after successful registration
    /// </summary>
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
                From = $"{_fromName} <{_fromEmail}>",
                To = new[] { toEmail },
                Subject = $"¡Bienvenido a DreamSoft - {companyName}!",
                Html = GetWelcomeEmailHtml(firstName, companyName, subdomain)
            };

            var response = await _httpClient.PostAsJsonAsync("/emails", emailRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Welcome email sent successfully to: {Email}", toEmail);
            }
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
            // Don't throw - welcome email is not critical
        }
    }

    /// <summary>
    /// Sends password reset email
    /// </summary>
    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var emailRequest = new ResendEmailRequest
            {
                From = $"{_fromName} <{_fromEmail}>",
                To = new[] { toEmail },
                Subject = "Restablecer Contraseña - DreamSoft",
                Html = GetPasswordResetEmailHtml(resetToken)
            };

            var response = await _httpClient.PostAsJsonAsync("/emails", emailRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Password reset email sent successfully to: {Email}", toEmail);
            }
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

    private string GetVerificationEmailHtml(string verificationCode)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .container {{
            background-color: #f9f9f9;
            border-radius: 10px;
            padding: 30px;
            border: 1px solid #e0e0e0;
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 24px;
            font-weight: bold;
            color: #14b8a6;
        }}
        .code-container {{
            background-color: #ffffff;
            border: 2px solid #14b8a6;
            border-radius: 8px;
            padding: 20px;
            text-align: center;
            margin: 30px 0;
        }}
        .code {{
            font-size: 32px;
            font-weight: bold;
            color: #14b8a6;
            letter-spacing: 8px;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            font-size: 12px;
            color: #666;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>DreamSoft</div>
            <h2>Verificación de Correo Electrónico</h2>
        </div>

        <p>Hola,</p>

        <p>¡Gracias por registrarte en DreamSoft! Para completar tu registro, por favor usa el código de verificación a continuación:</p>

        <div class='code-container'>
            <div class='code'>{verificationCode}</div>
        </div>

        <p><strong>Este código expirará en 5 minutos.</strong></p>

        <p>Si no solicitaste este código, por favor ignora este correo electrónico.</p>

        <div class='footer'>
            <p>© 2025 DreamSoft. Todos los derechos reservados.</p>
            <p>Este es un correo electrónico automatizado, por favor no respondas.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetWelcomeEmailHtml(string firstName, string companyName, string subdomain)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .container {{
            background-color: #f9f9f9;
            border-radius: 10px;
            padding: 30px;
            border: 1px solid #e0e0e0;
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 24px;
            font-weight: bold;
            color: #14b8a6;
        }}
        .content {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 20px;
            margin: 20px 0;
        }}
        .button {{
            display: inline-block;
            background-color: #14b8a6;
            color: #ffffff;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            font-size: 12px;
            color: #666;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>DreamSoft</div>
            <h2>¡Bienvenido a DreamSoft!</h2>
        </div>

        <p>Hola {firstName},</p>

        <p>¡Felicitaciones! Tu cuenta de DreamSoft ha sido creada exitosamente para <strong>{companyName}</strong>.</p>

        <div class='content'>
            <p><strong>Detalles de tu cuenta:</strong></p>
            <ul>
                <li>Empresa: {companyName}</li>
                <li>Subdominio: {subdomain}.dreamsoft.com</li>
            </ul>
        </div>

        <p>Ya puedes iniciar sesión y comenzar a usar DreamSoft para gestionar tu negocio.</p>

        <div style='text-align: center;'>
            <a href='https://{subdomain}.dreamsoft.com/login' class='button'>Iniciar Sesión</a>
        </div>

        <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>

        <div class='footer'>
            <p>© 2025 DreamSoft. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPasswordResetEmailHtml(string resetToken)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .container {{
            background-color: #f9f9f9;
            border-radius: 10px;
            padding: 30px;
            border: 1px solid #e0e0e0;
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 24px;
            font-weight: bold;
            color: #14b8a6;
        }}
        .button {{
            display: inline-block;
            background-color: #14b8a6;
            color: #ffffff;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            font-size: 12px;
            color: #666;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>DreamSoft</div>
            <h2>Restablecer Contraseña</h2>
        </div>

        <p>Hola,</p>

        <p>Recibimos una solicitud para restablecer tu contraseña. Haz clic en el botón a continuación para crear una nueva contraseña:</p>

        <div style='text-align: center;'>
            <a href='https://dreamsoft.com/reset-password?token={resetToken}' class='button'>Restablecer Contraseña</a>
        </div>

        <p><strong>Este enlace expirará en 1 hora.</strong></p>

        <p>Si no solicitaste restablecer tu contraseña, puedes ignorar este correo electrónico de forma segura.</p>

        <div class='footer'>
            <p>© 2025 DreamSoft. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }
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
