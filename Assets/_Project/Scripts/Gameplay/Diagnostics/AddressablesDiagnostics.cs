using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;

public class AddressablesDiagnostics
{
    public delegate void OnAssetLoad(string key);
    public delegate void OnAssetUnload(string key);

    public event OnAssetLoad OnAssetLoaded;
    public event OnAssetUnload OnAssetUnloaded;
    
    private Action<AsyncOperationHandle, Exception> _previousExceptionHandler;

    private readonly StringBuilder sharedStringBuilder = new StringBuilder();
    private readonly Dictionary<string, Stopwatch> stopwatchesByKey = new Dictionary<string, Stopwatch>(256);

    private Type _collectorType;
    private Type _eventType;
    private MethodInfo _initializeMethod;
    private PropertyInfo _profileEventsProp;
    private MethodInfo _registerMethod;
    private MethodInfo _unregisterMethod;
    private Delegate _handlerDelegate;
    
    private bool _enabled;

    public void Enable()
    {
        if (_enabled)
            return;

        _enabled = true;

        _previousExceptionHandler = ResourceManager.ExceptionHandler;
        ResourceManager.ExceptionHandler = WrapExceptionHandler(_previousExceptionHandler);
    }

    public void Disable()
    {
        if (!_enabled)
            return;

        _enabled = false;

        ResourceManager.ExceptionHandler = _previousExceptionHandler;
        _previousExceptionHandler = null;

        stopwatchesByKey.Clear();
    }

    public void NotifyAssetLoaded(string key)
    {
        StopAndLogTimerIfAny(key);
        
        OnAssetLoaded?.Invoke(key);
    }

    public void NotifyAssetUnloaded(string key)
    {
        StopAndLogTimerIfAny(key);
        
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

    private void StopAndLogTimerIfAny(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (stopwatchesByKey.TryGetValue(key, out Stopwatch stopwatch) && stopwatch.IsRunning)
            stopwatch.Stop();
        
        sharedStringBuilder.Clear();
        sharedStringBuilder.Append("[Addressables][Loaded] ").Append(key).Append(" | ms: ").Append(stopwatch?.ElapsedMilliseconds ?? 0);
        
        Debug.Log(sharedStringBuilder.ToString());
    }

    private Action<AsyncOperationHandle, Exception> WrapExceptionHandler(Action<AsyncOperationHandle, Exception> previousHandler)
    {
        return (handle, exception) => { previousHandler?.Invoke(handle, exception); };
    }
}