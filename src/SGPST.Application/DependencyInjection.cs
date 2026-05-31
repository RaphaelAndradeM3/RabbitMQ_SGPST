using Microsoft.Extensions.DependencyInjection;
using SGPST.Application.Interfaces;
using SGPST.Application.Services;

namespace SGPST.Application;

// Classe estatica para registrar os servicos de aplicacao e casos de uso
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        try
        {
            // Registro de todos os servicos da camada de aplicacao
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<ITechnicianService, TechnicianService>();
            services.AddScoped<IServicePriceService, ServicePriceService>();
            services.AddScoped<ISupportTicketService, SupportTicketService>();

            return services;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao configurar injecao de dependencias de aplicacao.", ex);
        }
    }
}
