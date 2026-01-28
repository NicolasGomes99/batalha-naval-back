using BatalhaNaval.Domain.Entities;

namespace BatalhaNaval.Domain.Interfaces;

public interface IUserRepository
{
    Task<User> AddAsync(User user);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
}