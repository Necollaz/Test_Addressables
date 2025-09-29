using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesDiagnostics
{
    public delegate void OnAssetLoad(string key);
    public delegate void OnAssetUnload(string key);

    public event OnAssetLoad OnAssetLoaded;
    public event OnAssetUnload OnAssetUnloaded;
    
    private Action<AsyncOperationHandle, Exception> _previousExceptionHandler;

    private readonly StringBuilder sharedStringBuilder = new StringBuilder();
    private readonly System.Collections.Generic.Dictionary<string, Stopwatch> stopwatchesByKey =
        new System.Collections.Generic.Dictionary<string, Stopwatch>(256);

    private bool _enabled;

    public void Enable()
    {
        if (_enabled)
            return;

        _enabled = true;

        _previousExceptionHandler = ResourceManager.ExceptionHandler;
        ResourceManager.ExceptionHandler = WrapExceptionHandler(_previousExceptionHandler);

        Addressables.ResourceManager.RegisterDiagnosticCallback(HandleDiagnosticContext);
    }

    public void Disable()
    {
        if (!_enabled)
            return;

        _enabled = false;

        ResourceManager.ExceptionHandler = _previousExceptionHandler;
        _previousExceptionHandler = null;

        Addressables.ResourceManager.UnregisterDiagnosticCallback(HandleDiagnosticContext);

        stopwatchesByKey.Clear();
    }

    public void NotifyAssetLoaded(string key)
    {
        StopAndLogTimerIfAny(key, "Loaded");
        OnAssetLoaded?.Invoke(key);
    }

    public void NotifyAssetUnloaded(string key)
    {
        StopAndLogTimerIfAny(key, "Unloaded");
        OnAssetUnloaded?.Invoke(key);
    }

    public void StartTimer(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (!stopwatchesByKey.TryGetValue(key, out var stopwatch))
        {
            stopwatch = new Stopwatch();
            stopwatchesByKey[key] = stopwatch;
        }

        stopwatch.Restart();
    }

    private void StopAndLogTimerIfAny(string key, string phase)
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (stopwatchesByKey.TryGetValue(key, out var stopwatch))
        {
            if (stopwatch.IsRunning)
                stopwatch.Stop();

            UnityEngine.Debug.Log($"[Addressables][{phase}] {key} in {stopwatch.ElapsedMilliseconds} ms");
        }
    }

    private Action<AsyncOperationHandle, Exception> WrapExceptionHandler(Action<AsyncOperationHandle, Exception> previousHandler)
    {
        return (handle, exception) =>
        {
            previousHandler?.Invoke(handle, exception);
        };
    }

    private void HandleDiagnosticContext(ResourceManager.DiagnosticEventContext context)
    {
        if (context.Type != ResourceManager.DiagnosticEventType.AsyncOperationComplete)
            return;

        string displayName = context.OperationHandle.DebugName ?? "Unknown";

        sharedStringBuilder.Clear();
        sharedStringBuilder.Append("[Addressables][Complete] ");
        sharedStringBuilder.Append(displayName);
        sharedStringBuilder.Append(" | Id:").Append(context.EventValue);
        sharedStringBuilder.Append(" | Frame:").Append(Time.frameCount);

        if (context.Context != null)
            sharedStringBuilder.Append(" | Ctx: ").Append(context.Context);

        UnityEngine.Debug.Log(sharedStringBuilder.ToString());
    }
}