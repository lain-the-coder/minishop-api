using Microsoft.AspNetCore.Mvc;
using MiniShop.Api.Services;

namespace MiniShop.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Hand off to the next component in the pipeline
            await next(context);
        }
        catch (Exception ex)
        {
            // If response headers have already been sent to the client,
            // we cannot change the status code or write a body.
            if (context.Response.HasStarted)
            {
                logger.LogWarning("The response has already started, cannot write ProblemDetails.");
                throw;
            }

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        // 1. Map the exception to HTTP semantics
        var (status, title, detail) = ex switch
        {
            NotFoundException notFound => (
                StatusCodes.Status404NotFound,
                "Not Found",
                notFound.Message
            ),
            MiniShop.Api.Services.ValidationException validation => (
                StatusCodes.Status400BadRequest,
                "Validation Failed",
                validation.Message
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "An unexpected error occurred. Please try again later."
            )
        };

        // 2. Log appropriately
        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(ex, "Unhandled server crash: {Message}", ex.Message);
        }
        else
        {
            logger.LogInformation("Domain exception mapped to HTTP {StatusCode}: {Message}", status, ex.Message);
        }

        // 3. Create RFC 9457 ProblemDetails
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        // 4. Write HTTP response
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }
}