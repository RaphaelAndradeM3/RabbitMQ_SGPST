using Microsoft.EntityFrameworkCore;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Infrastructure.Data.Repositories;

// Implementacao concreta do repositorio de Usuarios usando Entity Framework Core
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.Users.FindAsync(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar usuario por ID: {id}", ex);
        }
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar usuario por username: {username}", ex);
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            var normalizedEmail = email.Trim().ToLower();
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar usuario por email: {email}", ex);
        }
    }

    public async Task AddAsync(User user)
    {
        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao adicionar novo usuario no banco de dados.", ex);
        }
    }

    public async Task UpdateAsync(User user)
    {
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao atualizar usuario com ID: {user.Id}", ex);
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            return await _context.Users.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao listar todos os usuarios.", ex);
        }
    }
}
