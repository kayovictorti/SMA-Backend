using AutoMapper;
using SMA.Api.Requests;
using SMA.Api.Responses;
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
    }
}
