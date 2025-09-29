using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class AddressablesBootstrap : MonoBehaviour
{
    private AddressablesInitializer _initializer;
    private AddressablesDiagnostics _diagnostics;

    [Inject]
    private void Construct(AddressablesInitializer initializer, AddressablesDiagnostics diagnostics)
    {
        _initializer = initializer;
        _diagnostics = diagnostics;
    }
    
    private async void Start()
    {
        _diagnostics?.Enable();
        
        await SafeInitialize();
    }

    private void OnDestroy()
    {
        _diagnostics.Disable();
    }
    
    private async Task SafeInitialize()
    {
        try
        {
            await _initializer.InitializeAndUpdateCatalogs();
        }
        catch (SystemException exception)
        {
            Debug.LogError($"Addressables initialization failed: {exception.Message}");
        }
    }
}