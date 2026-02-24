using System.Net;
using System.Text.Json;
using DreamSoft.Api.Contracts.Responses;
using DreamSoft.Api.Resources;
using DreamSoft.Application.Common.Exceptions;
using Microsoft.Extensions.Localization;
using ApplicationException = DreamSoft.Application.Common.Exceptions.ApplicationException;

namespace DreamSoft.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// Catches all unhandled exceptions and returns standardized error responses
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IWebHostEnvironment environment,
    IStringLocalizer<ErrorMessages> localizer)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;
    private readonly IWebHostEnvironment _environment = environment;
    private readonly IStringLocalizer<ErrorMessages> _localizer = localizer;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var traceId = context.TraceIdentifier;
        var timestamp = DateTime.UtcNow;

        // Log the exception
        _logger.LogError(exception, "An error occurred. TraceId: {TraceId}", traceId);

        // Handle ValidationException separately
        if (exception is ValidationException validationException)
        {
            await HandleValidationExceptionAsync(context, validationException, traceId, timestamp);
            return;
        }

        // Handle Application Exceptions with localization
        if (exception is ApplicationException appException)
        {
            await HandleApplicationExceptionAsync(context, appException, traceId, timestamp);
            return;
        }

        // Handle unexpected exceptions
        await HandleUnexpectedExceptionAsync(context, exception, traceId, timestamp);
    }

    private async Task HandleValidationExceptionAsync(
        HttpContext context,
        ValidationException exception,
        string traceId,
        DateTime timestamp)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new ValidationErrorResponse
        {
            ErrorMessage = _localizer["ValidationError"],
            TraceId = traceId,
            Timestamp = timestamp,
            Errors = (Dictionary<string, string[]>)exception.Errors
        };

        var json = JsonSerializer.Serialize(response, _jsonOptions);

        await context.Response.WriteAsync(json);
    }

    private async Task HandleApplicationExceptionAsync(
        HttpContext context,
        ApplicationException exception,
        string traceId,
        DateTime timestamp)
    {
        var (statusCode, errorCode, errorType) = MapExceptionToHttpStatus(exception);

        // ForbiddenException carries its own specific error code — use it directly
        if (exception is ForbiddenException forbiddenEx)
            errorCode = forbiddenEx.ErrorCode;

        context.Response.StatusCode = statusCode;

        // Get localized message using resource key and parameters
        var localizedMessage = exception.Parameters.Length > 0
            ? _localizer[exception.ResourceKey, exception.Parameters]
            : _localizer[exception.ResourceKey];

        var response = new ErrorResponse
        {
            StatusCode = statusCode,
            ErrorCode = errorCode,
            ErrorType = errorType,
            ErrorMessage = localizedMessage,
            TraceId = traceId,
            Timestamp = timestamp,
            StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private async Task HandleUnexpectedExceptionAsync(
        HttpContext context,
        Exception exception,
        string traceId,
        DateTime timestamp)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new ErrorResponse
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            ErrorCode = ErrorCodes.InternalError,
            ErrorType = ErrorTypes.InternalError,
            ErrorMessage = _environment.IsDevelopment()
                ? $"{exception.GetType().Name}: {exception.Message}{(exception.InnerException != null ? $" | Inner: {exception.InnerException.Message}" : string.Empty)}"
                : _localizer["InternalServerError"],
            TraceId = traceId,
            Timestamp = timestamp,
            StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static (int statusCode, string errorCode, string errorType) MapExceptionToHttpStatus(
        ApplicationException exception)
    {
        return exception switch
        {
            NotFoundException => (
                (int)HttpStatusCode.NotFound,
                ErrorCodes.NotFound,
                ErrorTypes.NotFound
            ),
            ConflictException => (
                (int)HttpStatusCode.Conflict,
                ErrorCodes.Conflict,
                ErrorTypes.Conflict
            ),
            UnauthorizedException => (
                (int)HttpStatusCode.Unauthorized,
                ErrorCodes.Unauthorized,
                ErrorTypes.Unauthorized
            ),
            ForbiddenException => (
                (int)HttpStatusCode.Forbidden,
                ErrorCodes.Forbidden,
                ErrorTypes.Forbidden
            ),
            RateLimitExceededException => (
                (int)HttpStatusCode.TooManyRequests,
                ErrorCodes.RateLimitExceeded,
                ErrorTypes.RateLimitExceeded
            ),
            EmailSendException => (
                (int)HttpStatusCode.ServiceUnavailable,
                ErrorCodes.EmailSendFailed,
                ErrorTypes.EmailSendFailed
            ),
            _ => (
                (int)HttpStatusCode.BadRequest,
                ErrorCodes.InternalError,
                ErrorTypes.InternalError
            )
        };
    }
}
