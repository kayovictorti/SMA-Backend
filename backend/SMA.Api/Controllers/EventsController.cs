using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SMA.Api.Requests;
using SMA.Api.Responses;
using SMA.Application.DTOs;
using SMA.Application.Interfaces;

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
    }
    }
