using FluentAssertions;
using Moq;
using SGPST.Application.DTOs;
using SGPST.Application.Services;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;
using Xunit;

namespace SGPST.Tests.Application;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IMessageBroker> _brokerMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _brokerMock = new Mock<IMessageBroker>();
        _orderService = new OrderService(_orderRepoMock.Object, _brokerMock.Object);
    }

    [Fact]
    public async Task SubmitOrderAsync_WhenValid_ShouldSaveAndPublish()
    {
        // Arrange
        var dto = new CreateOrderDto("Customer1", "Help me", OrderPriority.Alta);

        // Act
        var result = await _orderService.SubmitOrderAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Description.Should().Be(dto.Description);

        _orderRepoMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
        _brokerMock.Verify(x => x.PublishOrderAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnList()
    {
        // Arrange
        var orders = new List<Order> { 
            Order.Create("C1", "D1", OrderPriority.Baixa),
            Order.Create("C2", "D2", OrderPriority.Media)
        };
        _orderRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetAllOrdersAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }
}
