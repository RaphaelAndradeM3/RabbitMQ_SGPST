using System.Windows;
using SGPST.Application.DTOs.Client;

namespace SGPST.Presentation.Desktop;

/// <summary>
/// Interaction logic for CreateClientWindow.xaml
/// </summary>
public partial class CreateClientWindow : Window
{
    public CreateClientWindow()
    {
        InitializeComponent();
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var name = TxtName.Text.Trim();
        var document = TxtDocument.Text.Trim();
        var email = TxtEmail.Text.Trim();
        var phone = TxtPhone.Text.Trim();
        var address = TxtAddress.Text.Trim();
        var zip = TxtZipCode.Text.Trim();
        var neighborhood = TxtNeighborhood.Text.Trim();
        var city = TxtCity.Text.Trim();
        var state = TxtState.Text.Trim().ToUpper();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(document) || string.IsNullOrEmpty(email) || 
            string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(zip) || 
            string.IsNullOrEmpty(neighborhood) || string.IsNullOrEmpty(city) || string.IsNullOrEmpty(state))
        {
            MessageBox.Show("Preencha todos os campos do cadastro do cliente.", "Validacao", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            BtnSave.IsEnabled = false;
            var createDto = new CreateClientDto(name, document, email, phone, address, neighborhood, city, state, zip);
            var result = await ApiClient.Instance.CreateClientAsync(createDto);

            if (result.Success)
            {
                MessageBox.Show("Cliente cadastrado com sucesso na base corporativa.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Falha ao cadastrar cliente", MessageBoxButton.OK, MessageBoxImage.Error);
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
