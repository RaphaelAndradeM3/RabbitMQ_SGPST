using System.Windows;

namespace SGPST.Presentation.Desktop;

/// <summary>
/// Interaction logic for LoginWindow.xaml
/// </summary>
public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        var username = TxtUsername.Text.Trim();
        var password = TxtPassword.Password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            LblStatus.Text = "Preencha todos os campos.";
            return;
        }

        try
        {
            // Desabilita controles durante a autenticacao
            SetControlsEnabled(false);
            LblStatus.Foreground = System.Windows.Media.Brushes.LightGray;
            LblStatus.Text = "Autenticando...";

            var result = await ApiClient.Instance.LoginAsync(username, password);

            if (result.Success)
            {
                // Abre a janela principal e fecha a de login
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                LblStatus.Foreground = System.Windows.Media.Brushes.Red;
                LblStatus.Text = result.Message;
                SetControlsEnabled(true);
            }
        }
        catch (Exception ex)
        {
            LblStatus.Foreground = System.Windows.Media.Brushes.Red;
            LblStatus.Text = $"Erro de processamento: {ex.Message}";
            SetControlsEnabled(true);
        }
    }

    private void SetControlsEnabled(bool enabled)
    {
        TxtUsername.IsEnabled = enabled;
        TxtPassword.IsEnabled = enabled;
        BtnLogin.IsEnabled = enabled;
    }
}
