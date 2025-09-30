using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class DemoLoaderView : MonoBehaviour
{
    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown _assetKeyDropdown;
    [SerializeField] private TMP_Dropdown _sceneKeyDropdown;

    [Header("Buttons")]
    [SerializeField] private Button _loadSpriteButton;
    [SerializeField] private Button _loadPrefabButton;
    [SerializeField] private Button _unloadAllButton;
    [SerializeField] private Button _loadSceneButton;

    [Header("Preview Objects")]
    [SerializeField] private Image _spritePreviewImage;
    [SerializeField] private GameObject _gameObjectPreviewContainer;
    [SerializeField] private Transform _spawnRootTransform;
    [SerializeField] private PrefabPreviewRenderer _prefabPreviewRenderer;

    [Header("Scene Progress")]
    [SerializeField] private Slider _sceneProgressSlider;
    [SerializeField] private bool _alsoInstantiateIntoScene = false;

    [Header("Overlays")]
    [SerializeField] private Sprite _loadedPrefabIconSprite;
    [SerializeField] private Image _loadedPrefabIconImage;
    [SerializeField] private Image _assetItemLoadedPrototype;
    [SerializeField] private Image _sceneItemLoadedPrototype;

    [Inject] private readonly AddressablesAssetLoader assetLoader;
    [Inject] private readonly AddressablesSceneLoader sceneLoader;
    [Inject] private readonly PrefabSwapCoordinator prefabSwapCoordinator;
    [Inject] private readonly SpriteSwapCoordinator spriteSwapCoordinator;
    [Inject] private readonly AddressableKeyNormalizer keyNormalizer;
    [Inject] private readonly AddressablesInitializer initializer;
    [Inject] private readonly WebGLSafeSceneLoader safeSceneLoader;
    
    private readonly List<string> assetKeys = new List<string>(2048);
    private readonly List<string> sceneKeys = new List<string>(256);
    private readonly List<TMP_Dropdown.OptionData> assetOptions = new List<TMP_Dropdown.OptionData>(2048);
    private readonly List<TMP_Dropdown.OptionData> sceneOptions = new List<TMP_Dropdown.OptionData>(256);

    private AllowedKeyFilter _allowedKeyFilter;
    private AddressableKeyCatalog _keyCatalog;
    private AssetKeyListProvider _assetKeyListProvider;
    private SceneKeyListProvider _sceneKeyListProvider;
    private AssetTypeProbe _typeProbe;

    private PreviewPanelSwitcher _previewPanels;
    private PrefabLoadAndPreview _prefabFlow;
    private DropdownOptionsPopulator _optionsPopulator;
    private DropdownCaptionOverlay _assetCaptionOverlay;
    private DropdownCaptionOverlay _sceneCaptionOverlay;
    
    private DropdownItemImageSlotInstaller _itemImageSlotInstaller;
    private DropdownLoadedFlagPresenter _loadedFlagPresenter;
    private AssetButtonsAvailabilityUpdater _buttonsAvailabilityUpdater;
    private AssetLivePreviewer _livePreviewer;
    private SceneSelectionButtonGate _sceneSelectionGate;
    
    private void Awake()
    {
        _allowedKeyFilter = new AllowedKeyFilter(new[]
        {
            GroupNameKeys.KEY_GROUP_CHARACTERS,
            GroupNameKeys.KEY_GROUP_UI,
            GroupNameKeys.KEY_GROUP_BUILDINGS,
            GroupNameKeys.KEY_GROUP_EFFECTS,
            GroupNameKeys.KEY_GROUP_SCENES
        });
        _keyCatalog = new AddressableKeyCatalog(_allowedKeyFilter);
        _assetKeyListProvider = new AssetKeyListProvider(_keyCatalog);
        _sceneKeyListProvider = new SceneKeyListProvider();
        _typeProbe = new AssetTypeProbe(_allowedKeyFilter);
        _previewPanels = new PreviewPanelSwitcher(_spritePreviewImage, _gameObjectPreviewContainer, _prefabPreviewRenderer);
        _prefabFlow = new PrefabLoadAndPreview(assetLoader, _prefabPreviewRenderer, _spawnRootTransform, _alsoInstantiateIntoScene);
        _optionsPopulator = new DropdownOptionsPopulator();
        _itemImageSlotInstaller = new DropdownItemImageSlotInstaller(new DropdownItemImageLayout(new Vector2(1f, 0.5f),
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),8f));
        _loadedFlagPresenter = new DropdownLoadedFlagPresenter();
        _buttonsAvailabilityUpdater = new AssetButtonsAvailabilityUpdater(_typeProbe, _loadSpriteButton, _loadPrefabButton, keyNormalizer);
        _livePreviewer = new AssetLivePreviewer(_typeProbe, _prefabPreviewRenderer, _previewPanels, _spritePreviewImage, keyNormalizer);
        _sceneSelectionGate = new SceneSelectionButtonGate();
        _assetCaptionOverlay = new DropdownCaptionOverlay(_assetItemLoadedPrototype, "ItemLoaded");
        _sceneCaptionOverlay = new DropdownCaptionOverlay(_sceneItemLoadedPrototype, "LoadedScene");

        if (_loadedPrefabIconImage != null)
            _loadedPrefabIconImage.enabled = false;
        
        if (_assetItemLoadedPrototype != null)
            _assetItemLoadedPrototype.enabled = false;
        
        if (_sceneItemLoadedPrototype != null)
            _sceneItemLoadedPrototype.enabled = false;
    }

    private async void OnEnable()
    {
        _loadSpriteButton.onClick.AddListener(OnLoadSpriteClicked);
        _loadPrefabButton.onClick.AddListener(OnLoadPrefabClicked);
        _unloadAllButton.onClick.AddListener(OnUnloadAllClicked);
        _loadSceneButton.onClick.AddListener(OnLoadSceneClicked);

        if (_assetKeyDropdown != null)
            _assetKeyDropdown.onValueChanged.AddListener(OnAssetDropdownChanged);

        if (_sceneKeyDropdown != null)
            _sceneKeyDropdown.onValueChanged.AddListener(OnSceneDropdownChanged);

        await initializer.RefreshCatalogIfNeeded(false);
        await PopulateAssetDropdownAsync();
        await PopulateSceneDropdownAsync();

        _buttonsAvailabilityUpdater.SetButtonsInteractable(false, false);
        _previewPanels.TryHideAll();
        _sceneSelectionGate.UpdateInteractable(_loadSceneButton, _sceneKeyDropdown, sceneKeys);
    }

    private void OnDisable()
    {
        _loadSpriteButton.onClick.RemoveAllListeners();
        _loadPrefabButton.onClick.RemoveAllListeners();
        _unloadAllButton.onClick.RemoveAllListeners();
        _loadSceneButton.onClick.RemoveAllListeners();

        if (_assetKeyDropdown != null)
            _assetKeyDropdown.onValueChanged.RemoveAllListeners();
        
        if (_sceneKeyDropdown != null)
            _sceneKeyDropdown.onValueChanged.RemoveAllListeners();

        _livePreviewer.ReleasePreviewIfAny();
    }
    
    private async Task PopulateAssetDropdownAsync()
    {
        assetKeys.Clear();
        assetKeys.AddRange(await _assetKeyListProvider.GetAssetKeysAsync());
        _optionsPopulator.Populate(_assetKeyDropdown, assetKeys, assetOptions);
        assetKeys.RemoveAll(key => !string.IsNullOrEmpty(key) &&
                                 key.StartsWith(GroupNameKeys.KEY_GROUP_SCENES, StringComparison.OrdinalIgnoreCase));
        _optionsPopulator.Populate(_assetKeyDropdown, assetKeys, assetOptions);
        _itemImageSlotInstaller.EnsureItemImageSlot(_assetKeyDropdown, _assetItemLoadedPrototype);
        _assetCaptionOverlay.Build(_assetKeyDropdown);

        string currentKey = GetAssetKeyByIndex(_assetKeyDropdown != null ? _assetKeyDropdown.value : -1);
        await _buttonsAvailabilityUpdater.UpdateForKeyAsync(currentKey);
        await _livePreviewer.PreviewSelectedAssetAsync(currentKey);

        UpdateAssetLoadedOverlay();
    }

    private async Task PopulateSceneDropdownAsync()
    {
        sceneKeys.Clear();
        sceneKeys.AddRange(await _sceneKeyListProvider.GetSceneKeysAsync());
        _optionsPopulator.Populate(_sceneKeyDropdown, sceneKeys, sceneOptions);
        _itemImageSlotInstaller.EnsureItemImageSlot(_sceneKeyDropdown, _sceneItemLoadedPrototype);
        _sceneCaptionOverlay.Build(_sceneKeyDropdown);
        _sceneSelectionGate.UpdateInteractable(_loadSceneButton, _sceneKeyDropdown, sceneKeys);
        
        UpdateSceneLoadedOverlay();
    }
    
    private async void OnAssetDropdownChanged(int index)
    {
        string key = GetAssetKeyByIndex(index);
        await _buttonsAvailabilityUpdater.UpdateForKeyAsync(key);
        await _livePreviewer.PreviewSelectedAssetAsync(key);
        
        UpdateAssetLoadedOverlay();
    }

    private void OnSceneDropdownChanged(int _)
    {
        _sceneSelectionGate.UpdateInteractable(_loadSceneButton, _sceneKeyDropdown, sceneKeys);
        
        UpdateSceneLoadedOverlay();
    }
    
    private async void OnLoadSpriteClicked()
    {
        string key = GetAssetKeyByIndex(_assetKeyDropdown != null ? _assetKeyDropdown.value : -1);

        await new AssetSelectionPreviewFlow(_allowedKeyFilter, _typeProbe, keyNormalizer, _prefabFlow,
                _prefabPreviewRenderer, _previewPanels, _spritePreviewImage)
            .OnAssetKeySelectedAsync(key, assetLoader, _loadedPrefabIconSprite, _loadedPrefabIconImage);
        await spriteSwapCoordinator.ApplyByAssetKey(key);
        
        UpdateAssetLoadedOverlay();
    }

    private async void OnLoadPrefabClicked()
    {
        string key = GetAssetKeyByIndex(_assetKeyDropdown != null ? _assetKeyDropdown.value : -1);

        await new AssetSelectionPreviewFlow(_allowedKeyFilter, _typeProbe, keyNormalizer,
                _prefabFlow, _prefabPreviewRenderer, _previewPanels, _spritePreviewImage)
            .OnAssetKeySelectedAsync(key, assetLoader, _loadedPrefabIconSprite, _loadedPrefabIconImage);
        await prefabSwapCoordinator.ApplyByAssetKey(key);
        
        UpdateAssetLoadedOverlay();
    }

    private async void OnUnloadAllClicked()
    {
        await _prefabFlow.TryUnloadAllAsync();
        
        _previewPanels.TryHideAll();
        prefabSwapCoordinator.ResetAll();
        spriteSwapCoordinator.ResetAll();

        if (_loadedPrefabIconImage != null)
        {
            _loadedPrefabIconImage.enabled = false;
            _loadedPrefabIconImage.sprite = null;
        }

        UpdateAssetLoadedOverlay();
        UpdateSceneLoadedOverlay();
    }

    private async void OnLoadSceneClicked()
    {
        string sceneKey = GetSceneKeyByIndex(_sceneKeyDropdown != null ? _sceneKeyDropdown.value : -1);
        
        if (string.IsNullOrWhiteSpace(sceneKey))
            return;
        
        string probeKey = keyNormalizer.Normalize(sceneKey) as string ?? sceneKey;

        var sceneFlow = new SceneLoadingFlow(safeSceneLoader, _sceneProgressSlider);

        if (!await sceneFlow.IsSceneKeyAsync(probeKey))
        {
            _sceneSelectionGate.UpdateInteractable(_loadSceneButton, _sceneKeyDropdown, sceneKeys);
            
            return;
        }

        await sceneFlow.LoadSceneByKeyAsync(probeKey);

        _sceneSelectionGate.UpdateInteractable(_loadSceneButton, _sceneKeyDropdown, sceneKeys);
        UpdateSceneLoadedOverlay();
    }
    
    private void UpdateAssetLoadedOverlay()
    {
        if (assetLoader == null)
        {
            _assetCaptionOverlay.SetActive(false);
            
            return;
        }

        string selectedKey = GetAssetKeyByIndex(_assetKeyDropdown != null ? _assetKeyDropdown.value : -1);
        bool isSelectedLoaded = !string.IsNullOrEmpty(selectedKey) && assetLoader.LoadedAssetKeys != null &&
                                assetLoader.LoadedAssetKeys.Contains(selectedKey);

        _assetCaptionOverlay.SetActive(isSelectedLoaded);
        _loadedFlagPresenter.SyncItemIcons(_assetKeyDropdown, assetKeys, assetLoader.LoadedAssetKeys,
            _assetItemLoadedPrototype != null ? _assetItemLoadedPrototype.sprite : null);
    }

    private void UpdateSceneLoadedOverlay()
    {
        bool highlight = (_sceneKeyDropdown != null && _sceneKeyDropdown.value >= 0 && _sceneKeyDropdown.value < sceneKeys.Count);
        _sceneCaptionOverlay.SetActive(highlight);
    }
    
    private string GetAssetKeyByIndex(int index)
    {
        if (assetKeys.Count == 0 || index < 0 || index >= assetKeys.Count)
            return string.Empty;
        
        return assetKeys[index];
    }

    private string GetSceneKeyByIndex(int index)
    {
        if (sceneKeys.Count == 0 || index < 0 || index >= sceneKeys.Count)
            return string.Empty;
        
        return sceneKeys[index];
    }
}