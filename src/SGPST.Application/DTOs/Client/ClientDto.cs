namespace SGPST.Application.DTOs.Client;

// DTO para representar as informacoes completas de um cliente
public record ClientDto(
    Guid Id,
    string Name,
    string Document,
    string Email,
    string Phone,
    string AddressLine,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    bool IsActive
);
