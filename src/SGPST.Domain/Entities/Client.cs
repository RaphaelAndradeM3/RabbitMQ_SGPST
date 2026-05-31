namespace SGPST.Domain.Entities;

// Entidade que representa os clientes que solicitam suporte tecnico
public class Client
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Document { get; private set; } = string.Empty; // CPF ou CNPJ
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string AddressLine { get; private set; } = string.Empty; // Rua, numero, complemento
    public string Neighborhood { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    // Construtor protegido exigido pelo Entity Framework Core para materializacao
    protected Client() { }

    private Client(Guid id, string name, string document, string email, string phone, 
                   string addressLine, string neighborhood, string city, string state, string zipCode)
    {
        Id = id;
        Name = name;
        Document = document;
        Email = email;
        Phone = phone;
        AddressLine = addressLine;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        ZipCode = zipCode;
        IsActive = true;
    }

    // Factory Method para criacao de novos clientes
    public static Client Create(string name, string document, string email, string phone, 
                                 string addressLine, string neighborhood, string city, string state, string zipCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome do cliente nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(document))
                throw new ArgumentException("O documento (CPF/CNPJ) nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("O email nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("O telefone nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(addressLine))
                throw new ArgumentException("O endereco nao pode ser vazio.");

            return new Client(
                Guid.NewGuid(), 
                name.Trim(), 
                document.Trim(), 
                email.Trim().ToLower(), 
                phone.Trim(), 
                addressLine.Trim(), 
                neighborhood.Trim(), 
                city.Trim(), 
                state.Trim().ToUpper(), 
                zipCode.Trim()
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Falha ao criar instancia de Cliente no dominio.", ex);
        }
    }

    // Atualiza informacoes cadastrais do cliente
    public void UpdateDetails(string name, string phone, string addressLine, 
                              string neighborhood, string city, string state, string zipCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome do cliente nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("O telefone nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(addressLine))
                throw new ArgumentException("O endereco nao pode ser vazio.");

            Name = name.Trim();
            Phone = phone.Trim();
            AddressLine = addressLine.Trim();
            Neighborhood = neighborhood.Trim();
            City = city.Trim();
            State = state.Trim().ToUpper();
            ZipCode = zipCode.Trim();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao atualizar detalhes do cliente no dominio.", ex);
        }
    }

    // Altera o estado de ativacao do cliente
    public void SetActive(bool active)
    {
        try
        {
            IsActive = active;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao alterar estado de ativacao do cliente.", ex);
        }
    }
}
