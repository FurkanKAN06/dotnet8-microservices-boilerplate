using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AuthService.Application.Features.Auth.Login;
using BuildingBlocks.Presentation.Controllers;

namespace AuthService.Api.Controllers
{
    [AllowAnonymous]
    [Asp.Versioning.ApiVersion("1.0")]
    public class AuthController : ApiControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            return CreateResponse(await Mediator.Send(command));
        }

    }
}
