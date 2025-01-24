using AutoMapper;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Requests;

public sealed class AssetMappingProfile : Profile
{
    public AssetMappingProfile()
    {
        CreateMap<Asset, AssetDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.CreatedByUsername, opt => opt.MapFrom(src => src.CreatedBy.Username))
            .ForMember(dest => dest.UpdatedByUsername, opt => opt.MapFrom(src => src.UpdatedBy.Username))
            .ForMember(dest => dest.AssetPath, opt => opt.MapFrom(src => src.AssetPath));
        CreateMap<Asset, AssetWithWorkItemsDto>()
            .IncludeBase<Asset, AssetDto>()
            .ForMember(dest => dest.WorkItems, opt => opt.MapFrom(src => src.WorkItems));
        CreateMap<CreateAssetDto, Asset>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.AssetPath, opt => opt.MapFrom(src => src.AssetPath))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.WorkItemAssets, opt => opt.Ignore())
            .ForMember(dest => dest.WorkItems, opt => opt.Ignore());
    }
}
