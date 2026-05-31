using SGPST.Application;
using SGPST.Infrastructure;
using SGPST.Service.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Registra dependencias de persistencia (PostgreSQL) e broker (RabbitMQ)
builder.Services.AddInfrastructure(builder.Configuration);

// Registra dependencias dos servicos da camada de aplicacao
builder.Services.AddApplication();

// Registra o Hosted Service do Worker (Consumer)
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
