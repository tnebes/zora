#region

using AutoMapper;
using zora.Core.Domain;
using zora.Core.DTOs.Assets;

#endregion

namespace zora.API.Mapping;

public sealed class AssetMappingProfile : Profile
{
    public AssetMappingProfile()
    {
        this.CreateMap<Asset, AssetDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.AssetPath, opt => opt.MapFrom(src => src.AssetPath))
            .AfterMap((src, dest) =>
            {
                dest.CreatedBy = src.CreatedBy != null
                    ? (int)src.CreatedBy.Id
                    : src.CreatedById.HasValue
                        ? (int)src.CreatedById.Value
                        : 0;

                dest.UpdatedBy = src.UpdatedBy != null
                    ? (int)src.UpdatedBy.Id
                    : src.UpdatedById.HasValue
                        ? (int)src.UpdatedById.Value
                        : null;
            });
        this.CreateMap<Asset, AssetWithWorkItemsDto>()
            .IncludeBase<Asset, AssetDto>()
            .ForMember(dest => dest.WorkItems, opt => opt.MapFrom(src => src.WorkItems));
        this.CreateMap<CreateAssetDto, Asset>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.AssetPath, opt => opt.MapFrom(src => src.AssetPath))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedById, opt => opt.MapFrom(src => src.CreatedById))
            .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.WorkItemAssets,
                opt => opt.MapFrom(src => src.WorkAssetId != null ? src.WorkAssetId : null))
            .ForMember(dest => dest.WorkItems, opt => opt.Ignore());
        this.CreateMap<UpdateAssetDto, Asset>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.AssetPath, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedById, opt => opt.MapFrom(src => src.UpdatedById))
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.WorkItemAssets, opt => opt.Ignore())
            .ForMember(dest => dest.WorkItems, opt => opt.Ignore());
    }
}
