using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadingFlow
{
    private readonly WebGLSafeSceneLoader safeLoader;
    private readonly Slider progressSlider;

    public SceneLoadingFlow(WebGLSafeSceneLoader safeLoader, Slider progressSlider)
    {
        this.safeLoader = safeLoader;
        this.progressSlider = progressSlider;
    }

    public async Task<bool> IsSceneKeyAsync(object keyOrLabel)
    {
        await Addressables.InitializeAsync().Task;
        
        var h = Addressables.LoadResourceLocationsAsync(keyOrLabel, typeof(SceneInstance));
        var locs = await h.Task;
        
        if (h.IsValid())
            Addressables.Release(h);
        
        return locs != null && locs.Count > 0;
    }

    public async Task LoadSceneByKeyAsync(string sceneKey)
    {
        if (progressSlider)
            progressSlider.gameObject.SetActive(false);
        
        await safeLoader.LoadSingleAsync(sceneKey);
    }
    
    // private const float PROGRESS_COMPLETE_THRESHOLD = 0.999f;
    // private const float PROGRESS_COMPLETE_VALUE = 1f;
    //
    // private readonly AddressablesSceneLoader sceneLoader;
    // private readonly Slider progressSlider;
    //
    // public SceneLoadingFlow(AddressablesSceneLoader sceneLoader, Slider progressSlider)
    // {
    //     this.sceneLoader = sceneLoader;
    //     this.progressSlider = progressSlider;
    // }
    //
    // public async Task<bool> IsSceneKeyAsync(object keyOrLabel)
    // {
    //     await Addressables.InitializeAsync().Task;
    //     
    //     var handle = Addressables.LoadResourceLocationsAsync(keyOrLabel, typeof(SceneInstance));
    //     var locations = await handle.Task;
    //     Addressables.Release(handle);
    //     
    //     return locations != null && locations.Count > 0;
    // }
    //
    // public async Task LoadSceneByKeyAsync(string sceneKey)
    // {
    //     if (progressSlider != null)
    //     {
    //         progressSlider.gameObject.SetActive(true);
    //         progressSlider.value = 0f;
    //     }
    //     
    //     var sizeHandle = Addressables.GetDownloadSizeAsync(sceneKey);
    //     long bytes = await sizeHandle.Task;
    //     Addressables.Release(sizeHandle);
    //
    //     if (bytes > 0)
    //     {
    //         var downloadDependencies = Addressables.DownloadDependenciesAsync(sceneKey, true);
    //         
    //         while (!downloadDependencies.IsDone)
    //         {
    //             if (progressSlider != null)
    //             {
    //                 var status = downloadDependencies.GetDownloadStatus();
    //                 
    //                 if (status.TotalBytes > 0)
    //                     progressSlider.value = Mathf.Clamp01((float)status.DownloadedBytes / status.TotalBytes) * 0.85f;
    //             }
    //             
    //             await Task.Yield();
    //         }
    //         
    //         if (progressSlider != null && progressSlider.value < 0.85f)
    //             progressSlider.value = 0.85f;
    //     }
    //
    //     var progressTask = TrackSceneProgressAsync();
    //
    //     try
    //     {
    //         await sceneLoader.TryLoadScene(sceneKey, LoadSceneMode.Single);
    //     }
    //     catch (Exception e)
    //     {
    //         if (progressSlider != null)
    //             progressSlider.value = 0f;
    //         
    //         return;
    //     }
    //
    //     await progressTask;
    // }
    //
    // private async Task TrackSceneProgressAsync()
    // {
    //     if (progressSlider == null)
    //         return;
    //
    //     try
    //     {
    //         const float PROGRESS_COMPLETE_THRESHOLD = 0.999f;
    //         
    //         while (sceneLoader.CurrentSceneLoadingProgress < PROGRESS_COMPLETE_THRESHOLD)
    //         {
    //             progressSlider.value = sceneLoader.CurrentSceneLoadingProgress;
    //             await Task.Yield();
    //         }
    //         
    //         progressSlider.value = 1f;
    //     }
    //     catch
    //     {
    //         progressSlider.value = 0f;
    //     }
    // }
}