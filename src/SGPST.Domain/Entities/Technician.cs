namespace SGPST.Domain.Entities;

// Entidade que representa os tecnicos que executam os chamados de suporte
public class Technician
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Specialty { get; private set; } = string.Empty;
    public bool IsAvailable { get; private set; }
    public string? CurrentLocation { get; private set; } // Coordenadas ou descricao de localizacao atual
    public bool IsActive { get; private set; }

    // Construtor protegido exigido pelo Entity Framework Core para materializacao
    protected Technician() { }

    private Technician(Guid id, Guid userId, string specialty)
    {
        Id = id;
        UserId = userId;
        Specialty = specialty;
        IsAvailable = true;
        IsActive = true;
    }

    // Factory Method para criacao de tecnicos vinculados a um usuario existente
    public static Technician Create(Guid userId, string specialty)
    {
        try
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("O ID do usuario vinculado nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(specialty))
                throw new ArgumentException("A especialidade do tecnico nao pode ser vazia.");

            return new Technician(Guid.NewGuid(), userId, specialty.Trim());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Falha ao criar instancia de Tecnico no dominio.", ex);
        }
    }

    // Atualiza a especialidade
    public void UpdateSpecialty(string specialty)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(specialty))
                throw new ArgumentException("A especialidade nao pode ser vazia.");

            Specialty = specialty.Trim();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao atualizar especialidade do tecnico no dominio.", ex);
        }
    }

    // Altera a disponibilidade do tecnico
    public void SetAvailability(bool isAvailable)
    {
        try
        {
            IsAvailable = isAvailable;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao alterar disponibilidade do tecnico.", ex);
        }
    }

    // Atualiza localizacao atual do tecnico
    public void UpdateLocation(string location)
    {
        try
        {
            CurrentLocation = location?.Trim();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao atualizar localizacao do tecnico no dominio.", ex);
        }
    }

    // Altera o estado de ativacao do cadastro de tecnico
    public void SetActive(bool active)
    {
        try
        {
            IsActive = active;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao alterar estado de ativacao do tecnico.", ex);
        }
    }
}
