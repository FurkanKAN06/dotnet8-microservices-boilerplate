using MediatR;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using BuildingBlocks.Domain.Models;

namespace AuthService.Application.Features.Auth.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;

        public LoginCommandHandler(IUserRepository userRepository, IJwtProvider jwtProvider)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
        }

        public async Task<Result<TokenResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            User? user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
            if (user == null)
            {
                return Result<TokenResponseDto>.Failure(new Error("Auth.InvalidCredentials", "Geçersiz kullanıcı adı veya şifre."));
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Result<TokenResponseDto>.Failure(new Error("Auth.InvalidCredentials", "Geçersiz kullanıcı adı veya şifre."));
            }

            TokenResponseDto tokenDto = await _jwtProvider.GenerateTokensAsync(user, cancellationToken);
            return Result<TokenResponseDto>.Success(tokenDto);
        }
    }
}

