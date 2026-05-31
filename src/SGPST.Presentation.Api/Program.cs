using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SGPST.Application;
using SGPST.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Habilita o suporte a controllers na API
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// Configura o Swagger para aceitar autenticacao via JWT Bearer
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "SGPST Enterprise API", Version = "v1" });
    c.CustomSchemaIds(type => type.FullName);

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta forma: Bearer {seu_token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configura injecao de dependencias das outras camadas (Persistencia + Broker + Servicos)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Configura autenticacao por Token JWT
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "SGPST_Enterprise_Super_Secret_Key_2026_RSA_Security_Token";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Executa migrações automáticas e seeding de demonstração no banco de dados se necessário
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SGPST.Infrastructure.Data.AppDbContext>();
        await context.Database.MigrateAsync();

        var userRepository = services.GetRequiredService<SGPST.Domain.Interfaces.IUserRepository>();
        var users = await userRepository.GetAllAsync();
        if (!users.Any())
        {
            var userService = services.GetRequiredService<SGPST.Application.Interfaces.IUserService>();
            var clientService = services.GetRequiredService<SGPST.Application.Interfaces.IClientService>();
            var technicianService = services.GetRequiredService<SGPST.Application.Interfaces.ITechnicianService>();

            await userService.CreateAsync(new SGPST.Application.DTOs.User.CreateUserDto("admin", "admin@sgpst.com", "admin123", "Admin"));
            await userService.CreateAsync(new SGPST.Application.DTOs.User.CreateUserDto("atendente", "atendente@sgpst.com", "atendente123", "Atendente"));
            var techUser = await userService.CreateAsync(new SGPST.Application.DTOs.User.CreateUserDto("tecnico", "tecnico@sgpst.com", "tecnico123", "Tecnico"));
            var clientUser = await userService.CreateAsync(new SGPST.Application.DTOs.User.CreateUserDto("cliente", "cliente@sgpst.com", "cliente123", "Cliente"));

            if (clientUser.Success && clientUser.Data != null)
            {
                var companyResult = await clientService.CreateAsync(new SGPST.Application.DTOs.Client.CreateClientDto(
                    "Empresa de Tecnologia RSA Ltda",
                    "12.345.678/0001-99",
                    "cliente@sgpst.com",
                    "(11) 98888-7777",
                    "Av. Paulista, 1000 - Andar 15",
                    "Bela Vista",
                    "Sao Paulo",
                    "SP",
                    "01310-100"
                ));

                if (companyResult.Success && companyResult.Data != null)
                {
                    var dbUser = await userRepository.GetByIdAsync(clientUser.Data.Id);
                    if (dbUser != null)
                    {
                        dbUser.AssociateClient(companyResult.Data.Id);
                        await userRepository.UpdateAsync(dbUser);
                    }
                }
            }

            if (techUser.Success && techUser.Data != null)
            {
                await technicianService.CreateAsync(new SGPST.Application.DTOs.Technician.CreateTechnicianDto(
                    techUser.Data.Id,
                    "Redes de Computadores e Servidores Linux"
                ));
            }
        }
        else
        {
            // Garante que o usuario cliente ja existente seja associado se o banco ja estiver populado
            var seededClientUser = users.FirstOrDefault(u => u.Username == "cliente" && u.Role == "Cliente");
            if (seededClientUser != null && !seededClientUser.ClientId.HasValue)
            {
                var clientService = services.GetRequiredService<SGPST.Application.Interfaces.IClientService>();
                var clientsResult = await clientService.GetAllAsync();
                var seededCompany = clientsResult.Data?.FirstOrDefault(c => c.Name == "Empresa de Tecnologia RSA Ltda" || c.Email == "cliente@sgpst.com" || c.Email == "contato@rsa.com.br");
                if (seededCompany != null)
                {
                    seededClientUser.AssociateClient(seededCompany.Id);
                    await userRepository.UpdateAsync(seededClientUser);
                }
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao aplicar as migrações ou ao semear o banco de dados.");
    }
}

// Configura o pipeline de requisicoes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();

// Habilita autenticacao e autorizacao na API
app.UseAuthentication();
app.UseAuthorization();

// Mapeia as rotas dos controllers
app.MapControllers();

app.Run();
