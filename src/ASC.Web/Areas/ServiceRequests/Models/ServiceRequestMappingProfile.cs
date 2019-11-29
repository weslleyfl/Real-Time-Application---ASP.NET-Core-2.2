﻿using ASC.Models.Models;
using AutoMapper;

namespace ASC.Web.Areas.ServiceRequests.Models
{
    public class ServiceRequestMappingProfile : Profile
    {
        public ServiceRequestMappingProfile()
        {
            CreateMap<ServiceRequest, NewServiceRequestViewModel>().ReverseMap();
        }
    }
}
