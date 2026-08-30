
using Microsoft.AspNetCore.Http;

namespace LumiaFoundation.AspNetCore.Commons.Exceptions
{
    public class CommandValidationException(string message) : DomainBaseException(message)
    {
        protected override int StatusCodeValue => StatusCodes.Status422UnprocessableEntity;
    }
}