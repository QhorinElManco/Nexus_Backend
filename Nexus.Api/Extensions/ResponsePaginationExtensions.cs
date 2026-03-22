using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Response;

namespace Nexus.Api.Extensions;

public static class ResponsePaginationExtensions
{
    public static ActionResult<ResponsePagination<T>> ToActionResult<T>(this ResponsePagination<T> response)
    {
        return response.Success
            ? new OkObjectResult(response)
            : new ObjectResult(response) { StatusCode = response.ErrorCode.ToHttpStatusCode() };
    }
}
