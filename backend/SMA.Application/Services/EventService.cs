using Microsoft.Extensions.Logging;
using SMA.Application.DTOs;
using SMA.Application.Interfaces;
using SMA.Domain.Entities;

namespace SMA.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<EventService> _logger;

        public EventService(IDeviceRepository deviceRepository, IEventRepository eventRepository, ILogger<EventService> logger)
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
    }
}
