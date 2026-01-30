namespace BatalhaNaval.Application.DTOs;

public class TokenResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string Username { get; set; }
    public UserProfileDTO Profile { get; set; }
}