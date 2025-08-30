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
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _service;
        private readonly IMapper _mapper;

        public DevicesController(IDeviceService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }


        [HttpPost]
        [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DeviceResponse>> Create([FromBody] DeviceCreateRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var dto = _mapper.Map<DeviceDto>(request);
            var device = await _service.CreateAsync(dto, ct);

            var response = _mapper.Map<DeviceResponse>(device);

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);

        }

        [HttpGet]
        [ProducesResponseType(typeof(List<DeviceResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DeviceResponse>>> GetAll(CancellationToken ct)
        {
            var devices = await _service.GetAllAsync(ct);

            var response = _mapper.Map<List<DeviceResponse>>(devices);

            return Ok(response);
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeviceResponse>> GetById(long id, CancellationToken ct)
        {
            var device = await _service.GetByIdAsync(id, ct);
            if (device is null) return NotFound();

            var response = _mapper.Map<DeviceResponse>(device);
            return Ok(response);
        }
    }
}
