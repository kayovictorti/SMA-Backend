using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SMA.Api.Requests;
using SMA.Api.Responses;
using SMA.Application.DTOs;
using SMA.Application.Interfaces;
using SMA.Domain.Entities;

namespace SMA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _service;
        private readonly IMapper _mapper;

        public EventsController(IEventService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }


        [HttpPost]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EventResponse>> Ingest([FromBody] EventIngestRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var dto = _mapper.Map<EventDto>(request);

                var ev = await _service.IngestAsync(dto, ct);
                var resp = _mapper.Map<EventResponse>(ev);

                return Created($"/api/events/{resp.Id}", resp);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<EventResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EventResponse>>> GetAll(
            [FromQuery] long? deviceId,
            [FromQuery] int take = 50,
            CancellationToken ct = default)
        {
            if (take <= 0) take = 50;
            if (take > 500) take = 500;

            List<Event> events;
            if (deviceId.HasValue)
                events = await _service.GetByDeviceAsync(deviceId.Value, take, ct);
            else
                events = await _service.GetLatestAsync(take, ct);

            var response = _mapper.Map<List<EventResponse>>(events);
            return Ok(response);
        }
    }
}
