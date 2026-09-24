using Microsoft.AspNetCore.Mvc;
using LumiaFoundation.AspNetCore.ServiceFilters;

namespace LumiaFoundation.AspNetCore.Commons.BaseControllers;

[ServiceFilter(typeof(DomainExceptionMappingFilter))]
public abstract class BaseApiController : ControllerBase
{
    protected Guid GetCurrentUserId()
    {
        var userId = HttpContext.Items["UserId"]?.ToString();

        if (Guid.TryParse(userId, out var currentUserId))
            return currentUserId;

        throw new InvalidOperationException("Não foi possível identificar o usuário autenticado.");
    }
}
