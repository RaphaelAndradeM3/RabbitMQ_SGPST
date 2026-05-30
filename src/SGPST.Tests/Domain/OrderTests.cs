using FluentAssertions;
using SGPST.Domain.Entities;
using Xunit;

namespace SGPST.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void Create_ShouldInitializeOrderCorrecty()
    {
        // Arrange
        var customerId = "Cli-01";
        var description = "Teste";
        var priority = OrderPriority.Alta;

        // Act
        var order = Order.Create(customerId, description, priority);

        // Assert
        order.Id.Should().NotBeEmpty();
        order.CustomerId.Should().Be(customerId);
        order.Description.Should().Be(description);
        order.Priority.Should().Be(priority);
        order.Status.Should().Be(OrderStatus.Pendente);
        order.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void StartProcessing_ShouldUpdateStatusAndProvider()
    {
        // Arrange
        var order = Order.Create("Cli-01", "Teste", OrderPriority.Media);
        var providerId = "Prov-01";

        // Act
        order.StartProcessing(providerId);

        // Assert
        order.Status.Should().Be(OrderStatus.EmProcessamento);
        order.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public void Complete_ShouldUpdateStatusAndProcessedAt()
    {
        // Arrange
        var order = Order.Create("Cli-01", "Teste", OrderPriority.Media);
        order.StartProcessing("Prov-01");

        // Act
        order.Complete();

        // Assert
        order.Status.Should().Be(OrderStatus.Concluido);
        order.ProcessedAt.Should().NotBeNull();
    }
}
