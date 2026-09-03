using System.Text.Json;
using DigitalArs.Application.DTOs.Common;
using DigitalArs.Application.Exceptions;
using FluentValidation;

namespace DigitalArs.Api.Middlewares;

/// <summary>
/// Middleware global para captura y manejo centralizado de excepciones (HU-18).
/// Estandariza todas las respuestas de error en el formato uniforme ErrorResponse
/// y asegura el registro en logs con su correspondiente TraceId.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var traceId = context.TraceIdentifier;

        _logger.LogError(ex, "Excepción capturada en GlobalExceptionHandlerMiddleware: {Message} | TraceId: {TraceId}", ex.Message, traceId);

        var (statusCode, message, errors) = ex switch
        {
            ValidationException valEx => (
                StatusCodes.Status400BadRequest,
                "Error de validación en la solicitud.",
                valEx.Errors.Select(e => e.ErrorMessage).ToList()
            ),
            BadRequestException badReqEx => (
                StatusCodes.Status400BadRequest,
                badReqEx.Message,
                new List<string>()
            ),
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                argEx.Message,
                new List<string>()
            ),
            InvalidOperationException invOpEx => (
                StatusCodes.Status400BadRequest,
                invOpEx.Message,
                new List<string>()
            ),
            UnauthorizedAccessException unAuthAccEx => (
                StatusCodes.Status401Unauthorized,
                string.IsNullOrWhiteSpace(unAuthAccEx.Message) ? "No autorizado." : unAuthAccEx.Message,
                new List<string>()
            ),
            UnauthorizedException unAuthEx => (
                StatusCodes.Status401Unauthorized,
                unAuthEx.Message,
                new List<string>()
            ),
            ForbiddenException forbEx => (
                StatusCodes.Status403Forbidden,
                forbEx.Message,
                new List<string>()
            ),
            KeyNotFoundException keyNotEx => (
                StatusCodes.Status404NotFound,
                keyNotEx.Message,
                new List<string>()
            ),
            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                notFoundEx.Message,
                new List<string>()
            ),
            ConflictException confEx => (
                StatusCodes.Status409Conflict,
                confEx.Message,
                new List<string>()
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                _environment.IsDevelopment() ? ex.Message : "Ha ocurrido un error interno en el servidor.",
                _environment.IsDevelopment() && ex.StackTrace != null
                    ? new List<string> { ex.StackTrace }
                    : new List<string>()
            )
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Errors = errors,
            TraceId = traceId
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}

public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
