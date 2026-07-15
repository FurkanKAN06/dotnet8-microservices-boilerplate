using MediatR;
using AuthService.Application.DTOs;
using BuildingBlocks.Domain.Models;

namespace AuthService.Application.Features.Auth.Login
{
    public class LoginCommand : IRequest<Result<TokenResponseDto>>
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
