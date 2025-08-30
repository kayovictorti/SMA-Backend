using AutoMapper;
using SMA.Application.DTOs;
using SMA.Domain.Entities;

namespace SMA.Application.Mappings;

public class ApplicationProfile : Profile
{
    public ApplicationProfile()
    {
        CreateMap<DeviceDto, Device>();
        CreateMap<Device, DeviceDto>();
    }
}
