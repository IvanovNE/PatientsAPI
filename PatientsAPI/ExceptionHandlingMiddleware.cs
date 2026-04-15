using Microsoft.AspNetCore.Mvc;
using PatientsAPI.Application.Common;
using PatientsAPI.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace PatientsAPI
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            var response = context.Response;
            response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Instance = context.Request.Path,
                Status = GetStatusCode(exception),
                Title = GetTitle(exception),
                Detail = GetDetail(exception)
            };

            if (_env.IsDevelopment())
            {
                problemDetails.Extensions["traceId"] = context.TraceIdentifier;
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            response.StatusCode = problemDetails.Status.Value;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
        }

        private static int GetStatusCode(Exception exception) => exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ValidationException => StatusCodes.Status400BadRequest,
            DomainException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        private static string GetTitle(Exception exception) => exception switch
        {
            NotFoundException => "Resource Not Found",
            ValidationException => "Validation Error",
            DomainException => "Business Rule Violation",
            ArgumentException => "Invalid Argument",
            InvalidOperationException => "Invalid Operation",
            _ => "Internal Server Error"
        };

        private static string GetDetail(Exception exception) =>
            exception.Message;
    }
}
