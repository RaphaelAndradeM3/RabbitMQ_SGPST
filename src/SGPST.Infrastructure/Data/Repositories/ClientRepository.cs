using Microsoft.EntityFrameworkCore;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Infrastructure.Data.Repositories;

// Implementacao concreta do repositorio de Clientes usando Entity Framework Core
public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;

    public ClientRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.Clients.FindAsync(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar cliente por ID: {id}", ex);
        }
    }

    public async Task<Client?> GetByDocumentAsync(string document)
    {
        try
        {
            var cleanDocument = document.Trim();
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.Document == cleanDocument);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar cliente pelo documento: {document}", ex);
        }
    }

    public async Task AddAsync(Client client)
    {
        try
        {
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao adicionar cliente no banco de dados.", ex);
        }
    }

    public async Task UpdateAsync(Client client)
    {
        try
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao atualizar cliente com ID: {client.Id}", ex);
        }
    }

    public async Task<IEnumerable<Client>> GetAllAsync()
    {
        try
        {
            return await _context.Clients.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao listar todos os clientes.", ex);
        }
    }
}
