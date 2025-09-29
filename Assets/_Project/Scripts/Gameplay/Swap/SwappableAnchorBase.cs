using UnityEngine;

public abstract class SwappableAnchorBase<TAsset> : MonoBehaviour
{
    [SerializeField] private string _slotId = "DefaultSlot";
    
    public string SlotId => _slotId;

    public abstract void ApplyOverride(TAsset asset);
    
    public abstract void ResetToBase();
}