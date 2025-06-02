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


            CreateMap<CreateProductDTO, Product>()
    .ForMember(dest => dest.ImageURl, opt => opt.Ignore()); // ImageFile'dan ImageURl'ye manuel atama yapacağız

            CreateMap<Product, UpdateProductDTO>()
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentImageUrl, opt => opt.MapFrom(src => src.ImageURl));
        }
    }
}
