using System.ComponentModel.DataAnnotations;

namespace BatalhaNaval.Application.DTOs;

public record CreateUserDto(
    [Required]
    [StringLength(50, MinimumLength = 3)]
    string Username,
    [Required] [MinLength(6)] string Password
);