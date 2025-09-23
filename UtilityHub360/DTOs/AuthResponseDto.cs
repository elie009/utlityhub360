namespace UtilityHub360.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = new UserDto();
    }

    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
