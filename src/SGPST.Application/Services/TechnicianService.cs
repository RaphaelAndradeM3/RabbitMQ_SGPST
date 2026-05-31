using SGPST.Application.DTOs.Technician;
using SGPST.Application.Interfaces;
using SGPST.Domain.Common;
using SGPST.Domain.Entities;
using SGPST.Domain.Interfaces;

namespace SGPST.Application.Services;

// Servico para cadastro e controle de técnicos de suporte
public class TechnicianService : ITechnicianService
{
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUserRepository _userRepository;

    public TechnicianService(ITechnicianRepository technicianRepository, IUserRepository userRepository)
    {
        _technicianRepository = technicianRepository;
        _userRepository = userRepository;
    }

    public async Task<IAppResult<TechnicianDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var technician = await _technicianRepository.GetByIdAsync(id);
            if (technician == null)
            {
                return AppResult<TechnicianDto>.Failure($"Tecnico com ID {id} nao encontrado.");
            }

            var dto = MapToDto(technician);
            return AppResult<TechnicianDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return AppResult<TechnicianDto>.Failure("Erro ao buscar tecnico por ID.", ex);
        }
    }

    public async Task<IAppResult<TechnicianDto>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var technician = await _technicianRepository.GetByUserIdAsync(userId);
            if (technician == null)
            {
                return AppResult<TechnicianDto>.Failure($"Tecnico vinculado ao usuario {userId} nao encontrado.");
            }

            var dto = MapToDto(technician);
            return AppResult<TechnicianDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return AppResult<TechnicianDto>.Failure("Erro ao buscar tecnico por UserId.", ex);
        }
    }

    public async Task<IAppResult<TechnicianDto>> CreateAsync(CreateTechnicianDto createDto)
    {
        try
        {
            // Valida se o usuario de vinculo existe
            var user = await _userRepository.GetByIdAsync(createDto.UserId);
            if (user == null)
            {
                return AppResult<TechnicianDto>.Failure("O usuario para vinculo do tecnico nao existe.");
            }

            // Valida se o usuario ja esta vinculado a outro tecnico
            var existing = await _technicianRepository.GetByUserIdAsync(createDto.UserId);
            if (existing != null)
            {
                return AppResult<TechnicianDto>.Failure("Este usuario ja esta vinculado a um cadastro de tecnico.");
            }

            var technician = Technician.Create(createDto.UserId, createDto.Specialty);
            await _technicianRepository.AddAsync(technician);

            // Recarrega os dados completos do tecnico (incluindo o User relacionado)
            var loaded = await _technicianRepository.GetByIdAsync(technician.Id);
            
            var dto = MapToDto(loaded ?? technician);
            return AppResult<TechnicianDto>.Ok(dto, "Tecnico cadastrado com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult<TechnicianDto>.Failure("Erro ao cadastrar novo tecnico.", ex);
        }
    }

    public async Task<IAppResult<IEnumerable<TechnicianDto>>> GetAllAsync()
    {
        try
        {
            var technicians = await _technicianRepository.GetAllAsync();
            var dtos = technicians.Select(MapToDto);
            return AppResult<IEnumerable<TechnicianDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return AppResult<IEnumerable<TechnicianDto>>.Failure("Erro ao listar todos os tecnicos.", ex);
        }
    }

    public async Task<IAppResult<IEnumerable<TechnicianDto>>> GetAvailableAsync()
    {
        try
        {
            var technicians = await _technicianRepository.GetAvailableAsync();
            var dtos = technicians.Select(MapToDto);
            return AppResult<IEnumerable<TechnicianDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return AppResult<IEnumerable<TechnicianDto>>.Failure("Erro ao listar tecnicos disponiveis.", ex);
        }
    }

    public async Task<IAppResult> UpdateLocationAsync(Guid id, string location)
    {
        try
        {
            var technician = await _technicianRepository.GetByIdAsync(id);
            if (technician == null)
            {
                return AppResult.Failure($"Tecnico com ID {id} nao encontrado.");
            }

            technician.UpdateLocation(location);
            await _technicianRepository.UpdateAsync(technician);

            return AppResult.Ok("Localizacao do tecnico atualizada com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao atualizar localizacao do tecnico.", ex);
        }
    }

    public async Task<IAppResult> SetAvailabilityAsync(Guid id, bool isAvailable)
    {
        try
        {
            var technician = await _technicianRepository.GetByIdAsync(id);
            if (technician == null)
            {
                return AppResult.Failure($"Tecnico com ID {id} nao encontrado.");
            }

            technician.SetAvailability(isAvailable);
            await _technicianRepository.UpdateAsync(technician);

            var status = isAvailable ? "disponivel" : "indisponivel";
            return AppResult.Ok($"Disponibilidade do tecnico alterada para {status}.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao alterar disponibilidade do tecnico.", ex);
        }
    }

    public async Task<IAppResult> SetActiveAsync(Guid id, bool active)
    {
        try
        {
            var technician = await _technicianRepository.GetByIdAsync(id);
            if (technician == null)
            {
                return AppResult.Failure($"Tecnico com ID {id} nao encontrado.");
            }

            technician.SetActive(active);
            await _technicianRepository.UpdateAsync(technician);

            var status = active ? "ativado" : "desativado";
            return AppResult.Ok($"Cadastro do tecnico {status} com sucesso.");
        }
        catch (Exception ex)
        {
            return AppResult.Failure("Erro ao alterar status de ativacao do tecnico.", ex);
        }
    }

    private static TechnicianDto MapToDto(Technician tech)
    {
        return new TechnicianDto(
            tech.Id,
            tech.UserId,
            tech.User?.Username ?? string.Empty,
            tech.User?.Email ?? string.Empty,
            tech.Specialty,
            tech.IsAvailable,
            tech.CurrentLocation,
            tech.IsActive
        );
    }
}
