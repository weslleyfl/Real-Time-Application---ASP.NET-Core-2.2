using ASC.Models.Models;
using ASC.Web.Models.MasterDataViewModels;
using AutoMapper;

namespace ASC.Web.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<MasterDataKey, MasterDataKeyViewModel>().ReverseMap();
            //CreateMap<MasterDataKeyViewModel, MasterDataKey>();
            CreateMap<MasterDataValue, MasterDataValueViewModel>().ReverseMap();
            //CreateMap<MasterDataValueViewModel, MasterDataValue>();
        }
    }
}
