#region

using System.Runtime.CompilerServices;
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
            File = new MockFormFile(
                new MemoryStream(),
                0,
                1024,
                "updated.jpg",
                "updated.jpg"
            )
        };
    }

    internal static CreateAssetDto GetValidCreateAssetDto()
    {
        return new CreateAssetDto
        {
            Name = "Test Asset",
            Description = "Test asset description",
            Asset = new MockFormFile(
                new MemoryStream(),
                0,
                1024,
                "test.jpg",
                "test.jpg"
            )
        };
    }

    internal static CreateAssetDto GetValidCreateAssetWithDataDto()
    {
        CreateAssetDto assetDto = GetValidCreateAssetDto();
        assetDto.Asset = new MockFormFile(
            new MemoryStream(),
            0,
            1024,
            "test.jpg",
            "test.jpg"
            );
        return assetDto;
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

    public sealed class MockFormFile : IFormFile
    {
        private readonly Stream _baseStream;
        private readonly long _baseStreamOffset;

        public MockFormFile(Stream baseStream, long baseStreamOffset, long length, string name, string fileName)
        {
            this._baseStream = baseStream;
            this._baseStreamOffset = baseStreamOffset;
            this.Length = length;
            this.Name = name;
            this.FileName = fileName;
            this.Headers = new HeaderDictionary();
            this.ContentType = "image/jpeg";
        }

        public Stream OpenReadStream()
        {
            this._baseStream.Position = this._baseStreamOffset;
            return this._baseStream;
        }

        public void CopyTo(Stream target)
        {
            this._baseStream.Position = this._baseStreamOffset;
            this._baseStream.CopyTo(target);
        }

        public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            this._baseStream.Position = this._baseStreamOffset;
            await this._baseStream.CopyToAsync(target, cancellationToken);
        }

        public string ContentType { get; set; }
        public string ContentDisposition => $"form-data; name=\"{this.Name}\"; filename=\"{this.FileName}\"";
        public IHeaderDictionary Headers { get; }
        public long Length { get; }
        public string Name { get; }
        public string FileName { get; }
    }
}
