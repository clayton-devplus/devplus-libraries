using System.Net;
using System.Text.Json;
using Devplus.Security.OAuth.Contracts;
using Refit;

namespace Devplus.Security.OAuth.Exceptions;

/// <summary>
/// Exceção tipada para erros retornados pela API OAuth.
/// Encapsula o conteúdo de erro parseado da resposta HTTP.
/// </summary>
public class OAuthApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? ErrorCode { get; }
    public OAuthErrorResponse? ErrorResponse { get; }

    public OAuthApiException(HttpStatusCode statusCode, string message, string? errorCode = null, OAuthErrorResponse? errorResponse = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        ErrorResponse = errorResponse;
    }

    /// <summary>
    /// Cria OAuthApiException a partir de uma ApiException do Refit,
    /// tentando parsear o corpo da resposta como OAuthErrorResponse.
    /// </summary>
    public static OAuthApiException FromApiException(ApiException apiException)
    {
        OAuthErrorResponse? errorResponse = null;
        string? errorCode = null;
        var message = apiException.Message;

        if (!string.IsNullOrEmpty(apiException.Content))
        {
            try
            {
                errorResponse = JsonSerializer.Deserialize<OAuthErrorResponse>(apiException.Content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                });

                if (errorResponse != null)
                {
                    errorCode = errorResponse.ErrorCode;
                    if (!string.IsNullOrEmpty(errorResponse.Message))
                        message = errorResponse.Message;
                }
            }
            catch (JsonException)
            {
                // Se não conseguir parsear, mantém a mensagem original do Refit
            }
        }

        return new OAuthApiException(
            apiException.StatusCode,
            message,
            errorCode,
            errorResponse,
            apiException);
    }
}
