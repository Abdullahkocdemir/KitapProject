using AutoMapper;
using KitapProject.DTO.ProductDTO;
using KitapProject.Entities;

namespace KitapProject.Mapping
{
    public class GeneralMapping:Profile
    {
        public GeneralMapping()
        {
            CreateMap<Product, ResultProductDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category!.Name)).ReverseMap();
            CreateMap<CreateProductDTO, Product>().ReverseMap();
            CreateMap<UpdateProductDTO, Product>().ReverseMap();
            CreateMap<GetByIdProductDTO,Product >().ReverseMap();
        }
    }
}
