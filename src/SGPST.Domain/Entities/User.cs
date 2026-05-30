namespace SGPST.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private User(Guid id, string username, string email)
    {
        Id = id;
        Username = username;
        Email = email;
        IsActive = true;
    }

    public static User Create(string username, string email)
    {
        return new User(Guid.NewGuid(), username, email);
    }
}
