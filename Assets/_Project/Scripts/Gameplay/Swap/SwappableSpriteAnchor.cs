using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SwappableSpriteAnchor : SwappableAnchorBase<Sprite>
{
    [Header("Target (любой):")]
    [SerializeField] private Image _uiImage;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    [Header("Base (опционально)")]
    [SerializeField] private Sprite _baseSprite;

    private SpriteSwapCoordinator _coordinator;
    private bool _isRegistered;

    [Inject]
    private void Construct(SpriteSwapCoordinator coordinator)
    {
        _coordinator = coordinator;
        
        if (_baseSprite == null)
            _baseSprite = GetCurrentSprite();

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
        
        if (_baseSprite == null)
            _baseSprite = GetCurrentSprite();
        
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

    public override void ApplyOverride(Sprite sprite)
    {
        if (sprite == null)
            return;
        
        if (_uiImage != null)
            _uiImage.sprite = sprite;
        
        if (_spriteRenderer != null)
            _spriteRenderer.sprite = sprite;
    }

    public override void ResetToBase()
    {
        if (_uiImage != null)
            _uiImage.sprite = _baseSprite;
        
        if (_spriteRenderer != null)
            _spriteRenderer.sprite = _baseSprite;
    }

    private Sprite GetCurrentSprite()
    {
        if (_uiImage != null)
            return _uiImage.sprite;
        
        if (_spriteRenderer != null)
            return _spriteRenderer.sprite;
        
        return null;
    }
}