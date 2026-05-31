using System.Windows;

namespace SGPST.Presentation.Desktop;

/// <summary>
/// Interaction logic for CreateTicketWindow.xaml
/// </summary>
public partial class CreateTicketWindow : Window
{
    private Guid _resolvedClientId = Guid.Empty;

    public CreateTicketWindow()
    {
        InitializeComponent();
        this.Loaded += CreateTicketWindow_Loaded;
    }

    private async void CreateTicketWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var role = ApiClient.Instance.Role;
            var username = ApiClient.Instance.Username;

            if (role == "Cliente")
            {
                // Esconde selecao de cliente e descobre o Id do cliente logado automaticamente
                PnlClientSelect.Visibility = Visibility.Collapsed;
                var clients = await ApiClient.Instance.GetClientsAsync();
                var client = clients.FirstOrDefault(c => c.Email.Contains(username ?? "", StringComparison.OrdinalIgnoreCase));
                if (client == null)
                {
                    MessageBox.Show("Nao foi possivel localizar seu cadastro de cliente na base de dados.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.DialogResult = false;
                    this.Close();
                    return;
                }
                _resolvedClientId = client.Id;
            }
            else
            {
                // Carrega todos os clientes
                var clients = await ApiClient.Instance.GetClientsAsync();
                CboClients.ItemsSource = clients.ToList();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar inicializacao: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var title = TxtTitle.Text.Trim();
        var description = TxtDescription.Text.Trim();
        var priority = CboPriority.SelectedIndex + 1; // 1-Baixa, 2-Media, 3-Alta, 4-Urgente
        var type = CboType.SelectedIndex + 1; // 1-Remoto, 2-Presencial

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
        {
            MessageBox.Show("Preencha o titulo e a descricao do chamado.", "Validacao", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Guid clientId = _resolvedClientId;

        if (ApiClient.Instance.Role != "Cliente")
        {
            if (CboClients.SelectedValue == null)
            {
                MessageBox.Show("Selecione o cliente solicitante.", "Validacao", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            clientId = (Guid)CboClients.SelectedValue;
        }

        try
        {
            BtnSave.IsEnabled = false;
            var result = await ApiClient.Instance.CreateTicketAsync(clientId, title, description, priority, type);

            if (result.Success)
            {
                MessageBox.Show("Chamado aberto com sucesso e encaminhado para triagem.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Falha ao abrir chamado", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnSave.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro de conexao: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            BtnSave.IsEnabled = true;
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        this.Close();
    }
}
