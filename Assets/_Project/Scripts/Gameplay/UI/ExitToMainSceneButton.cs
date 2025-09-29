using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ExitToMainSceneButton : MonoBehaviour
{
    [SerializeField] private Button _exitButton;
    [SerializeField] private string _mainSceneKey = "scenes/main";
    
    private AddressablesSceneLoader _sceneLoader;
    private AddressablesAssetLoader _assetLoader;
    private AddressablesInitializer _initializer;

    [Inject]
    private void Construct(AddressablesSceneLoader sceneLoader, AddressablesAssetLoader assetLoader, AddressablesInitializer initializer)
    {
        _sceneLoader = sceneLoader;
        _assetLoader = assetLoader;
        _initializer = initializer;
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
        await _initializer.RefreshCatalogIfNeeded(true);
        await _sceneLoader.TryLoadScene(_mainSceneKey, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}