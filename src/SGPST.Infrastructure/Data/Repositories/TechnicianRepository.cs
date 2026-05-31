using Microsoft.EntityFrameworkCore;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Infrastructure.Data.Repositories;

// Implementacao concreta do repositorio de Tecnicos usando Entity Framework Core
public class TechnicianRepository : ITechnicianRepository
{
    private readonly AppDbContext _context;

    public TechnicianRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Technician?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.Technicians
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar tecnico por ID: {id}", ex);
        }
    }

    public async Task<Technician?> GetByUserIdAsync(Guid userId)
    {
        try
        {
            return await _context.Technicians
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar tecnico pelo UserId: {userId}", ex);
        }
    }

    public async Task AddAsync(Technician technician)
    {
        try
        {
            await _context.Technicians.AddAsync(technician);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao adicionar tecnico no banco de dados.", ex);
        }
    }

    public async Task UpdateAsync(Technician technician)
    {
        try
        {
            _context.Technicians.Update(technician);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao atualizar cadastro do tecnico com ID: {technician.Id}", ex);
        }
    }

    public async Task<IEnumerable<Technician>> GetAllAsync()
    {
        try
        {
            return await _context.Technicians
                .Include(t => t.User)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao listar todos os tecnicos.", ex);
        }
    }

    public async Task<IEnumerable<Technician>> GetAvailableAsync()
    {
        try
        {
            return await _context.Technicians
                .Include(t => t.User)
                .Where(t => t.IsAvailable && t.IsActive)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao listar tecnicos disponiveis.", ex);
        }
    }
}
