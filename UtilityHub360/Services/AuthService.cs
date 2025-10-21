using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using BCrypt.Net;

namespace UtilityHub360.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailService _emailService;

        public AuthService(ApplicationDbContext context, IConfiguration configuration, IEmailService emailService, JwtSettings jwtSettings)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _jwtSettings = jwtSettings;
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDataDto registerData)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerData.Email))
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Only check for phone number uniqueness if phone number is provided
            if (!string.IsNullOrEmpty(registerData.Phone) && 
                await _context.Users.AnyAsync(u => u.Phone == registerData.Phone))
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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerData.Password),
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

            // Verify the password
            if (!BCrypt.Net.BCrypt.Verify(loginCredentials.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

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
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

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
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey); 

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

        public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                // Check if user exists
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email && u.IsActive);

                if (user == null)
                {
                    // For security, don't reveal if email exists or not
                    return ApiResponse<object>.SuccessResult(new { }, "If the email exists, a password reset link has been sent.");
                }

                // Generate reset token
                var resetToken = Guid.NewGuid().ToString();
                var expiresAt = DateTime.UtcNow.AddHours(1); // Token expires in 1 hour

                // Invalidate any existing reset tokens for this user
                var existingResets = await _context.PasswordResets
                    .Where(pr => pr.UserId == user.Id && !pr.IsUsed)
                    .ToListAsync();

                foreach (var existingReset in existingResets)
                {
                    existingReset.IsUsed = true;
                    existingReset.UsedAt = DateTime.UtcNow;
                }

                // Create new password reset record
                var passwordReset = new Entities.PasswordReset
                {
                    UserId = user.Id,
                    Token = resetToken,
                    Email = user.Email,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IsUsed = false
                };

                _context.PasswordResets.Add(passwordReset);
                await _context.SaveChangesAsync();

                // Send email
                await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, user.Name);

                return ApiResponse<object>.SuccessResult(new { }, "If the email exists, a password reset link has been sent.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to process password reset request: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                // Find the password reset record
                var passwordReset = await _context.PasswordResets
                    .FirstOrDefaultAsync(pr => pr.Token == resetPasswordDto.Token && 
                                             pr.Email == resetPasswordDto.Email && 
                                             !pr.IsUsed && 
                                             pr.ExpiresAt > DateTime.UtcNow);

                if (passwordReset == null)
                {
                    throw new InvalidOperationException("Invalid or expired reset token.");
                }

                // Find the user
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == passwordReset.UserId && u.IsActive);

                if (user == null)
                {
                    throw new InvalidOperationException("User not found.");
                }

                // Update user password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                // Mark reset token as used
                passwordReset.IsUsed = true;
                passwordReset.UsedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(new { }, "Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to reset password: {ex.Message}");
            }
        }
    }
}
