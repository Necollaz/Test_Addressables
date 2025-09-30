using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public sealed class WebGLSafeSceneLoader
{
    private readonly bool _allowNameFallback;

    public WebGLSafeSceneLoader(bool allowNameFallback = true)
    {
        _allowNameFallback = allowNameFallback;
    }

    public async Task LoadSingleAsync(string sceneKey)
    {
        if (string.IsNullOrWhiteSpace(sceneKey))
            throw new ArgumentNullException(nameof(sceneKey));
        
        Exception addrEx = null;
        
        try
        {
            await Addressables.InitializeAsync().Task;
            
            var sizeH = Addressables.GetDownloadSizeAsync(sceneKey);
            long bytes = 0;
            
            try
            {
                bytes = await sizeH.Task;
            }
            finally
            {
                if (sizeH.IsValid())
                    Addressables.Release(sizeH);
            }

            if (bytes > 0)
            {
                var deps = Addressables.DownloadDependenciesAsync(sceneKey, true);
                
                try
                {
                    while (deps.IsValid() && !deps.IsDone)
                        await Task.Yield();
                }
                finally
                {
                    if (deps.IsValid())
                        Addressables.Release(deps);
                }
            }

            var load = Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Single, true);
            
            while (load.IsValid() && !load.IsDone)
                await Task.Yield();

            if (!load.IsValid() || load.Status != AsyncOperationStatus.Succeeded)
                throw load.OperationException ?? new InvalidOperationException($"Addressables failed for '{sceneKey}'.");
            
            return;
        }
        catch (Exception e)
        {
            addrEx = e;
        }
        
        if (_allowNameFallback)
        {
            try
            {
                string sceneName = ExtractSceneName(sceneKey);
                var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                if (op == null)
                    throw new InvalidOperationException($"SceneManager.LoadSceneAsync returned null for '{sceneName}'.");

                while (!op.isDone) await Task.Yield();
                return; // успех
            }
            catch (Exception nameEx)
            {
                Debug.LogError($"[SceneLoad] Addressables failed: {addrEx?.Message}\nName fallback failed: {nameEx.Message}");
                throw;
            }
        }
        
        throw addrEx ?? new Exception("Unknown scene load failure.");
    }

    private string ExtractSceneName(string key)
    {
        var k = key.Replace('\\','/').Trim();
        int slash = k.LastIndexOf('/');
        
        if (slash >= 0 && slash + 1 < k.Length)
            k = k.Substring(slash + 1);
        
        if (k.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
            k = k.Substring(0, k.Length - ".unity".Length);
        
        return k;
    }
}