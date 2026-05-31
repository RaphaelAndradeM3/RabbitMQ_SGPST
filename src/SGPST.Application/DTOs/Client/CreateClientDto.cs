namespace SGPST.Application.DTOs.Client;

// DTO para cadastrar um novo cliente
public record CreateClientDto(
    string Name, 
    string Document, 
    string Email, 
    string Phone, 
    string AddressLine, 
    string Neighborhood, 
    string City, 
    string State, 
    string ZipCode
);
