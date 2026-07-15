using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Domain.Models;
using BuildingBlocks.Presentation.Models;
using System.Collections.Generic;

namespace BuildingBlocks.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        private ISender? _mediator;

        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

        protected string? TraceId => HttpContext.Items["TraceId"]?.ToString();

        protected IActionResult CreateResponse<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(ApiResponse<T>.Success(result.Value, TraceId));
            }

            return BadRequest(ApiResponse<T>.Failure(new List<string> { result.Error.Message }, TraceId));
        }

        protected IActionResult CreateResponse(Result result)
        {
            if (result.IsSuccess)
            {
                return Ok(ApiResponse.SuccessResult(TraceId));
            }

            return BadRequest(ApiResponse.Failure(new List<string> { result.Error.Message }, TraceId));
        }
    }
}
