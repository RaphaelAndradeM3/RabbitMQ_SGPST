using Microsoft.EntityFrameworkCore;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Infrastructure.Data.Repositories;

// Implementacao concreta do repositorio de Deslocamentos usando Entity Framework Core
public class DisplacementLogRepository : IDisplacementLogRepository
{
    private readonly AppDbContext _context;

    public DisplacementLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DisplacementLog?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.DisplacementLogs
                .Include(d => d.Ticket)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar registro de deslocamento por ID: {id}", ex);
        }
    }

    public async Task<DisplacementLog?> GetActiveLogByTicketIdAsync(Guid ticketId)
    {
        try
        {
            // Um deslocamento ativo e aquele que possui partida (DepartureTime) mas nao tem chegada (ArrivalTime) registrada
            return await _context.DisplacementLogs
                .Include(d => d.Ticket)
                .FirstOrDefaultAsync(d => d.TicketId == ticketId && d.ArrivalTime == null);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar deslocamento ativo do chamado: {ticketId}", ex);
        }
    }

    public async Task AddAsync(DisplacementLog log)
    {
        try
        {
            await _context.DisplacementLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao salvar registro de deslocamento no banco de dados.", ex);
        }
    }

    public async Task UpdateAsync(DisplacementLog log)
    {
        try
        {
            _context.DisplacementLogs.Update(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao atualizar registro de deslocamento com ID: {log.Id}", ex);
        }
    }

    public async Task<IEnumerable<DisplacementLog>> GetByTicketIdAsync(Guid ticketId)
    {
        try
        {
            return await _context.DisplacementLogs
                .Where(d => d.TicketId == ticketId)
                .OrderBy(d => d.DepartureTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar historico de deslocamento do chamado: {ticketId}", ex);
        }
    }
}
