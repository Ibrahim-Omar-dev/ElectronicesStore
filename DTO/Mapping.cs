using AutoMapper;
using ElectronicsStore.Models;
using ElectronicsStore.Models.DTO;

namespace ElectronicsStore.DTO
{
    public class Mapping: Profile
    {
        public Mapping()
        {
            CreateMap<Product, ProductDTO>()
            .ForMember(dest => dest.ImageFile, opt => opt.Ignore()); // Ignore IFormFile when mapping from Product

            CreateMap<ProductDTO, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Don't map Id
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore()) // Handle separately
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()); // Don't overwrite CreatedAt
        }
    }
}
