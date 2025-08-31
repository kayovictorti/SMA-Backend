using AutoMapper;
using SMA.Application.DTOs;
using SMA.Application.Interfaces;
using SMA.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace SMA.Application.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _repository;
    private readonly IMapper _mapper;
    private readonly IIotIntegrationClient _iotIntegrationClient;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(IDeviceRepository repository, IMapper mapper, IIotIntegrationClient iotIntegrationClient, ILogger<DeviceService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _iotIntegrationClient = iotIntegrationClient;
        _logger = logger;
    }

    public async Task<Device> CreateAsync(DeviceDto dto, CancellationToken ct)
    {
        try
        {
            var device = _mapper.Map<Device>(dto);

            await _repository.AddAsync(device, ct);

            try
            {
                var integrationId = await _iotIntegrationClient.RegisterAsync(device.Name, device.Location, ct);
                device.IntegrationId = integrationId;
                await _repository.UpdateAsync(device, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao registrar dispositivo no IoT; prosseguindo com MOCK.");
                device.IntegrationId ??= $"mock-{Guid.NewGuid()}";
                await _repository.UpdateAsync(device, ct);
            }

            return device;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar dispositivo");
            throw;
        }
    }

    public async Task<List<Device>> GetAllAsync(CancellationToken ct)
    {
        try
        {
            return await _repository.GetAllAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter todos os dispositivos");
            throw;
        }
    }

    public async Task<Device?> GetByIdAsync(long id, CancellationToken ct)
    {
        try
        {
            return await _repository.GetByIdAsync(id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dispositivo por id {DeviceId}", id);
            throw;
        }
    }

    public async Task<Device?> UpdateAsync(long id, DeviceDto dto, CancellationToken ct)
    {
        try
        {
            var device = await _repository.GetByIdForUpdateAsync(id, ct);
            if (device is null) return null;

            device.Name = dto.Name;
            device.Location = dto.Location;

            await _repository.UpdateAsync(device, ct);
            return device;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar dispositivo com id {DeviceId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        try
        {
            var device = await _repository.GetByIdForUpdateAsync(id, ct);
            if (device is null) return false;

            if (!string.IsNullOrWhiteSpace(device.IntegrationId))
            {
                try
                {
                    await _iotIntegrationClient.UnregisterAsync(device.IntegrationId, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao desregistrar dispositivo do IoT. IntegrationId: {IntegrationId}", device.IntegrationId);
                }
            }

            await _repository.DeleteAsync(device, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao tentar deletar o dispositivo com id {DeviceId}", id);
            throw;
        }
    }
}
