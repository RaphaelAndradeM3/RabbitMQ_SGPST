using System.Windows;
using System.Windows.Media;
using SGPST.Application.DTOs.SupportTicket;

namespace SGPST.Presentation.Desktop;

/// <summary>
/// Interaction logic for TicketDetailsWindow.xaml
/// </summary>
public partial class TicketDetailsWindow : Window
{
    private readonly Guid _ticketId;

    public TicketDetailsWindow(Guid ticketId)
    {
        InitializeComponent();
        _ticketId = ticketId;
        this.Loaded += TicketDetailsWindow_Loaded;
    }

    private async void TicketDetailsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadTicketDetailsAsync();
    }

    private async Task LoadTicketDetailsAsync()
    {
        try
        {
            // 1. Carrega dados básicos do chamado da API
            var ticket = await ApiClient.Instance.GetTicketByIdAsync(_ticketId);
            if (ticket == null)
            {
                MessageBox.Show("Chamado nao encontrado na base de dados.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            // 2. Preenche os campos textuais
            TxtTicketId.Text = $"ID: #{ticket.Id.ToString().ToUpper()}";
            TxtTitle.Text = ticket.Title;
            TxtStatus.Text = ticket.StatusDescription.ToUpper();
            TxtClient.Text = ticket.ClientName;
            TxtPriority.Text = ticket.PriorityDescription;
            TxtType.Text = ticket.TypeDescription;
            TxtCreatedAt.Text = ticket.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            TxtAttendant.Text = string.IsNullOrEmpty(ticket.AttendantName) ? "Nao atribuido" : ticket.AttendantName;
            TxtTechnician.Text = string.IsNullOrEmpty(ticket.TechnicianName) ? "Nao designado" : ticket.TechnicianName;
            TxtServicePrice.Text = string.IsNullOrEmpty(ticket.ServicePriceName) ? "Nao faturado" : ticket.ServicePriceName;
            
            var culturePt = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
            TxtTotalCost.Text = ticket.TotalCost.ToString("C2", culturePt);
            TxtDescription.Text = ticket.Description;

            // 3. Estiliza a tag do status
            StyleStatusTag(ticket.Status);

            // 4. Carrega historico de deslocamentos (se for chamado presencial)
            List<DisplacementLogDto> displacements = new();
            if (ticket.Type == 2) // 2-Presencial
            {
                PnlDisplacements.Visibility = Visibility.Visible;
                var logs = await ApiClient.Instance.GetDisplacementsAsync(_ticketId);
                displacements = logs.ToList();
                GridDisplacements.ItemsSource = displacements;
            }
            else
            {
                PnlDisplacements.Visibility = Visibility.Collapsed;
            }

            // 5. Configura visibilidade das acoes com base no status e perfil do usuario logado
            ConfigureActionPanels(ticket, displacements);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar detalhes: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StyleStatusTag(int status)
    {
        var brushBg = new SolidColorBrush(Color.FromRgb(30, 32, 48)); // default dark gray
        
        switch (status)
        {
            case 1: // Aberto
                brushBg = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // red
                break;
            case 2: // EmTriagem
                brushBg = new SolidColorBrush(Color.FromRgb(245, 158, 11)); // orange
                break;
            case 3: // EmDeslocamento
                brushBg = new SolidColorBrush(Color.FromRgb(59, 130, 246)); // blue
                break;
            case 4: // EmAtendimento
                brushBg = new SolidColorBrush(Color.FromRgb(124, 77, 255)); // purple
                break;
            case 5: // Concluido
                brushBg = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // green
                break;
            case 6: // Cancelado
                brushBg = new SolidColorBrush(Color.FromRgb(107, 114, 128)); // gray
                break;
        }

        BdrStatus.Background = brushBg;
    }

    private async void ConfigureActionPanels(SupportTicketDto ticket, List<DisplacementLogDto> displacements)
    {
        var role = ApiClient.Instance.Role;
        var status = ticket.Status;
        var hasActiveDisplacement = displacements.Any(d => d.ArrivalTime == null);

        // Oculta todas as acoes inicialmente
        PnlAssignAttendant.Visibility = Visibility.Collapsed;
        PnlAssignTechnician.Visibility = Visibility.Collapsed;
        PnlStartDisplacement.Visibility = Visibility.Collapsed;
        PnlEndDisplacement.Visibility = Visibility.Collapsed;
        PnlCompleteTicket.Visibility = Visibility.Collapsed;
        PnlClosedInfo.Visibility = Visibility.Collapsed;
        PnlCancelTicket.Visibility = Visibility.Collapsed;

        // Se o chamado ja estiver concluido ou cancelado
        if (status == 5 || status == 6)
        {
            PnlClosedInfo.Visibility = Visibility.Visible;
            TxtClosedMsg.Text = status == 5 
                ? $"Chamado concluido e finalizado em {ticket.ClosedAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm")}. Valor total faturado: {TxtTotalCost.Text}."
                : "Este chamado foi cancelado e encerrado no sistema.";
            return;
        }

        // Exibe o painel de cancelamento para chamados nao finalizados
        PnlCancelTicket.Visibility = Visibility.Visible;

        // Painel condicional dependendo do status e role do usuario
        if (status == 1) // Aberto
        {
            if (role == "Admin" || role == "Atendente")
            {
                PnlAssignAttendant.Visibility = Visibility.Visible;
            }
        }
        else if (status == 2) // EmTriagem
        {
            if (role == "Admin" || role == "Atendente")
            {
                PnlAssignTechnician.Visibility = Visibility.Visible;
                try
                {
                    var technicians = await ApiClient.Instance.GetTechniciansAsync();
                    CboTechnicians.ItemsSource = technicians.Where(t => t.IsActive).ToList();
                }
                catch
                {
                    // falha silenciosa no combobox
                }
            }
        }
        else if (status == 3) // EmDeslocamento
        {
            if (role == "Admin" || role == "Tecnico")
            {
                if (!hasActiveDisplacement)
                {
                    PnlStartDisplacement.Visibility = Visibility.Visible;
                }
                else
                {
                    PnlEndDisplacement.Visibility = Visibility.Visible;
                }
            }
        }
        else if (status == 4) // EmAtendimento
        {
            if (role == "Admin" || role == "Atendente" || role == "Tecnico")
            {
                PnlCompleteTicket.Visibility = Visibility.Visible;
                try
                {
                    var prices = await ApiClient.Instance.GetServicePricesAsync();
                    CboServicePrices.ItemsSource = prices.Where(p => p.IsActive).ToList();
                }
                catch
                {
                    // falha silenciosa
                }
            }
        }
    }

    private async void BtnAssignAttendant_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var attendantId = ApiClient.Instance.UserId ?? Guid.Empty;
            if (attendantId == Guid.Empty)
            {
                MessageBox.Show("Nao foi possivel identificar o ID do atendente logado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BtnAssignAttendant.IsEnabled = false;
            var result = await ApiClient.Instance.AssignAttendantAsync(_ticketId, attendantId);

            if (result.Success)
            {
                MessageBox.Show("Você assumiu a triagem deste chamado.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadTicketDetailsAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Falha", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnAssignAttendant.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro de conexao: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            BtnAssignAttendant.IsEnabled = true;
        }
    }

    private async void BtnAssignTechnician_Click(object sender, RoutedEventArgs e)
    {
        if (CboTechnicians.SelectedValue == null)
        {
            MessageBox.Show("Selecione um tecnico para designacao.", "Validacao", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var techId = (Guid)CboTechnicians.SelectedValue;

        try
        {
            BtnAssignTechnician.IsEnabled = false;
            var result = await ApiClient.Instance.AssignTechnicianAsync(_ticketId, techId);

            if (result.Success)
            {
                MessageBox.Show("Tecnico designado com sucesso. O chamado foi atualizado.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadTicketDetailsAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Falha", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnAssignTechnician.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            BtnAssignTechnician.IsEnabled = true;
        }
    }

    private async void BtnStartDisplacement_Click(object sender, RoutedEventArgs e)
    {
        var startLoc = TxtStartLocation.Text.Trim();
        if (string.IsNullOrEmpty(startLoc)) startLoc = "Base Operacional";

        try
        {
            BtnStartDisplacement.IsEnabled = false;
            var result = await ApiClient.Instance.StartDisplacementAsync(_ticketId, startLoc);

            if (result.Success)
            {
                MessageBox.Show("Deslocamento do tecnico iniciado com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadTicketDetailsAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Falha", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnStartDisplacement.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            BtnStartDisplacement.IsEnabled = true;
        }
    }

    private async void BtnEndDisplacement_Click(object sender, RoutedEventArgs e)
    {
        var endLoc = TxtEndLocation.Text.Trim();
        if (string.IsNullOrEmpty(endLoc)) endLoc = "Local do Cliente";

        try
        {
            BtnEndDisplacement.IsEnabled = false;
            var result = await ApiClient.Instance.EndDisplacementAsync(_ticketId, endLoc);

            if (result.Success)
            {
                MessageBox.Show("Chegada registrada. Chamado colocado em atendimento fisico.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadTicketDetailsAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Falha", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnEndDisplacement.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            BtnEndDisplacement.IsEnabled = true;
        }
    }

    private async void BtnCompleteTicket_Click(object sender, RoutedEventArgs e)
    {
        if (CboServicePrices.SelectedValue == null)
        {
            MessageBox.Show("Selecione o servico executado para faturamento.", "Validacao", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var serviceId = (Guid)CboServicePrices.SelectedValue;
        var extraCostText = TxtExtraCost.Text.Trim();

        if (!decimal.TryParse(extraCostText.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal extraCost) || extraCost < 0)
        {
            MessageBox.Show("Custo extra informado e invalido.", "Validacao", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            BtnCompleteTicket.IsEnabled = false;
            var result = await ApiClient.Instance.CompleteTicketAsync(_ticketId, serviceId, extraCost);

            if (result.Success)
            {
                MessageBox.Show("Chamado finalizado e faturado com sucesso.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadTicketDetailsAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Falha", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnCompleteTicket.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            BtnCompleteTicket.IsEnabled = true;
        }
    }

    private async void BtnCancelTicket_Click(object sender, RoutedEventArgs e)
    {
        var reason = TxtCancelReason.Text.Trim();
        if (string.IsNullOrEmpty(reason))
        {
            MessageBox.Show("Informe o motivo do cancelamento.", "Validacao", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirm = MessageBox.Show("Deseja realmente cancelar este chamado?", "Confirmacao", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            BtnCancelTicket.IsEnabled = false;
            var result = await ApiClient.Instance.CancelTicketAsync(_ticketId, reason);

            if (result.Success)
            {
                MessageBox.Show("Chamado cancelado com sucesso.", "Cancelado", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadTicketDetailsAsync();
            }
            else
            {
                MessageBox.Show(result.Message, "Falha", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnCancelTicket.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            BtnCancelTicket.IsEnabled = true;
        }
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
