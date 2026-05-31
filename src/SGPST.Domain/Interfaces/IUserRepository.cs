using SGPST.Domain.Entities;

namespace SGPST.Domain.Interfaces;

// Interface de contrato para persistencia da entidade de Usuario
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<IEnumerable<User>> GetAllAsync();
}
