using UnityEngine;
using UnityEngine.UI;

public class PrefabPreviewRenderer : MonoBehaviour
{
    private const string LAYER_NAME_PREVIEW_3D = "Preview3D";
    private const string RENDER_TEXTURE_NAME = "PrefabPreview_RT";
    private const int RENDER_TEXTURE_SIZE = 512;
    private const int RENDER_TEXTURE_DEPTH = 16;
    private const float CAMERA_LOOK_OFFSET_Y = 0.2f;

    [SerializeField] private RawImage _targetRawImage;
    [SerializeField] private Camera _previewCamera;
    [SerializeField] private Transform _previewRootTransform;
    [SerializeField] private Light _previewLight;
    
    [Header("Spin (preview only)")]
    [SerializeField] private Vector3 _spinAxisWorld = Vector3.up;
    [SerializeField] private float _spinDegreesPerSecond = 40f;

    [Header("Per-Group Camera Overrides")]
    [SerializeField] private CameraGroupOverride[] _cameraGroupOverrides;
    
    private readonly Vector3 lightWorldOffset = new Vector3(1.5f, 2.0f, 1.5f);
    
    private PrefabPreviewSpinner _spinner;
    private RenderTexture _renderTexture;
    private GameObject _currentPreviewInstance;
    
    private RenderTextureAllocator _renderTextureAllocator;
    private GameObjectHierarchyLayerSetter _hierarchyLayerSetter;
    private RendererBoundsCalculator _rendererBoundsCalculator;
    private PreviewLightPlacer _previewLightPlacer;
    private CameraLookAtOperator _cameraLookAtOperator;
    private CameraOverrideSelector _cameraOverrideSelector;
    private CameraTransformApplier _cameraTransformApplier;
    
    private int _previewLayer = -1;
    private bool _overrideAppliedCurrentKey;

    private void Awake()
    {
        _spinner = new PrefabPreviewSpinner(_spinAxisWorld, _spinDegreesPerSecond);
        _renderTextureAllocator = new RenderTextureAllocator(RenderTextureFormat.ARGB32, RENDER_TEXTURE_NAME, RENDER_TEXTURE_SIZE, RENDER_TEXTURE_DEPTH);
        _hierarchyLayerSetter = new GameObjectHierarchyLayerSetter();
        _rendererBoundsCalculator = new RendererBoundsCalculator();
        _previewLightPlacer = new PreviewLightPlacer(lightWorldOffset);
        _cameraLookAtOperator = new CameraLookAtOperator(CAMERA_LOOK_OFFSET_Y);
        _cameraOverrideSelector = new CameraOverrideSelector(_cameraGroupOverrides);
        _cameraTransformApplier = new CameraTransformApplier();

        _previewLayer = LayerMask.NameToLayer(LAYER_NAME_PREVIEW_3D);
        
        if (_previewLayer < 0)
        {
            Debug.LogWarning("[Preview3D] Layer 'Preview3D' not found in build. Falling back to Default layer.");
            
            _previewLayer = 0;

            if (_previewCamera != null)
                _previewCamera.cullingMask = ~0;
        }
        
        EnsureRenderTarget();

        if (_previewCamera != null)
            _previewCamera.enabled = false;

        _overrideAppliedCurrentKey = false;
    }

    private void Update()
    {
        if (_spinner != null)
            _spinner.Tick(Time.unscaledDeltaTime);
    }

    public void TryShowPrefab(GameObject sourcePrefab)
    {
        EnsureRenderTarget();

        if (_currentPreviewInstance != null)
        {
            Destroy(_currentPreviewInstance);
            
            _currentPreviewInstance = null;
        }

        _currentPreviewInstance = Instantiate(sourcePrefab, _previewRootTransform);
        _hierarchyLayerSetter.SetLayerRecursively(_currentPreviewInstance, _previewLayer >= 0 ? _previewLayer : 0);

        Bounds bounds = _rendererBoundsCalculator.Calculate(_currentPreviewInstance);

        if (!_overrideAppliedCurrentKey)
            _cameraLookAtOperator.LookAtBoundsCenter(_previewCamera, bounds);

        _previewLightPlacer.MoveLightToBoundsCenterWithOffset(_previewLight, bounds);
        _spinner?.SetTarget(_currentPreviewInstance.transform);

        if (_previewCamera != null)
            _previewCamera.enabled = true;
        
        if (_targetRawImage != null && _previewCamera != null)
        {
            if (_previewCamera.targetTexture == null)
                _previewCamera.targetTexture = _renderTexture;
            
            if (_targetRawImage.texture != _renderTexture)
                _targetRawImage.texture = _renderTexture;
        }
    }

    public void Clear()
    {
        _spinner?.ClearTarget();

        if (_currentPreviewInstance != null)
        {
            Destroy(_currentPreviewInstance);
            
            _currentPreviewInstance = null;
        }

        if (_targetRawImage != null)
            _targetRawImage.texture = null;

        if (_previewCamera != null)
            _previewCamera.enabled = false;

        if (_renderTexture != null)
        {
            if (_previewCamera != null && _previewCamera.targetTexture == _renderTexture)
                _previewCamera.targetTexture = null;

            _renderTexture.Release();
            
            Destroy(_renderTexture);
            
            _renderTexture = null;
        }
        
        _overrideAppliedCurrentKey = false;
    }

    public void ApplyCameraOverrideForKey(string assetKey)
    {
        if (_previewCamera == null)
            return;

        if (_cameraOverrideSelector.TrySelectPose(assetKey, out CameraPose pose))
        {
            _cameraTransformApplier.Apply(_previewCamera, pose);
            _overrideAppliedCurrentKey = true;
            
            return;
        }

        _overrideAppliedCurrentKey = false;
    }
    
    private void EnsureRenderTarget()
    {
        _renderTexture = _renderTextureAllocator.Ensure(_renderTexture);

        if (_previewCamera != null && _previewCamera.targetTexture != _renderTexture)
            _previewCamera.targetTexture = _renderTexture;

        if (_targetRawImage != null && _targetRawImage.texture != _renderTexture)
            _targetRawImage.texture = _renderTexture;

        if (_renderTexture != null && !_renderTexture.IsCreated())
            _renderTexture.Create();
    }
}