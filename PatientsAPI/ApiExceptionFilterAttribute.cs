using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PatientsAPI.Application.Common;
using PatientsAPI.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace PatientsAPI
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<ApiExceptionFilterAttribute> _logger;
        private readonly IWebHostEnvironment _env;

        public ApiExceptionFilterAttribute(
            ILogger<ApiExceptionFilterAttribute> logger,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public override void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "An error occurred");

            var problemDetails = new ProblemDetails
            {
                Instance = context.HttpContext.Request.Path,
                Status = GetStatusCode(context.Exception),
                Title = GetTitle(context.Exception),
                Detail = context.Exception.Message
            };

            if (_env.IsDevelopment())
            {
                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                problemDetails.Extensions["stackTrace"] = context.Exception.StackTrace;
            }

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status
            };

            context.ExceptionHandled = true;
        }

        private static int GetStatusCode(Exception exception) => exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            DomainException => StatusCodes.Status400BadRequest,
            ValidationException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        private static string GetTitle(Exception exception) => exception switch
        {
            NotFoundException => "Resource Not Found",
            DomainException => "Business Rule Violation",
            ValidationException => "Validation Error",
            _ => "Server Error"
        };
    }
}
