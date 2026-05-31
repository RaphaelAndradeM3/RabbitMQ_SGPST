using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using SGPST.Application.DTOs;
using SGPST.Domain.Common;

namespace SGPST.Presentation.Desktop;

public partial class MainWindow : Window
{
    private readonly HttpClient _httpClient;

    public MainWindow()
    {
        InitializeComponent();

        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:5042/");
        
        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Orders");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AppResult<IEnumerable<OrderDto>>>();
                dgOrders.ItemsSource = result?.Data;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao carregar dados: {ex.Message}");
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadData();
    }
}
