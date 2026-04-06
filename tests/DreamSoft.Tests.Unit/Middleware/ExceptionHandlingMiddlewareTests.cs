using System.Text.Json;
using DreamSoft.Api;
using DreamSoft.Api.Contracts.Responses;
using DreamSoft.Api.Middleware;
using DreamSoft.Application.Common.Exceptions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using ApplicationException = DreamSoft.Application.Common.Exceptions.ApplicationException;

namespace DreamSoft.Tests.Unit.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static (ExceptionHandlingMiddleware middleware, DefaultHttpContext context) CreateSut(
        RequestDelegate next,
        bool isDevelopment = false)
    {
        var logger = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();

        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(e => e.EnvironmentName)
            .Returns(isDevelopment ? Environments.Development : Environments.Production);

        var localizer = new Mock<IStringLocalizer<ErrorMessages>>();
        localizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, $"Localized:{key}"));
        localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, $"Localized:{key}"));

        var middleware = new ExceptionHandlingMiddleware(next, logger, environment.Object, localizer.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        return (middleware, context);
    }

    private static async Task<T?> ReadResponseBodyAsync<T>(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return JsonSerializer.Deserialize<T>(body, JsonOptions);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_NoException_CallsNextWithoutWritingResponse()
    {
        var nextCalled = false;
        var (middleware, context) = CreateSut(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    // ── ValidationException ───────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400WithFieldErrors()
    {
        var failures = new[]
        {
            new ValidationFailure("Email", "Email is required"),
            new ValidationFailure("Email", "Email is invalid"),
            new ValidationFailure("Name", "Name is required"),
        };
        var (middleware, context) = CreateSut(_ => throw new ValidationException(failures));

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ValidationErrorResponse>(context);

        Assert.Equal(400, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        Assert.NotNull(response);
        Assert.Contains("Email", response.Errors.Keys);
        Assert.Contains("Name", response.Errors.Keys);
        Assert.Equal(2, response.Errors["Email"].Length);
        Assert.Single(response.Errors["Name"]);
    }

    [Fact]
    public async Task InvokeAsync_EmptyValidationException_Returns400WithEmptyErrors()
    {
        var (middleware, context) = CreateSut(_ => throw new ValidationException());

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ValidationErrorResponse>(context);

        Assert.Equal(400, context.Response.StatusCode);
        Assert.NotNull(response);
        Assert.Empty(response.Errors);
    }

    // ── Application exceptions ────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404()
    {
        var (middleware, context) = CreateSut(_ => throw new NotFoundException());

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ErrorResponse>(context);

        Assert.Equal(404, context.Response.StatusCode);
        Assert.Equal(ErrorCodes.NotFound, response!.ErrorCode);
        Assert.Equal(ErrorTypes.NotFound, response.ErrorType);
    }

    [Fact]
    public async Task InvokeAsync_ConflictException_Returns409()
    {
        var (middleware, context) = CreateSut(_ => throw new ConflictException());

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ErrorResponse>(context);

        Assert.Equal(409, context.Response.StatusCode);
        Assert.Equal(ErrorCodes.Conflict, response!.ErrorCode);
        Assert.Equal(ErrorTypes.Conflict, response.ErrorType);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedException_Returns401()
    {
        var (middleware, context) = CreateSut(_ => throw new UnauthorizedException());

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ErrorResponse>(context);

        Assert.Equal(401, context.Response.StatusCode);
        Assert.Equal(ErrorCodes.Unauthorized, response!.ErrorCode);
        Assert.Equal(ErrorTypes.Unauthorized, response.ErrorType);
    }

    [Fact]
    public async Task InvokeAsync_ForbiddenException_Returns403WithSpecificErrorCode()
    {
        var exception = new ForbiddenException(
            ForbiddenErrorCodes.AccountLocked,
            ErrorMessageKeys.Forbidden);

        var (middleware, context) = CreateSut(_ => throw exception);

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ErrorResponse>(context);

        Assert.Equal(403, context.Response.StatusCode);
        Assert.Equal(ForbiddenErrorCodes.AccountLocked, response!.ErrorCode);
        Assert.Equal(ErrorTypes.Forbidden, response.ErrorType);
    }

    [Fact]
    public async Task InvokeAsync_RateLimitExceededException_Returns429()
    {
        var (middleware, context) = CreateSut(_ => throw new RateLimitExceededException());

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ErrorResponse>(context);

        Assert.Equal(429, context.Response.StatusCode);
        Assert.Equal(ErrorCodes.RateLimitExceeded, response!.ErrorCode);
        Assert.Equal(ErrorTypes.RateLimitExceeded, response.ErrorType);
    }

    [Fact]
    public async Task InvokeAsync_EmailSendException_Returns503()
    {
        var (middleware, context) = CreateSut(_ => throw new EmailSendException());

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ErrorResponse>(context);

        Assert.Equal(503, context.Response.StatusCode);
        Assert.Equal(ErrorCodes.EmailSendFailed, response!.ErrorCode);
        Assert.Equal(ErrorTypes.EmailSendFailed, response.ErrorType);
    }

    // ── Unexpected / generic exception ────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_UnexpectedException_Returns500()
    {
        var (middleware, context) = CreateSut(_ => throw new InvalidOperationException("boom"));

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ErrorResponse>(context);

        Assert.Equal(500, context.Response.StatusCode);
        Assert.Equal(ErrorCodes.InternalError, response!.ErrorCode);
        Assert.Equal(ErrorTypes.InternalError, response.ErrorType);
    }

    // ── Environment-specific behaviour ───────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_Development_UnexpectedException_ExposesExceptionDetails()
    {
        var (middleware, context) = CreateSut(
            _ => throw new InvalidOperationException("secret detail"),
            isDevelopment: true);

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ErrorResponse>(context);

        Assert.Equal(500, context.Response.StatusCode);
        Assert.Contains("InvalidOperationException", response!.ErrorMessage);
        Assert.Contains("secret detail", response.ErrorMessage);
    }

    [Fact]
    public async Task InvokeAsync_Production_UnexpectedException_UsesLocalizedMessage()
    {
        var (middleware, context) = CreateSut(
            _ => throw new InvalidOperationException("secret detail"),
            isDevelopment: false);

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ErrorResponse>(context);

        Assert.Equal(500, context.Response.StatusCode);
        Assert.DoesNotContain("secret detail", response!.ErrorMessage);
        Assert.StartsWith("Localized:", response.ErrorMessage);
    }

    [Fact]
    public async Task InvokeAsync_Production_AppException_OmitsStackTrace()
    {
        var (middleware, context) = CreateSut(
            _ => throw new NotFoundException(),
            isDevelopment: false);

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ErrorResponse>(context);

        Assert.Null(response!.StackTrace);
    }

    // ── Response shape ────────────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_AnyException_SetsContentTypeToApplicationJson()
    {
        var (middleware, context) = CreateSut(_ => throw new Exception("error"));

        await middleware.InvokeAsync(context);

        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_AnyException_ResponseIncludesTraceId()
    {
        var (middleware, context) = CreateSut(_ => throw new NotFoundException());
        context.TraceIdentifier = "test-trace-42";

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ErrorResponse>(context);

        Assert.Equal("test-trace-42", response!.TraceId);
    }

    [Fact]
    public async Task InvokeAsync_AnyException_ResponseIncludesTimestamp()
    {
        var before = DateTime.UtcNow;
        var (middleware, context) = CreateSut(_ => throw new NotFoundException());

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ErrorResponse>(context);

        Assert.True(response!.Timestamp >= before);
        Assert.True(response.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public async Task InvokeAsync_AppException_UsesLocalizedMessage()
    {
        var (middleware, context) = CreateSut(_ => throw new NotFoundException());

        await middleware.InvokeAsync(context);
        var response = await ReadResponseBodyAsync<ErrorResponse>(context);

        Assert.StartsWith("Localized:", response!.ErrorMessage);
    }
}
