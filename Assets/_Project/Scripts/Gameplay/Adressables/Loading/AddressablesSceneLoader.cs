using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class AddressablesSceneLoader
{
    private AsyncOperationHandle<SceneInstance>? _currentSceneHandle;
    private float _currentProgress;

    public float CurrentSceneLoadingProgress
    {
        get
        {
            try
            {
                if (!_currentSceneHandle.HasValue)
                    return 0f;

                var sceneHandle = _currentSceneHandle.Value;
                
                if (!sceneHandle.IsValid())
                    return 0f;
                
                return sceneHandle.PercentComplete;
            }
            catch
            {
                return 0f;
            }
        }
    }

    public async Task TryLoadScene(string sceneKey, LoadSceneMode mode)
    {
        if (sceneKey == null)
            throw new ArgumentNullException(nameof(sceneKey));

        var previous = _currentSceneHandle;
        var load = Addressables.LoadSceneAsync(sceneKey, mode, activateOnLoad: true);
        
        while (load.IsValid() && !load.IsDone)
        {
            _currentProgress = load.PercentComplete;
            await Task.Yield();
        }
        
        if (!load.IsValid() || load.Status != AsyncOperationStatus.Succeeded)
        {
            if (load.IsValid())
                Addressables.Release(load);

            _currentSceneHandle = null;
            _currentProgress = 0f;
            throw load.OperationException ?? new InvalidKeyException($"Key '{sceneKey}' is not a Scene.");
        }
        
        _currentSceneHandle = load;
        _currentProgress = 1f;
        
        if (previous.HasValue && previous.Value.IsValid())
        {
            try
            {
                var unloadOp = Addressables.UnloadSceneAsync(previous.Value, autoReleaseHandle: true);
                
                while (unloadOp.IsValid() && !unloadOp.IsDone)
                    await Task.Yield();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SceneLoader] Unload previous scene failed: {e.Message}");
            }
        }
    }

    public async Task LoadSceneWithExplicitActivation(string sceneKey, LoadSceneMode mode)
    {
        var load = Addressables.LoadSceneAsync(sceneKey, mode, activateOnLoad: false);

        while (load.IsValid() && load.PercentComplete < 0.9f)
            await Task.Yield();

        if (!load.IsValid())
            throw load.OperationException ?? new Exception("Load handle became invalid.");

        var activate = load.Result.ActivateAsync();
        
        while (!activate.isDone)
            await Task.Yield();

        if (!load.IsValid() || load.Status != AsyncOperationStatus.Succeeded)
        {
            if (load.IsValid())
                Addressables.Release(load);
            throw load.OperationException ?? new Exception("Activation failed");
        }
    }
}