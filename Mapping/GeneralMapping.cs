﻿using AutoMapper;
using KitapProject.DTO.CategoryDTO;
using KitapProject.DTO.ProductDTO;
using KitapProject.DTO.TestimonialDTO;
using KitapProject.Entities;

namespace KitapProject.Mapping
{
    public class GeneralMapping : Profile
    {
        public GeneralMapping()
        {
            CreateMap<Product, ResultProductDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category!.Name))
                .ReverseMap();
            CreateMap<CreateProductDTO, Product>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Product, GetByIdProductDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category!.Name)) 
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId)); 
            CreateMap<Product, UpdateProductDTO>()
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore()) 
                .ForMember(dest => dest.CurrentImageUrl, opt => opt.MapFrom(src => src.ImageUrl)) 
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId));


            CreateMap<Category, ResultCategoryDTO>();
            CreateMap<CreateCategoryDTO, Category>();
            CreateMap<UpdateCategoryDTO, Category>().ReverseMap(); 
            CreateMap<Category, GetByIdCategoryDTO>();


            CreateMap<Testimonial, ResultTestimonialDTO>().ReverseMap();
            CreateMap<CreateTestimonialDTO, Testimonial>().ReverseMap();
            CreateMap<UpdateTestimonialDTO, Testimonial>().ReverseMap();
            CreateMap<Testimonial, GetByIdTestimonialDTO>().ReverseMap();
        }
    }
}