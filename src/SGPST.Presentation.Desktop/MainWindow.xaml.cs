using System.Windows;
using System.Windows.Controls;
using SGPST.Application.DTOs.SupportTicket;

namespace SGPST.Presentation.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Exibe usuario e perfil logados
        TxtUserLogged.Text = $"Usuario: {ApiClient.Instance.Username}";
        TxtRoleLogged.Text = $"Perfil: {ApiClient.Instance.Role}";

        var role = ApiClient.Instance.Role;

        // Restringe abas de acordo com a Role do usuario
        if (role != "Admin" && role != "Atendente")
        {
            BtnNavClients.Visibility = Visibility.Collapsed;
            BtnNavTechs.Visibility = Visibility.Collapsed;
        }

        // Seleciona a primeira pagina (Chamados) por padrao
        SwitchTab(0, "Chamados de Suporte");
        await RefreshActiveTabAsync();
    }

    private void SwitchTab(int index, string title)
    {
        MainTabControl.SelectedIndex = index;
        TxtActivePageTitle.Text = title;

        var role = ApiClient.Instance.Role;

        // Controla o botao de acao da pagina de acordo com a aba ativa
        switch (index)
        {
            case 0: // Chamados
                BtnActionPage.Content = "➕ Novo Chamado";
                BtnActionPage.Visibility = Visibility.Visible;
                break;
            case 1: // Clientes
                BtnActionPage.Content = "➕ Novo Cliente";
                BtnActionPage.Visibility = (role == "Admin" || role == "Atendente") ? Visibility.Visible : Visibility.Collapsed;
                break;
            case 2: // Tecnicos
                BtnActionPage.Visibility = Visibility.Collapsed;
                break;
            case 3: // Catalogo
                BtnActionPage.Content = "➕ Novo Servico";
                BtnActionPage.Visibility = (role == "Admin" || role == "Atendente") ? Visibility.Visible : Visibility.Collapsed;
                break;
        }
    }

    private async Task RefreshActiveTabAsync()
    {
        try
        {
            BtnRefresh.IsEnabled = false;

            switch (MainTabControl.SelectedIndex)
            {
                case 0: // Chamados
                    await LoadTicketsAsync();
                    break;
                case 1: // Clientes
                    await LoadClientsAsync();
                    break;
                case 2: // Tecnicos
                    await LoadTechniciansAsync();
                    break;
                case 3: // Catalogo
                    await LoadCatalogAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar dados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            BtnRefresh.IsEnabled = true;
        }
    }

    // 1. Carregar dados de Chamados
    private async Task LoadTicketsAsync()
    {
        var tickets = await ApiClient.Instance.GetTicketsAsync();
        
        // Aplica filtro de status local se selecionado no ComboBox
        if (CboStatusFilter.SelectedIndex > 0)
        {
            var selectedStatus = CboStatusFilter.SelectedIndex; // 1-Aberto, 2-Triagem, etc.
            tickets = tickets.Where(t => t.Status == selectedStatus);
        }

        GridTickets.ItemsSource = tickets.ToList();
    }

    // 2. Carregar dados de Clientes
    private async Task LoadClientsAsync()
    {
        var clients = await ApiClient.Instance.GetClientsAsync();
        GridClients.ItemsSource = clients.ToList();
    }

    // 3. Carregar dados de Tecnicos
    private async Task LoadTechniciansAsync()
    {
        var technicians = await ApiClient.Instance.GetTechniciansAsync();
        GridTechnicians.ItemsSource = technicians.ToList();
    }

    // 4. Carregar dados do Catalogo de Precos
    private async Task LoadCatalogAsync()
    {
        var prices = await ApiClient.Instance.GetServicePricesAsync();
        GridCatalog.ItemsSource = prices.ToList();
    }

    // Navegacao
    private async void BtnNavTickets_Click(object sender, RoutedEventArgs e)
    {
        SwitchTab(0, "Chamados de Suporte");
        await RefreshActiveTabAsync();
    }

    private async void BtnNavClients_Click(object sender, RoutedEventArgs e)
    {
        SwitchTab(1, "Clientes Cadastrados");
        await RefreshActiveTabAsync();
    }

    private async void BtnNavTechs_Click(object sender, RoutedEventArgs e)
    {
        SwitchTab(2, "Tecnicos de Atendimento");
        await RefreshActiveTabAsync();
    }

    private async void BtnNavCatalog_Click(object sender, RoutedEventArgs e)
    {
        SwitchTab(3, "Catalogo de Precos e Servicos");
        await RefreshActiveTabAsync();
    }

    private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        await RefreshActiveTabAsync();
    }

    private async void BtnActionPage_Click(object sender, RoutedEventArgs e)
    {
        switch (MainTabControl.SelectedIndex)
        {
            case 0: // Novo Chamado
                var ticketWin = new CreateTicketWindow();
                if (ticketWin.ShowDialog() == true)
                {
                    await LoadTicketsAsync();
                }
                break;
            case 1: // Novo Cliente
                var clientWin = new CreateClientWindow();
                if (clientWin.ShowDialog() == true)
                {
                    await LoadClientsAsync();
                }
                break;
            case 3: // Novo Servico no Catalogo
                var serviceWin = new CreateServiceWindow();
                if (serviceWin.ShowDialog() == true)
                {
                    await LoadCatalogAsync();
                }
                break;
        }
    }

    private async void GridTickets_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (GridTickets.SelectedItem is SupportTicketDto ticket)
        {
            var detailsWin = new TicketDetailsWindow(ticket.Id);
            detailsWin.ShowDialog();
            // Recarrega apos fechar para refletir alteracoes de status
            await LoadTicketsAsync();
        }
    }

    private void CboStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this.IsLoaded)
        {
            _ = LoadTicketsAsync();
        }
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        ApiClient.Instance.Logout();
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        this.Close();
    }
}