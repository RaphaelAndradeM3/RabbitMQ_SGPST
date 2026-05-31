using SGPST.Application.DTOs.SupportTicket;

namespace SGPST.Presentation.Web.Models;

// ViewModel para agregar dados estatisticos e operacionais na Home do Dashboard
public class DashboardViewModel
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InTriageTickets { get; set; }
    public int InDisplacementTickets { get; set; }
    public int InServiceTickets { get; set; }
    public int CompletedTickets { get; set; }
    public int CancelledTickets { get; set; }
    
    public int TotalClients { get; set; }
    public int TotalTechnicians { get; set; }
    public decimal TotalBilling { get; set; }

    public IEnumerable<SupportTicketDto> RecentTickets { get; set; } = new List<SupportTicketDto>();
}
