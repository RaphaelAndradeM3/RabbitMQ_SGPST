namespace SGPST.Application.DTOs.ServicePrice;

// DTO para representar um servico cadastrado no catalogo de precos
public record ServicePriceDto(Guid Id, string Name, string Description, decimal Price, bool IsActive);
