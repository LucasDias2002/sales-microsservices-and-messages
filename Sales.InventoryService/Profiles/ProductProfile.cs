using AutoMapper;

namespace Sales.InventoryService.Profiles
{
    public class ProductProfile: Profile
    {
        public ProductProfile() { 
            CreateMap<Entities.Product, DTOs.ProductDTO>().ReverseMap();
        }
        
    }
}
