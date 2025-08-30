using AutoMapper;
using SMA.Application.DTOs;
using SMA.Application.Interfaces;
using SMA.Domain.Entities;

namespace SMA.Application.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _repository;
    private readonly IMapper _mapper;
    private readonly IIotIntegrationClient _iotIntegrationClient;

    public DeviceService(IDeviceRepository repository, IMapper mapper, IIotIntegrationClient iotIntegrationClient)
    {
        _repository = repository;
        _mapper = mapper;
        _iotIntegrationClient = iotIntegrationClient;
    }

    public async Task<Device> CreateAsync(DeviceDto dto, CancellationToken ct)
    {

        try
        {
            var device = _mapper.Map<Device>(dto);

            await _repository.AddAsync(device, ct);

            var integrationId = await _iotIntegrationClient.RegisterAsync(device.Name, device.Location, ct);
            device.IntegrationId = integrationId;

            await _repository.UpdateAsync(device, ct);

            return device;
        }
        catch
        {
            throw;
        }
    }

    public async Task<List<Device>> GetAllAsync(CancellationToken ct)
    {
        return await _repository.GetAllAsync(ct);
    }


    public async Task<Device?> GetByIdAsync(long id, CancellationToken ct) =>
        await _repository.GetByIdAsync(id, ct);
}
