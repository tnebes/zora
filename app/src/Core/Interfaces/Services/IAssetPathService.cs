namespace zora.Core.Interfaces.Services;

public interface IAssetPathService
{
    string GetAssetsBasePath();
    string GetAssetWebPath(string fileName);
}
