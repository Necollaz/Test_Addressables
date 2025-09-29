using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class AddressableKeyCatalog
{
    private const int BYTES_IN_KILOBYTE = 1024;
    
    private readonly AllowedKeyFilter filter;

    public AddressableKeyCatalog(AllowedKeyFilter filter)
    {
        this.filter = filter;
    }

    public async Task<List<string>> GetAllKeysAsync()
    {
        await Addressables.InitializeAsync().Task;

        var result = new List<string>(BYTES_IN_KILOBYTE);
        var unique = new HashSet<string>(StringComparer.Ordinal);

        foreach (var locator in Addressables.ResourceLocators)
        {
            if (locator == null || locator.Keys == null)
                continue;

            foreach (var keyObject in locator.Keys)
            {
                if (!locator.Locate(keyObject, null, out var locs) || locs == null)
                    continue;

                for (int i = 0; i < locs.Count; i++)
                {
                    string primary = locs[i].PrimaryKey;
                    
                    if (string.IsNullOrEmpty(primary))
                        continue;

                    if (!filter.IsAllowed(primary))
                        continue;
                    
                    if (filter.IsHexLike(primary, 32))
                        continue;

                    if (unique.Add(primary))
                        result.Add(primary);
                }
            }
        }

        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }
}