using System.Collections.Generic;
using TMPro;

public class DropdownOptionsPopulator
{
    public void Populate(TMP_Dropdown dropdown, List<string> keys, List<TMP_Dropdown.OptionData> cache)
    {
        if (dropdown == null)
            return;
        
        cache.Clear();

        for (int i = 0; i < keys.Count; i++)
            cache.Add(new TMP_Dropdown.OptionData(keys[i]));

        dropdown.options = cache;
        dropdown.RefreshShownValue();
    }
}