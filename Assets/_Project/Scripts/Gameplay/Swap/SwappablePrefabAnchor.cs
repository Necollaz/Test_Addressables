using UnityEngine;
using Zenject;

public class SwappablePrefabAnchor : SwappableAnchorBase<GameObject>
{
    [SerializeField] private GameObject _baseInstance;
    [SerializeField] private Transform _spawnParent;

    private PrefabSwapCoordinator _coordinator;
    private GameObject _currentOverrideInstance;
    private bool _isRegistered;

    [Inject]
    private void Construct(PrefabSwapCoordinator coordinator)
    {
        _coordinator = coordinator;

        if (isActiveAndEnabled && !_isRegistered)
        {
            _coordinator.Register(this);
            _isRegistered = true;
        }
    }

    private void OnEnable()
    {
        if (_coordinator == null)
        {
            _isRegistered = false;
            
            return;
        }
        
        _coordinator.Register(this); _isRegistered = true;
    }

    private void OnDisable()
    {
        if (_isRegistered && _coordinator != null)
        {
            _coordinator.Unregister(this);
            _isRegistered = false;
        }
    }

    public override void ApplyOverride(GameObject prefab)
    {
        if (prefab == null)
            return;

        if (_currentOverrideInstance != null)
        {
            Destroy(_currentOverrideInstance);
            _currentOverrideInstance = null;
        }

        if (_baseInstance != null)
            _baseInstance.SetActive(false);

        Transform parent = _spawnParent != null ? _spawnParent : transform;
        _currentOverrideInstance = Instantiate(prefab, parent, false);

        var t = _currentOverrideInstance.transform;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        _currentOverrideInstance.name = $"{SlotId}_Override";
    }

    public override void ResetToBase()
    {
        if (_currentOverrideInstance != null)
        {
            Destroy(_currentOverrideInstance);
            _currentOverrideInstance = null;
        }

        if (_baseInstance != null)
            _baseInstance.SetActive(true);
    }
}