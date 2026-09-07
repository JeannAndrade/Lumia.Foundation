using Microsoft.AspNetCore.Mvc;
using LumiaFoundation.AspNetCore.ServiceFilters;

namespace LumiaFoundation.AspNetCore.Commons.BaseControllers;

[ServiceFilter(typeof(DomainExceptionMappingFilter))]
public abstract class BaseApiController : ControllerBase
{

}