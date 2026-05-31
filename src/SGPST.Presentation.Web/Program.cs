using Microsoft.AspNetCore.Authentication.Cookies;
using SGPST.Application;
using SGPST.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Adiciona suporte a Controllers e Views (MVC)
builder.Services.AddControllersWithViews();

// Registra dependencias de persistencia (PostgreSQL) e broker (RabbitMQ)
builder.Services.AddInfrastructure(builder.Configuration);

// Registra dependencias dos servicos da camada de aplicacao
builder.Services.AddApplication();

// Configura Autenticacao via Cookies para o painel Web MVC
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

// Configura o pipeline de requisicoes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilita autenticacao e autorizacao
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
