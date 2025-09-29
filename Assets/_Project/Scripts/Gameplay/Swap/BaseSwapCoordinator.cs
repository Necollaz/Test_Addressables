using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class BaseSwapCoordinator<TAnchor, TAsset>
    where TAsset : class
    where TAnchor : SwappableAnchorBase<TAsset>
{
    public readonly AssetTypeProbe TypeProbe;
    public readonly AddressableKeyNormalizer KeyNormalizer;
    
    private readonly AddressablesAssetLoader assetLoader;
    private readonly Dictionary<string, List<TAnchor>> anchorsBySlot = new();
    private readonly Dictionary<string, string> pendingBySlot = new();

    protected BaseSwapCoordinator(AddressablesAssetLoader assetLoader, AssetTypeProbe probe, AddressableKeyNormalizer keyNormalizer)
    {
        this.assetLoader = assetLoader;
        TypeProbe = probe;
        KeyNormalizer = keyNormalizer;
    }

    public void Register(TAnchor anchor)
    {
        if (anchor == null || string.IsNullOrWhiteSpace(anchor.SlotId))
            return;

        if (!anchorsBySlot.TryGetValue(anchor.SlotId, out var list))
        {
            list = new List<TAnchor>(4);
            anchorsBySlot[anchor.SlotId] = list;
        }
        
        if (!list.Contains(anchor))
            list.Add(anchor);

        if (pendingBySlot.TryGetValue(anchor.SlotId, out var key))
        {
            _ = ApplyByAssetKey(key);
            pendingBySlot.Remove(anchor.SlotId);
        }
    }

    public void Unregister(TAnchor anchor)
    {
        if (anchor == null || string.IsNullOrWhiteSpace(anchor.SlotId))
            return;
        
        if (anchorsBySlot.TryGetValue(anchor.SlotId, out var list))
            list.Remove(anchor);
    }

    public void ResetAll()
    {
        foreach (var list in anchorsBySlot.Values)
            for (int i = 0; i < list.Count; i++)
                list[i].ResetToBase();

        pendingBySlot.Clear();
    }
    
    public async Task ApplyByAssetKey(string addressableKey)
    {
        if (string.IsNullOrWhiteSpace(addressableKey))
            return;
        
        if (!await CanLoadAsset(addressableKey))
            return;

        string slotId = DeriveSlotIdFromKey(addressableKey);
        TAsset asset = await assetLoader.LoadAssetAsync<TAsset>(addressableKey);

        if (anchorsBySlot.TryGetValue(slotId, out var list) && list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
                list[i].ApplyOverride(asset);
        }
        else
        {
            pendingBySlot[slotId] = addressableKey;
        }
    }
    
    protected abstract Task<bool> CanLoadAsset(string addressableKey);
    
    private string DeriveSlotIdFromKey(string rawKey)
    {
        if (string.IsNullOrWhiteSpace(rawKey))
            return "DefaultSlot";
        
        string key = rawKey.Replace('\\', '/').Trim();
        int index = key.LastIndexOf('/');
        
        return index <= 0 ? key : key.Substring(0, index);
    }
}