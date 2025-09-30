using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ExitToMainSceneButton : MonoBehaviour
{
    [SerializeField] private Button _exitButton;
    [SerializeField] private string _mainSceneKey = "scenes/main";
    
    private AddressablesAssetLoader _assetLoader;
    private AddressablesInitializer _initializer;
    private WebGLSafeSceneLoader _safeSceneLoader;
    
    [Inject]
    private void Construct(AddressablesAssetLoader assetLoader, AddressablesInitializer initializer, WebGLSafeSceneLoader safeSceneLoader)
    {
        _assetLoader = assetLoader;
        _initializer = initializer;
        _safeSceneLoader = safeSceneLoader;
    }

    private void OnEnable()
    {
        if (_exitButton != null)
            _exitButton.onClick.AddListener(ExitToMainScene);
    }

    private void OnDisable()
    {
        if (_exitButton != null)
            _exitButton.onClick.RemoveAllListeners();
    }
    
    private async void ExitToMainScene()
    {
        if (string.IsNullOrWhiteSpace(_mainSceneKey))
            return;

        await _assetLoader.UnloadAllAssetsAsync();
        await _initializer.RefreshCatalogIfNeeded(false);
        await _safeSceneLoader.LoadSingleAsync(_mainSceneKey);
    }
}