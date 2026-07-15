using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Extensions.Vault;

namespace AuthService.Infrastructure.Services
{
    public class JwtProvider : IJwtProvider
    {
        private readonly VaultSecrets _vaultSecrets;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public JwtProvider(VaultSecrets vaultSecrets, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
        {
            _vaultSecrets = vaultSecrets;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<TokenResponseDto> GenerateTokensAsync(User user, CancellationToken cancellationToken)
        {
            JwtSecrets jwtSecrets = _vaultSecrets.Jwt;
            if (jwtSecrets == null)
                throw new InvalidOperationException("JWT configuration is missing in Vault.");

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecrets.SecretKey));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            DateTime expiration = DateTime.UtcNow.AddMinutes(jwtSecrets.AccessTokenExpirationMinutes);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                Issuer = jwtSecrets.Issuer,
                Audience = jwtSecrets.Audience,
                SigningCredentials = credentials
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string accessToken = tokenHandler.WriteToken(securityToken);

            byte[] refreshTokenBytes = new byte[64];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(refreshTokenBytes);
            }
            string refreshToken = Convert.ToBase64String(refreshTokenBytes);

            RefreshTokenEntity refreshTokenEntity = new RefreshTokenEntity
            {
                Token = refreshToken,
                Username = user.Username,
                ExpiresAt = DateTime.UtcNow.AddDays(jwtSecrets.RefreshTokenExpirationDays),
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiration,
                Role = user.Role.ToString()
            };
        }
    }
}

