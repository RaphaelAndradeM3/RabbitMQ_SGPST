namespace SGPST.Application.DTOs.ServicePrice;

// DTO para cadastrar um novo servico com seu respectivo preco
public record CreateServicePriceDto(string Name, string Description, decimal Price);
