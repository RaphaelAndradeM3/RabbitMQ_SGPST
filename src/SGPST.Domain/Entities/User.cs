namespace SGPST.Domain.Entities;

// Entidade que representa os usuarios do sistema (administradores, atendentes, tecnicos e clientes)
public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty; // Ex: Admin, Atendente, Tecnico, Cliente
    public bool IsActive { get; private set; }

    // Construtor protegido exigido pelo Entity Framework Core para materializacao
    protected User() { }

    private User(Guid id, string username, string email, string passwordHash, string role)
    {
        Id = id;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    // Factory Method para criacao de novos usuarios com validacoes basicas
    public static User Create(string username, string email, string passwordHash, string role)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("O nome de usuario nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("O email nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("O hash da senha nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("O perfil (role) do usuario nao pode ser vazio.");

            return new User(Guid.NewGuid(), username.Trim(), email.Trim().ToLower(), passwordHash, role.Trim());
        }
        catch (Exception ex)
        {
            // Propaga a excecao de forma controlada
            throw new InvalidOperationException("Falha ao criar instancia de Usuario no dominio.", ex);
        }
    }

    // Altera o estado de ativacao do usuario
    public void SetActive(bool active)
    {
        try
        {
            IsActive = active;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao alterar estado de ativacao do usuario.", ex);
        }
    }

    // Atualiza as informacoes basicas do usuario
    public void UpdateProfile(string username, string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("O nome de usuario nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("O email nao pode ser vazio.");

            Username = username.Trim();
            Email = email.Trim().ToLower();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao atualizar perfil do usuario no dominio.", ex);
        }
    }
}
