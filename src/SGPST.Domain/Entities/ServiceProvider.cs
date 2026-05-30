namespace SGPST.Domain.Entities;

public class ServiceProvider
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsAvailable { get; private set; }

    private ServiceProvider(Guid id, string name)
    {
        Id = id;
        Name = name;
        IsAvailable = true;
    }

    public static ServiceProvider Create(string name)
    {
        return new ServiceProvider(Guid.NewGuid(), name);
    }

    public void SetAvailability(bool available)
    {
        IsAvailable = available;
    }
}
