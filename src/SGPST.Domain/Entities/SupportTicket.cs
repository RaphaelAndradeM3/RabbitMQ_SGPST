namespace SGPST.Domain.Entities;

public enum TicketStatus
{
    Aberto = 1,
    EmTriagem = 2,
    EmDeslocamento = 3,
    EmAtendimento = 4,
    Concluido = 5,
    Cancelado = 6
}

public enum TicketPriority
{
    Baixa = 1,
    Media = 2,
    Alta = 3,
    Urgente = 4
}

public enum TicketType
{
    Remoto = 1,
    Presencial = 2
}

// Entidade principal que representa o pedido de suporte tecnico (chamado / ordem de servico)
public class SupportTicket
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public Client Client { get; private set; } = null!;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    
    public Guid? AttendantId { get; private set; }
    public User? Attendant { get; private set; }
    
    public Guid? TechnicianId { get; private set; }
    public Technician? Technician { get; private set; }
    
    public Guid? ServicePriceId { get; private set; }
    public ServicePrice? ServicePrice { get; private set; }

    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TicketType Type { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? ScheduledFor { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public decimal TotalCost { get; private set; }

    // Construtor protegido exigido pelo Entity Framework Core para materializacao
    protected SupportTicket() { }

    private SupportTicket(Guid id, Guid clientId, string title, string description, TicketPriority priority, TicketType type)
    {
        Id = id;
        ClientId = clientId;
        Title = title;
        Description = description;
        Priority = priority;
        Type = type;
        Status = TicketStatus.Aberto;
        CreatedAt = DateTime.UtcNow;
        TotalCost = 0;
    }

    // Factory Method para abertura de novos chamados pelo cliente
    public static SupportTicket Create(Guid clientId, string title, string description, TicketPriority priority, TicketType type)
    {
        try
        {
            if (clientId == Guid.Empty)
                throw new ArgumentException("O ID do cliente nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("O titulo do chamado nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("A descricao do problema nao pode ser vazia.");

            return new SupportTicket(Guid.NewGuid(), clientId, title.Trim(), description.Trim(), priority, type);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Falha ao criar instancia de Chamado no dominio.", ex);
        }
    }

    // Atribuicao de atendente para triagem do chamado
    public void AssignAttendant(Guid attendantId)
    {
        try
        {
            if (attendantId == Guid.Empty)
                throw new ArgumentException("O ID do atendente nao pode ser vazio.");

            AttendantId = attendantId;
            Status = TicketStatus.EmTriagem;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao atribuir atendente ao chamado no dominio.", ex);
        }
    }

    // Designacao de tecnico para o chamado
    public void AssignTechnician(Guid technicianId)
    {
        try
        {
            if (technicianId == Guid.Empty)
                throw new ArgumentException("O ID do tecnico nao pode ser vazio.");

            TechnicianId = technicianId;
            
            // Se for presencial, entra em deslocamento; se for remoto, ja entra em atendimento
            Status = Type == TicketType.Presencial 
                ? TicketStatus.EmDeslocamento 
                : TicketStatus.EmAtendimento;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao designar tecnico no dominio.", ex);
        }
    }

    // Iniciar atendimento presencial (tecnico chegou ao local)
    public void StartPhysicalService()
    {
        try
        {
            if (Status != TicketStatus.EmDeslocamento)
                throw new InvalidOperationException("So e possivel iniciar o atendimento fisico se o tecnico estiver em deslocamento.");

            Status = TicketStatus.EmAtendimento;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao registrar inicio de atendimento fisico.", ex);
        }
    }

    // Associar o servico prestado do catalogo e faturar o chamado
    public void LinkServiceAndBilling(Guid servicePriceId, decimal price, decimal extraCost = 0)
    {
        try
        {
            if (servicePriceId == Guid.Empty)
                throw new ArgumentException("O ID do servico nao pode ser vazio.");
            if (price < 0 || extraCost < 0)
                throw new ArgumentException("Precos e custos adicionais nao podem ser negativos.");

            ServicePriceId = servicePriceId;
            TotalCost = price + extraCost;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao aplicar faturamento ao chamado no dominio.", ex);
        }
    }

    // Finalizar o chamado com sucesso
    public void Complete()
    {
        try
        {
            if (Status == TicketStatus.Concluido || Status == TicketStatus.Cancelado)
                throw new InvalidOperationException("Chamados ja finalizados ou cancelados nao podem ser alterados.");

            Status = TicketStatus.Concluido;
            ClosedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao concluir o chamado.", ex);
        }
    }

    // Cancelar o chamado
    public void Cancel(string reason)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("A razao do cancelamento deve ser informada.");

            Status = TicketStatus.Cancelado;
            ClosedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao cancelar o chamado.", ex);
        }
    }

    // Agendar o chamado
    public void ScheduleFor(DateTime scheduledTime)
    {
        try
        {
            if (scheduledTime < DateTime.UtcNow)
                throw new ArgumentException("A data de agendamento nao pode ser no passado.");

            ScheduledFor = scheduledTime;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao agendar o chamado.", ex);
        }
    }
}
