using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDataDto registerData)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerData.Email))
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            if (await _context.Users.AnyAsync(u => u.Phone == registerData.Phone))
            {
                throw new InvalidOperationException("User with this phone number already exists");
            }

            // Create new user
            var user = new Entities.User
            {
                Name = registerData.Name,
                Email = registerData.Email,
                Phone = registerData.Phone,
                Role = "USER",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate tokens
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return ApiResponse<AuthResponseDto>.SuccessResult(new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }
            });
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginCredentialsDto loginCredentials)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginCredentials.Email && u.IsActive);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // In a real application, you would verify the password hash here
            // For now, we'll assume the password is correct

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return ApiResponse<AuthResponseDto>.SuccessResult(new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }
            });
        }

        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            // In a real application, you would validate the refresh token
            // For now, we'll generate a new token
            throw new NotImplementedException("Refresh token functionality not implemented yet");
        }

        public async Task<UserDto> GetCurrentUserAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync(string userId)
        {
            // In a real application, you would invalidate the refresh token
            // For now, this is a placeholder
            await Task.CompletedTask;
        }

        private string GenerateJwtToken(Entities.User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
