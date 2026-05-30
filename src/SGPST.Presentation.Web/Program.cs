using SGPST.Application.Interfaces;
using SGPST.Application.Services;
using SGPST.Domain.Interfaces;
using SGPST.Infrastructure.Data;
using SGPST.Infrastructure.Messaging;
using SGPST.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Injeção de Dependencia - Camada de Infraestrutura
// Usando o mesmo banco SQLite da API para o Dashboard
builder.Services.AddSingleton<IDbConnectionFactory>(new SqliteConnectionFactory("Data Source=../sgpst.db"));
builder.Services.AddSingleton<IMessageBroker>(new RabbitMqBroker("localhost"));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Injeção de Dependencia - Camada de Aplicação
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Setup do Banco de Dados (SQLite)
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
    dbFactory.SetupDatabase();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
