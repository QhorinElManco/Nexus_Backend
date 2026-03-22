using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Response;

namespace Nexus.Api.Extensions;

public static class ResponseExtensions
{
    public static ActionResult<Response<T>> ToActionResult<T>(this Response<T> response)
    {
        return response.Success
            ? new OkObjectResult(response)
            : new ObjectResult(response) { StatusCode = response.ErrorCode.ToHttpStatusCode() };
    }

    public static ActionResult<Response<T>> ToCreatedAtActionResult<T>(
        this Response<T> response,
        string actionName,
        object routeValues)
    {
        return response.Success
            ? new CreatedAtActionResult(actionName, null, routeValues, response)
            : new ObjectResult(response) { StatusCode = response.ErrorCode.ToHttpStatusCode() };
    }

    public static ActionResult<Response<T>> ToNoContentResult<T>(this Response<T> response)
    {
        return response.Success
            ? new NoContentResult()
            : new ObjectResult(response) { StatusCode = response.ErrorCode.ToHttpStatusCode() };
    }
}
