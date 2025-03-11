#region

using System.Text;
using Microsoft.AspNetCore.Http;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Tests.Utils;

public static class AssetUtils
{
    public static IEnumerable<Asset> GetValidAssets()
    {
        List<Asset> assets =
        [
            new()
            {
                Name = "First Asset",
                Description = "First test asset description",
                AssetPath = "/assets/first.jpg",
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Name = "Second Asset",
                Description = "Second test asset description",
                AssetPath = "/assets/second.pdf",
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Name = "Third Asset",
                Description = "Third test asset description",
                AssetPath = "/assets/third.docx",
                CreatedAt = DateTime.UtcNow
            }
        ];

        return assets;
    }

    internal static UpdateAssetDto GetValidUpdateAssetDto()
    {
        return new UpdateAssetDto
        {
            Name = "Updated Asset",
            Description = "Updated asset description",
            WorkItemId = 1,
            File = GetValidAsset("Updated Asset.txt")
        };
    }

    internal static CreateAssetDto GetValidCreateAssetDto()
    {
        return new CreateAssetDto
        {
            Name = "Test Asset",
            Description = "Test asset description",
            Asset = GetValidAsset()
        };
    }

    internal static CreateAssetDto GetValidCreateAssetWithDataDto(long workItemId = 0, string assetName = "Test Asset",
        string extension = "txt")
    {
        string assetNameWithExtension = $"{assetName}.{extension}";
        CreateAssetDto assetDto = GetValidCreateAssetDto();
        assetDto.Asset = GetValidAsset(assetNameWithExtension);
        assetDto.AssetPath = assetNameWithExtension;
        assetDto.Name = assetName;

        if (workItemId > 0)
        {
            assetDto.WorkAssetId = workItemId;
        }

        return assetDto;
    }

    private static IFormFile GetValidAsset(string assetNameWithExtension = "Test Asset.txt")
    {
        string fileContent = "This is a test file content.";
        MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        IFormFile file = new FormFile(stream,
            0,
            stream.Length,
            assetNameWithExtension,
            assetNameWithExtension
        )
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        return file;
    }

    private static IFormFile GetInvalidAsset(string assetNameWithExtension = "Test Asset.txt")
    {
        string fileContent = "";
        MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        IFormFile file = new FormFile(stream,
            0,
            0,
            assetNameWithExtension,
            assetNameWithExtension
        )
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        return file;
    }

    internal static DynamicQueryAssetParamsDto GetValidDynamicQueryAssetParamsDto()
    {
        return new DynamicQueryAssetParamsDto
        {
            Page = 1,
            PageSize = 10,
            Name = "Asset",
            Description = "description"
        };
    }

    public static CreateAssetDto GetInvalidCreateAssetDto()
    {
        return new CreateAssetDto
        {
            Name = "Test Asset",
            Description = "Test asset description",
            Asset = GetInvalidAsset()
        };
    }
}
