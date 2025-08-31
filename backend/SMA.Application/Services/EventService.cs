using Microsoft.Extensions.Logging;
using SMA.Application.DTOs;
using SMA.Application.Interfaces;
using SMA.Domain.Entities;

namespace SMA.Application.Services;

public class EventService : IEventService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<EventService> _logger;

    public EventService(
        IDeviceRepository deviceRepository,
        IEventRepository eventRepository,
        ILogger<EventService> logger)
    {
        _deviceRepository = deviceRepository;
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public async Task<Event> IngestAsync(EventDto dto, CancellationToken ct)
    {
        try
        {
            var device = await _deviceRepository.GetByIntegrationIdAsync(dto.IntegrationId, ct);
            if (device is null)
                throw new KeyNotFoundException("Dispositivo não encontrado para o integrationId fornecido.");

            var ev = new Event
            {
                DeviceId = device.Id,
                Temperature = (decimal)dto.Temperature,
                Humidity = (decimal)dto.Humidity,
                IsAlarm = dto.IsAlarm,
                OccurredAt = dto.OccurredAt
            };

            await _eventRepository.AddAsync(ev, ct);
            return ev;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ingerir evento para integrationId {IntegrationId}", dto.IntegrationId);
            throw;
        }
    }

    public async Task<List<Event>> GetLatestAsync(int take, CancellationToken ct)
    {
        try
        {
            var events = await _eventRepository.GetLatestAsync(take, ct);

            foreach (var ev in events)
            {
                var dev = await _deviceRepository.GetByIdAsync(ev.DeviceId, ct);
                if (dev is not null)
                {
                    ev.Device = dev;
                }
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter eventos (take={Take})", take);
            throw;
        }
    }

    public async Task<List<Event>> GetByDeviceAsync(long deviceId, int take, CancellationToken ct)
    {
        try
        {
            var events = await _eventRepository.GetByDeviceAsync(deviceId, take, ct);

            foreach (var ev in events)
            {
                var dev = await _deviceRepository.GetByIdAsync(ev.DeviceId, ct);
                if (dev is not null)
                {
                    ev.Device = dev;
                }
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter eventos para o DeviceId {DeviceId} (take={Take})", deviceId, take);
            throw;
        }
    }

}