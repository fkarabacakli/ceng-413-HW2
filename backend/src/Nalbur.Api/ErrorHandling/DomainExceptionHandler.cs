using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nalbur.Domain.Exceptions;

namespace Nalbur.Api.ErrorHandling;

/// <summary>
/// Translates exceptions into HTTP answers. The Domain does not know about HTTP;
/// this outer layer decides that a broken rule is a 400 and a conflict is a 409.
/// </summary>
internal sealed class DomainExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (status, title, detail) = exception switch
        {
            InsufficientStockException e => (StatusCodes.Status409Conflict, "Stok yetersiz", e.Message),
            DuplicateSkuException e => (StatusCodes.Status409Conflict, "Kayıt çakışması", e.Message),
            DomainException e => (StatusCodes.Status400BadRequest, "Geçersiz istek", e.Message),
            BadHttpRequestException e => (e.StatusCode, "Geçersiz istek", "İstek gövdesi okunamadı. Alanları ve değerleri kontrol edin."),
            _ => (0, string.Empty, string.Empty),
        };

        if (status == 0)
        {
            return false; // not ours: let the default handler return a 500
        }

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
            },
        });
    }
}
