using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

public class SceneKeyListProvider
{
    public async Task<List<string>> GetSceneKeysAsync()
    {
        await Addressables.InitializeAsync().Task;

        var result = new List<string>(256);
        var unique = new HashSet<string>(StringComparer.Ordinal);

        foreach (var locator in Addressables.ResourceLocators)
        {
            if (locator?.Keys == null)
                continue;

            foreach (var key in locator.Keys)
            {
                if (!locator.Locate(key, typeof(SceneInstance), out var locations) || locations == null)
                    continue;

                for (int i = 0; i < locations.Count; i++)
                {
                    string k = locations[i].PrimaryKey;
                    
                    if (!string.IsNullOrEmpty(k) && unique.Add(k))
                        result.Add(k);
                }
            }
        }

        result.Sort(StringComparer.OrdinalIgnoreCase);
        
        return result;
    }
}