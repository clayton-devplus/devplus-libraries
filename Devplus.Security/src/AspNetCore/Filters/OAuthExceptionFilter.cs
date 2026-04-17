using System.Net;
using Devplus.Security.OAuth.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Devplus.Security.AspNetCore.Filters;

/// <summary>
/// Intercepta exceções do fluxo OAuth e retorna respostas HTTP
/// com o status code correto ao invés de 500.
/// </summary>
public class OAuthExceptionFilter : IExceptionFilter
{
    private readonly ILogger<OAuthExceptionFilter> _logger;

    public OAuthExceptionFilter(ILogger<OAuthExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case OAuthApiException oauthEx:
                HandleOAuthApiException(context, oauthEx);
                break;

            case UnauthorizedAccessException unauthEx:
                HandleUnauthorizedAccessException(context, unauthEx);
                break;
        }
    }

    private void HandleOAuthApiException(ExceptionContext context, OAuthApiException ex)
    {
        var statusCode = (int)ex.StatusCode;

        if (statusCode >= 500)
            _logger.LogError(ex, "OAuth API error: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
        else
            _logger.LogWarning("OAuth API error: {StatusCode} {ErrorCode} - {Message}", statusCode, ex.ErrorCode, ex.Message);

        var response = new
        {
            type = "error",
            code = statusCode.ToString(),
            errorCode = ex.ErrorCode ?? "OAUTH_ERROR",
            message = ex.Message,
            detailedMessage = ex.ErrorResponse?.DetailedMessage ?? ex.Message
        };

        context.Result = new ObjectResult(response) { StatusCode = statusCode };
        context.ExceptionHandled = true;
    }

    private void HandleUnauthorizedAccessException(ExceptionContext context, UnauthorizedAccessException ex)
    {
        _logger.LogWarning("Unauthorized: {Message}", ex.Message);

        var response = new
        {
            type = "error",
            code = "401",
            errorCode = "UNAUTHORIZED",
            message = ex.Message
        };

        context.Result = new ObjectResult(response) { StatusCode = (int)HttpStatusCode.Unauthorized };
        context.ExceptionHandled = true;
    }
}
