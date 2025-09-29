using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AssetTypeProbe
{
    private const int MAX_FUZZY = 20;

    private readonly AllowedKeyFilter filter;

    public AssetTypeProbe(AllowedKeyFilter filter)
    {
        this.filter = filter;
    }

    public async Task<(bool hasSprite, bool hasPrefab)> ProbeExactAsync(object key)
    {
        await Addressables.InitializeAsync().Task;

        var prefabTypedHandle = Addressables.LoadResourceLocationsAsync(key, typeof(GameObject));
        var prefabLocation = await prefabTypedHandle.Task;
        Addressables.Release(prefabTypedHandle);

        var anyTypedHandle = Addressables.LoadResourceLocationsAsync(key);
        var anyLocation = await anyTypedHandle.Task;
        Addressables.Release(anyTypedHandle);

        var spriteHandle = Addressables.LoadResourceLocationsAsync(key, typeof(Sprite));
        var spriteLocation = await spriteHandle.Task;
        Addressables.Release(spriteHandle);

        return (hasSprite: spriteLocation != null && spriteLocation.Count > 0, hasPrefab: HasGameObject(prefabLocation) || HasGameObject(anyLocation));
    }

    public async Task<List<string>> FindFuzzyPrefabKeysAsync(string fragment)
    {
        fragment = fragment?.Trim();
        
        if (string.IsNullOrEmpty(fragment))
            return new List<string>();

        await Addressables.InitializeAsync().Task;

        var resources = new List<string>();
        string fragLower = fragment.ToLowerInvariant();

        foreach (var locator in Addressables.ResourceLocators)
        {
            if (locator?.Keys == null)
                continue;

            foreach (var keyObject in locator.Keys)
            {
                string key = keyObject?.ToString();
                
                if (string.IsNullOrEmpty(key))
                    continue;
                
                if (!filter.IsAllowed(key))
                    continue;

                if (key.ToLowerInvariant().Contains(fragLower))
                {
                    if (locator.Locate(keyObject, typeof(GameObject), out var goLocs) && HasGameObject(goLocs))
                        resources.Add(key);
                    else if (locator.Locate(keyObject, null, out var anyLocs) && HasGameObject(anyLocs))
                        resources.Add(key);

                    if (resources.Count >= MAX_FUZZY)
                        return DedupSort(resources, fragment);
                }
            }
        }

        return DedupSort(resources, fragment);
    }

    private static bool HasGameObject(IList<IResourceLocation> locations)
    {
        if (locations == null || locations.Count == 0)
            return false;
        
        for (int i = 0; i < locations.Count; i++)
        {
            var type = locations[i].ResourceType;
            
            bool isPrefabType = type == typeof(GameObject) || type == typeof(UnityEngine.Object) || (type != null && typeof(GameObject).IsAssignableFrom(type));
            
            if (isPrefabType || !string.IsNullOrEmpty(locations[i].InternalId) && locations[i].InternalId.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        
        return false;
    }

    private List<string> DedupSort(List<string> list, string fragment)
    {
        return list.Distinct().OrderBy(key => key.StartsWith(fragment, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(key => Math.Abs(key.Length - fragment.Length)).ThenBy(key => key.Length).ToList();
    }
}