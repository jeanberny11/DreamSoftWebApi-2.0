# Multi-Language Error Handling Implementation

## Overview
This implementation adds comprehensive multi-language error handling with ASP.NET Core's built-in localization system using `.resx` resource files.

## Components Created

### 1. Resource Files (English & Spanish)
- `DreamSoft.Api/Resources/ErrorMessages.resx` - English (default)
- `DreamSoft.Api/Resources/ErrorMessages.es.resx` - Spanish
- `DreamSoft.Api/Resources/ErrorMessages.cs` - Marker class for IStringLocalizer

### 2. Error Response Classes
Located in `DreamSoft.Api/Contracts/Responses/`:
- `ErrorResponse.cs` - Standard error response for all non-validation exceptions
- `ValidationErrorResponse.cs` - Specialized response for FluentValidation errors
- `ErrorCodes.cs` - Constant error codes (e.g., "USER_NOT_FOUND")
- `ErrorTypes.cs` - User-friendly error type names (e.g., "Not Found")

### 3. Refactored Domain Exceptions
All exceptions in `DreamSoft.Application/Common/Exceptions/` now use resource keys:
- `ApplicationException.cs` - Base exception with ResourceKey and Parameters
- `NotFoundException.cs`
- `ConflictException.cs`
- `UnauthorizedException.cs`
- `RateLimitExceededException.cs`
- `EmailSendException.cs`
- `ValidationException.cs` - Special handling for FluentValidation

### 4. Exception Handling Middleware
- `DreamSoft.Api/Middleware/ExceptionHandlingMiddleware.cs`
  - Catches all unhandled exceptions
  - Translates messages based on `Accept-Language` header
  - Maps exceptions to appropriate HTTP status codes
  - Includes stack trace only in Development environment
  - Returns standardized JSON responses

### 5. Updated Program.cs
- Added localization services configuration
- Configured supported cultures (English, Spanish)
- Added `UseRequestLocalization()` middleware
- Added `UseMiddleware<ExceptionHandlingMiddleware>()` early in pipeline

## Usage

### Throwing Exceptions with Resource Keys

**Before:**
```csharp
throw new NotFoundException($"User with email '{email}' not found");
```

**After:**
```csharp
throw new NotFoundException("UserNotFound", email);
```

### Adding New Error Messages

1. **Add to ErrorMessages.resx (English):**
```xml
<data name="ProductNotFound" xml:space="preserve">
  <value>Product with ID '{0}' was not found</value>
</data>
```

2. **Add to ErrorMessages.es.resx (Spanish):**
```xml
<data name="ProductNotFound" xml:space="preserve">
  <value>Producto con ID '{0}' no fue encontrado</value>
</data>
```

3. **Use in code:**
```csharp
throw new NotFoundException("ProductNotFound", productId);
```

### Client-Side Usage

**Request with language preference:**
```http
GET /api/users/123
Accept-Language: es
```

**Response (Spanish):**
```json
{
  "statusCode": 404,
  "errorCode": "NOT_FOUND",
  "errorType": "Not Found",
  "errorMessage": "Usuario con correo 'test@example.com' no fue encontrado",
  "traceId": "0HN7...",
  "timestamp": "2024-12-17T10:30:00Z",
  "stackTrace": null
}
```

**Request with English (default):**
```http
GET /api/users/123
Accept-Language: en
```

**Response (English):**
```json
{
  "statusCode": 404,
  "errorCode": "NOT_FOUND",
  "errorType": "Not Found",
  "errorMessage": "User with email 'test@example.com' was not found",
  "traceId": "0HN7...",
  "timestamp": "2024-12-17T10:30:00Z",
  "stackTrace": null
}
```

## Exception to HTTP Status Code Mapping

| Exception Type | HTTP Status | Error Code | Error Type |
|---|---|---|---|
| `ValidationException` | 400 | VALIDATION_ERROR | Validation Error |
| `NotFoundException` | 404 | NOT_FOUND | Not Found |
| `UnauthorizedException` | 401 | UNAUTHORIZED | Unauthorized |
| `ConflictException` | 409 | CONFLICT | Conflict |
| `RateLimitExceededException` | 429 | RATE_LIMIT_EXCEEDED | Rate Limit Exceeded |
| `EmailSendException` | 503 | EMAIL_SEND_FAILED | Email Send Failed |
| Unexpected Exception | 500 | INTERNAL_ERROR | Internal Server Error |

## Available Resource Keys

### General Errors
- `ValidationError` - One or more validation errors occurred
- `InternalServerError` - An unexpected error occurred
- `Unauthorized` - You are not authorized to access this resource
- `Forbidden` - You do not have permission to perform this action
- `NotFound` - The requested resource was not found

### Authentication Errors
- `InvalidCredentials` - Invalid email or password
- `InvalidOtpCode` - Invalid or expired verification code
- `EmailNotVerified` - Email address must be verified before proceeding

### Specific Entity Errors
- `UserNotFound` - User with email '{0}' was not found
- `TenantNotFound` - Tenant '{0}' was not found

### Conflict Errors
- `Conflict` - The resource already exists
- `EmailAlreadyExists` - A user with email '{0}' already exists
- `TenantAlreadyExists` - A tenant with subdomain '{0}' already exists

### Rate Limiting
- `RateLimitExceeded` - Too many requests. Please try again later

### Email Service
- `EmailSendFailed` - Failed to send email. Please try again later

## Testing

### Test Different Languages
```bash
# Test with Spanish
curl -H "Accept-Language: es" https://localhost:7280/api/auth/login \
  -d '{"username":"invalid","password":"wrong"}'

# Test with English (default)
curl -H "Accept-Language: en" https://localhost:7280/api/auth/login \
  -d '{"username":"invalid","password":"wrong"}'
```

### Test Validation Errors
```bash
curl -H "Accept-Language: es" https://localhost:7280/api/verification/send-code \
  -d '{"email":"invalid-email"}'
```

Expected response:
```json
{
  "statusCode": 400,
  "errorCode": "VALIDATION_ERROR",
  "errorType": "Validation Error",
  "errorMessage": "Ocurrieron uno o más errores de validación",
  "traceId": "...",
  "timestamp": "...",
  "errors": {
    "Email": ["Email format is invalid"]
  }
}
```

## Benefits

1. **Consistency**: All errors follow the same response structure
2. **Localization**: Automatic translation based on Accept-Language header
3. **Type Safety**: Resource keys are strings, but typos are caught at runtime
4. **Maintainability**: Single source of truth for error messages
5. **Client-Friendly**: Error codes allow programmatic error handling
6. **Debugging**: TraceId links errors to logs, stack traces in dev mode
7. **Scalability**: Easy to add new languages or error messages

## Notes

- FluentValidation error messages are handled separately and use FluentValidation's built-in localization
- Resource files are compiled into the assembly (no runtime file I/O)
- Changes to `.resx` files require application restart
- Default language is English (`en`)
- Supported languages: English (`en`), Spanish (`es`)
