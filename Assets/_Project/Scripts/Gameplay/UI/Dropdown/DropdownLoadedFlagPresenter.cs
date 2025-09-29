using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownLoadedFlagPresenter
{
    public void SyncItemIcons(TMP_Dropdown dropdown, IList<string> keys, IReadOnlyCollection<string> loadedKeys, Sprite markerSprite)
    {
        if (dropdown == null || dropdown.options == null || markerSprite == null)
            return;
        
        if (keys == null || loadedKeys == null)
            return;

        var options = dropdown.options;
        int count = Mathf.Min(keys.Count, options.Count);

        for (int i = 0; i < count; i++)
        {
            string key = keys[i];
            bool isLoaded = ContainsFast(loadedKeys, key);
            options[i].image = isLoaded ? markerSprite : null;
        }

        dropdown.RefreshShownValue();
    }

    private bool ContainsFast(IReadOnlyCollection<string> collection, string key)
    {
        if (collection is HashSet<string> hashSet)
            return hashSet.Contains(key);
        
        if (collection is ICollection<string> objectCollider)
            return objectCollider.Contains(key);

        foreach (var item in collection)
            if (item == key)
                return true;

        return false;
    }
}