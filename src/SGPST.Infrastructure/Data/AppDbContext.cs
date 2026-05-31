using Microsoft.EntityFrameworkCore;
using SGPST.Domain.Entities;

namespace SGPST.Infrastructure.Data;

// Contexto do Entity Framework Core para o SGPST Enterprise
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Technician> Technicians { get; set; } = null!;
    public DbSet<ServicePrice> ServicePrices { get; set; } = null!;
    public DbSet<SupportTicket> SupportTickets { get; set; } = null!;
    public DbSet<DisplacementLog> DisplacementLogs { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuracao da entidade User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
            entity.Property(u => u.IsActive).IsRequired();
            
            // Garantir indices unicos para email e username
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();

            // Relacionamento com Client
            entity.HasOne(u => u.Client)
                  .WithMany()
                  .HasForeignKey(u => u.ClientId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuracao da entidade Client
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Document).IsRequired().HasMaxLength(20);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Phone).IsRequired().HasMaxLength(20);
            entity.Property(c => c.AddressLine).IsRequired().HasMaxLength(250);
            entity.Property(c => c.Neighborhood).IsRequired().HasMaxLength(100);
            entity.Property(c => c.City).IsRequired().HasMaxLength(100);
            entity.Property(c => c.State).IsRequired().HasMaxLength(2);
            entity.Property(c => c.ZipCode).IsRequired().HasMaxLength(15);
            entity.Property(c => c.IsActive).IsRequired();

            entity.HasIndex(c => c.Document).IsUnique();
        });

        // Configuracao da entidade Technician
        modelBuilder.Entity<Technician>(entity =>
        {
            entity.ToTable("Technicians");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Specialty).IsRequired().HasMaxLength(100);
            entity.Property(t => t.IsAvailable).IsRequired();
            entity.Property(t => t.CurrentLocation).HasMaxLength(250);
            entity.Property(t => t.IsActive).IsRequired();

            // Relacionamento Um-para-Um ou Muitos-para-Um com User
            entity.HasOne(t => t.User)
                  .WithMany()
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuracao da entidade ServicePrice
        modelBuilder.Entity<ServicePrice>(entity =>
        {
            entity.ToTable("ServicePrices");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(150);
            entity.Property(s => s.Description).HasMaxLength(500);
            entity.Property(s => s.Price).IsRequired().HasPrecision(18, 2);
            entity.Property(s => s.IsActive).IsRequired();
        });

        // Configuracao da entidade SupportTicket
        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.ToTable("SupportTickets");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(150);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(1000);
            entity.Property(t => t.Status).IsRequired();
            entity.Property(t => t.Priority).IsRequired();
            entity.Property(t => t.Type).IsRequired();
            entity.Property(t => t.CreatedAt).IsRequired();
            entity.Property(t => t.TotalCost).IsRequired().HasPrecision(18, 2);

            // Relacionamentos com restricoes de delete em cascata para integridade referencial
            entity.HasOne(t => t.Client)
                  .WithMany()
                  .HasForeignKey(t => t.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Attendant)
                  .WithMany()
                  .HasForeignKey(t => t.AttendantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Technician)
                  .WithMany()
                  .HasForeignKey(t => t.TechnicianId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.ServicePrice)
                  .WithMany()
                  .HasForeignKey(t => t.ServicePriceId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuracao da entidade DisplacementLog
        modelBuilder.Entity<DisplacementLog>(entity =>
        {
            entity.ToTable("DisplacementLogs");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.DepartureTime).IsRequired();
            entity.Property(d => d.StartLocation).HasMaxLength(250);
            entity.Property(d => d.EndLocation).HasMaxLength(250);

            entity.HasOne(d => d.Ticket)
                  .WithMany()
                  .HasForeignKey(d => d.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
