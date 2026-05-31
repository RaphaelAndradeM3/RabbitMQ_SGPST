namespace SGPST.Domain.Entities;

// Entidade que representa o catalogo de servicos e seus respectivos valores de faturamento
public class ServicePrice
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }

    // Construtor protegido exigido pelo Entity Framework Core para materializacao
    protected ServicePrice() { }

    private ServicePrice(Guid id, string name, string description, decimal price)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        IsActive = true;
    }

    // Factory Method para criacao de novos servicos
    public static ServicePrice Create(string name, string description, decimal price)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome do servico nao pode ser vazio.");
            if (price < 0)
                throw new ArgumentException("O preco do servico nao pode ser menor que zero.");

            return new ServicePrice(Guid.NewGuid(), name.Trim(), description.Trim(), price);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Falha ao criar instancia de Catalogo de Precos no dominio.", ex);
        }
    }

    // Atualiza preco e detalhes do servico
    public void UpdateService(string name, string description, decimal price)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome do servico nao pode ser vazio.");
            if (price < 0)
                throw new ArgumentException("O preco do servico nao pode ser menor que zero.");

            Name = name.Trim();
            Description = description.Trim();
            Price = price;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao atualizar servico e preco no dominio.", ex);
        }
    }

    // Altera o estado de ativacao do servico no catalogo
    public void SetActive(bool active)
    {
        try
        {
            IsActive = active;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao alterar estado de ativacao do preco de servico.", ex);
        }
    }
}
