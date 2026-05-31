using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGPST.Domain.Interfaces;
using SGPST.Infrastructure.Data;
using SGPST.Infrastructure.Data.Repositories;

namespace SGPST.Infrastructure;

// Classe estatica para registrar os servicos de infraestrutura e persistencia
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            // Registro do contexto do banco de dados PostgreSQL
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("SGPST.Infrastructure")
                )
            );

            // Registro de todos os repositorios de persistencia
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ITechnicianRepository, TechnicianRepository>();
            services.AddScoped<IServicePriceRepository, ServicePriceRepository>();
            services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
            services.AddScoped<IDisplacementLogRepository, DisplacementLogRepository>();

            // Registro do Broker de Mensageria (RabbitMQ) como Singleton
            services.AddSingleton<IMessageBroker, SGPST.Infrastructure.Messaging.RabbitMqBroker>();

            return services;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao configurar injecao de dependencias de infraestrutura.", ex);
        }
    }
}
