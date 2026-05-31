using System.Windows;
using SGPST.Application.DTOs.ServicePrice;

namespace SGPST.Presentation.Desktop;

/// <summary>
/// Interaction logic for CreateServiceWindow.xaml
/// </summary>
public partial class CreateServiceWindow : Window
{
    public CreateServiceWindow()
    {
        InitializeComponent();
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var name = TxtName.Text.Trim();
        var description = TxtDescription.Text.Trim();
        var priceText = TxtPrice.Text.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(priceText))
        {
            MessageBox.Show("Preencha todos os campos do servico.", "Validacao", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Valida e converte o preco
        if (!decimal.TryParse(priceText.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price) || price < 0)
        {
            MessageBox.Show("Preco informado e invalido. Use numeros decimais maiores ou iguais a zero.", "Validacao", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            BtnSave.IsEnabled = false;
            var createDto = new CreateServicePriceDto(name, description, price);
            var result = await ApiClient.Instance.CreateServicePriceAsync(createDto);

            if (result.Success)
            {
                MessageBox.Show("Servico cadastrado com sucesso no catalogo.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Falha ao cadastrar", MessageBoxButton.OK, MessageBoxImage.Error);
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
