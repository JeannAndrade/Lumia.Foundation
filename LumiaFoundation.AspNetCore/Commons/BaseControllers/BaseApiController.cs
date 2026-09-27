using Microsoft.AspNetCore.Mvc;

namespace LumiaFoundation.AspNetCore.Commons.BaseControllers;

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
