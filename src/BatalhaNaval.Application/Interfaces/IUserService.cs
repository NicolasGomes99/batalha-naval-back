using BatalhaNaval.Application.DTOs;

namespace BatalhaNaval.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> RegisterUserAsync(CreateUserDto dto);
}