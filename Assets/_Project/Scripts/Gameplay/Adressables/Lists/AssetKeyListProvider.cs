using System.Collections.Generic;
using System.Threading.Tasks;

public class AssetKeyListProvider
{
    private readonly AddressableKeyCatalog catalog;

    public AssetKeyListProvider(AddressableKeyCatalog catalog)
    {
        this.catalog = catalog;
    }

    public Task<List<string>> GetAssetKeysAsync() => catalog.GetAllKeysAsync();
}