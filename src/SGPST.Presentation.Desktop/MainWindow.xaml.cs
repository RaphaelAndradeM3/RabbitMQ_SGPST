using System.Windows;
using SGPST.Application.Interfaces;
using SGPST.Application.Services;
using SGPST.Infrastructure.Data;
using SGPST.Infrastructure.Messaging;
using SGPST.Infrastructure.Repositories;

namespace SGPST.Presentation.Desktop;

public partial class MainWindow : Window
{
    private readonly IOrderService _orderService;

    public MainWindow()
    {
        InitializeComponent();

        // Setup manual do DI para o prototipo Desktop
        var dbFactory = new SqliteConnectionFactory("Data Source=../sgpst.db");
        var broker = new RabbitMqBroker("localhost");
        var orderRepo = new OrderRepository(dbFactory);
        var userRepo = new UserRepository(dbFactory);
        
        _orderService = new OrderService(orderRepo, broker);
        
        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            var result = await _orderService.GetAllOrdersAsync();
            if (result.Success)
            {
                dgOrders.ItemsSource = result.Data;
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
