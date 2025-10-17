using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDataDto registerData);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginCredentialsDto loginCredentials);
        Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
        Task<UserDto> GetCurrentUserAsync(string userId);
        Task<bool> ValidateTokenAsync(string token);
        Task LogoutAsync(string userId);
        Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}
