using SGPST.Domain.Entities;

namespace SGPST.Application.DTOs;

public record OrderDto(
    Guid Id,
    string CustomerId,
    string Description,
    OrderPriority Priority,
    OrderStatus Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt,
    string? ProviderId
);

public record CreateOrderDto(
    string CustomerId,
    string Description,
    OrderPriority Priority
);
