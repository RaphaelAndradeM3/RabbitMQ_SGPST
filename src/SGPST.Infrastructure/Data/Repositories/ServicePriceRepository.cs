using Microsoft.EntityFrameworkCore;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Infrastructure.Data.Repositories;

// Implementacao concreta do repositorio de Servicos e Precos usando Entity Framework Core
public class ServicePriceRepository : IServicePriceRepository
{
    private readonly AppDbContext _context;

    public ServicePriceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServicePrice?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.ServicePrices.FindAsync(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar preco de servico por ID: {id}", ex);
        }
    }

    public async Task AddAsync(ServicePrice servicePrice)
    {
        try
        {
            await _context.ServicePrices.AddAsync(servicePrice);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao adicionar servico e preco no banco de dados.", ex);
        }
    }

    public async Task UpdateAsync(ServicePrice servicePrice)
    {
        try
        {
            _context.ServicePrices.Update(servicePrice);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao atualizar preco de servico com ID: {servicePrice.Id}", ex);
        }
    }

    public async Task<IEnumerable<ServicePrice>> GetAllAsync()
    {
        try
        {
            return await _context.ServicePrices.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao listar todos os precos de servico do catalogo.", ex);
        }
    }

    public async Task<IEnumerable<ServicePrice>> GetActiveAsync()
    {
        try
        {
            return await _context.ServicePrices
                .Where(s => s.IsActive)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao buscar precos de servicos ativos no catalogo.", ex);
        }
    }
}
