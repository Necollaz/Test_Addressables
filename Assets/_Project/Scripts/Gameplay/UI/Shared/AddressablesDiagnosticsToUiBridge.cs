using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class AddressablesDiagnosticsToUiBridge : MonoBehaviour
{
    private const string ADDRESSABLES_PREFIX = "[Addressables]";
    
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private UILogScrollList _logList;
    
    private readonly ConcurrentQueue<string> _pendingLines = new ConcurrentQueue<string>();
    
    private AddressablesDiagnostics _diagnostics;
    private bool _scrollQueued;

    [Inject]
    private void Construct(AddressablesDiagnostics diagnostics)
    {
        _diagnostics = diagnostics;
    }
    
    private void OnEnable()
    {
        if (_diagnostics != null)
        {
            _diagnostics.OnAssetLoaded += HandleLoaded;
            _diagnostics.OnAssetUnloaded += HandleUnloaded;
        }

        Application.logMessageReceivedThreaded += HandleUnityLogThreaded;
        
        EnqueueLine("UI Bridge enabled");
    }

    private void OnDisable()
    {
        if (_diagnostics != null)
        {
            _diagnostics.OnAssetLoaded -= HandleLoaded;
            _diagnostics.OnAssetUnloaded -= HandleUnloaded;
        }

        Application.logMessageReceivedThreaded -= HandleUnityLogThreaded;
    }
    
    private void Update()
    {
        bool hadAny = false;

        while (_pendingLines.TryDequeue(out var line))
        {
            _logList?.Append(line);
            hadAny = true;
        }

        if (hadAny)
            QueueScrollToBottom();
    }

    private void HandleLoaded(string key) => EnqueueLine($"Loaded: {key}");

    private void HandleUnloaded(string key) => EnqueueLine($"Unloaded: {key}");

    private void HandleUnityLogThreaded(string condition, string stacktrace, LogType type)
    {
        if (string.IsNullOrEmpty(condition))
            return;

        if (!condition.StartsWith(ADDRESSABLES_PREFIX, System.StringComparison.Ordinal))
            return;

        EnqueueLine(condition);
    }
    
    private void EnqueueLine(string line)
    {
        _pendingLines.Enqueue(line);
    }

    private void QueueScrollToBottom()
    {
        if (_scrollRect == null || _scrollQueued)
            return;

        _scrollQueued = true;
        
        StartCoroutine(ScrollBottomNextFrame());
    }

    private IEnumerator ScrollBottomNextFrame()
    {
        yield return null;
        
        Canvas.ForceUpdateCanvases();
        _scrollRect.verticalNormalizedPosition = 0f;

        Canvas.ForceUpdateCanvases();
        _scrollQueued = false;
    }
}