namespace SGPST.Domain.Entities;

// Entidade que representa o registro de deslocamento do tecnico para atendimentos presenciais
public class DisplacementLog
{
    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public SupportTicket Ticket { get; private set; } = null!;
    public DateTime DepartureTime { get; private set; } // Horario de partida do tecnico
    public DateTime? ArrivalTime { get; private set; }  // Horario de chegada no cliente
    public string? StartLocation { get; private set; }
    public string? EndLocation { get; private set; }

    // Construtor protegido exigido pelo Entity Framework Core para materializacao
    protected DisplacementLog() { }

    private DisplacementLog(Guid id, Guid ticketId, DateTime departureTime, string? startLocation)
    {
        Id = id;
        TicketId = ticketId;
        DepartureTime = departureTime;
        StartLocation = startLocation;
    }

    // Factory Method para registrar uma nova partida de deslocamento
    public static DisplacementLog Create(Guid ticketId, string? startLocation)
    {
        try
        {
            if (ticketId == Guid.Empty)
                throw new ArgumentException("O ID do chamado nao pode ser vazio para iniciar o deslocamento.");

            return new DisplacementLog(Guid.NewGuid(), ticketId, DateTime.UtcNow, startLocation?.Trim());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Falha ao registrar deslocamento no dominio.", ex);
        }
    }

    // Registra a chegada do tecnico no local do atendimento
    public void RecordArrival(string? endLocation)
    {
        try
        {
            if (ArrivalTime.HasValue)
                throw new InvalidOperationException("O horario de chegada ja foi registrado para este deslocamento.");

            ArrivalTime = DateTime.UtcNow;
            EndLocation = endLocation?.Trim();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao registrar chegada do tecnico.", ex);
        }
    }
}
