using Microsoft.EntityFrameworkCore;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Infrastructure.Data.Repositories;

// Implementacao concreta do repositorio de Chamados (SupportTicket) usando Entity Framework Core
public class SupportTicketRepository : ISupportTicketRepository
{
    private readonly AppDbContext _context;

    public SupportTicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SupportTicket?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.SupportTickets
                .Include(t => t.Client)
                .Include(t => t.Attendant)
                .Include(t => t.Technician)
                    .ThenInclude(tec => tec!.User)
                .Include(t => t.ServicePrice)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar chamado por ID: {id}", ex);
        }
    }

    public async Task AddAsync(SupportTicket ticket)
    {
        try
        {
            await _context.SupportTickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao salvar chamado no banco de dados.", ex);
        }
    }

    public async Task UpdateAsync(SupportTicket ticket)
    {
        try
        {
            _context.SupportTickets.Update(ticket);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao atualizar informacoes do chamado com ID: {ticket.Id}", ex);
        }
    }

    public async Task<IEnumerable<SupportTicket>> GetAllAsync()
    {
        try
        {
            return await _context.SupportTickets
                .Include(t => t.Client)
                .Include(t => t.Attendant)
                .Include(t => t.Technician)
                    .ThenInclude(tec => tec!.User)
                .Include(t => t.ServicePrice)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao listar todos os chamados.", ex);
        }
    }

    public async Task<IEnumerable<SupportTicket>> GetByStatusAsync(TicketStatus status)
    {
        try
        {
            return await _context.SupportTickets
                .Include(t => t.Client)
                .Include(t => t.Attendant)
                .Include(t => t.Technician)
                    .ThenInclude(tec => tec!.User)
                .Include(t => t.ServicePrice)
                .Where(t => t.Status == status)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao listar chamados pelo status: {status}", ex);
        }
    }

    public async Task<IEnumerable<SupportTicket>> GetByClientIdAsync(Guid clientId)
    {
        try
        {
            return await _context.SupportTickets
                .Include(t => t.Client)
                .Include(t => t.Attendant)
                .Include(t => t.Technician)
                    .ThenInclude(tec => tec!.User)
                .Include(t => t.ServicePrice)
                .Where(t => t.ClientId == clientId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar chamados do cliente com ID: {clientId}", ex);
        }
    }

    public async Task<IEnumerable<SupportTicket>> GetByTechnicianIdAsync(Guid technicianId)
    {
        try
        {
            return await _context.SupportTickets
                .Include(t => t.Client)
                .Include(t => t.Attendant)
                .Include(t => t.Technician)
                    .ThenInclude(tec => tec!.User)
                .Include(t => t.ServicePrice)
                .Where(t => t.TechnicianId == technicianId ||
                            (t.TechnicianId == null && (t.Status == TicketStatus.Aberto || t.Status == TicketStatus.EmTriagem)))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar chamados atribuidos ao tecnico com ID: {technicianId}", ex);
        }
    }
}
