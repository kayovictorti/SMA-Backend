using AutoMapper;
using SMA.Api.Requests;
using SMA.Api.Requests.DevicesControllerRequests;
using SMA.Api.Responses;
using SMA.Api.Responses.DevicesControllerResponse;
using SMA.Application.DTOs;
using SMA.Domain.Entities;

namespace SMA.Api.Mappings;

public class ApiProfile : Profile
{
    public ApiProfile()
    {
        CreateMap<DeviceCreateRequest, DeviceDto>();

        CreateMap<Device, DeviceResponse>();

        CreateMap<DeviceUpdateRequest, DeviceDto>();

        CreateMap<EventIngestRequest, EventDto>()
            .ForMember(dest => dest.OccurredAt, opt => opt.MapFrom(src => src.Timestamp))
            .ForMember(dest => dest.IntegrationId, opt => opt.MapFrom(src => src.DeviceId));

        CreateMap<Event, EventResponse>()
            .ForMember(d => d.DeviceName, o => o.MapFrom(s => s.Device != null ? s.Device.Name : string.Empty))
            .ForMember(d => d.CreationDate, o => o.MapFrom(s => s.CreationDate));
    }
}
