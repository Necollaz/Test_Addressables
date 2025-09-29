# Addressables WebGL Demo

![Demo](Docs/gifs/Recording_02.gif)

---

![Demo](Docs/gifs/Recording_01.gif)

---

## Особенности

- Отдельные списки **ассетов** и **сцен** в UI.
- Подмена **префабов/спрайтов** без обновления приложения.
- Очистка кэша и **подтягивание нового каталога** на выход в главную сцену.
- Встроенный **лог Addressables** в скролл-UI.

---

## Структура проекта
- _Project/
  - Scenes/
    - Main.unity
    - Gameplay.unity
  - Scripts/
    - Gameplay/
      - Addressables/
        - Catalog/ (`AddressableKeyCatalog.cs`, `AllowedKeyFilter.cs`)
        - Lists/ (`AssetKeyListProvider.cs`, `SceneKeyListProvider.cs`)
        - Loading/ (`AddressablesAssetLoader.cs`, `AddressablesSceneLoader.cs`)
        - Probing/ (`AssetTypeProbe.cs`)
        - `AddressablesInitializer.cs`, `AddressablesBootstrap.cs`
      - Core/ (`AddressableKeyNormalizer.cs`, `GroupNameKeys.cs`, `GroupNameKeyType.cs`)
      - Diagnostics/ (`AddressablesDiagnostics.cs`)
      - Flows/
        - Asset/ (`AssetButtonsAvailabilityUpdater.cs`, `AssetSelectionPreviewFlow.cs`, `PrefabLoadAndPreview.cs`)
        - Scene/ (`SceneLoadingFlow.cs`)
      - Swap/ (`BaseSwapCoordinator.cs`, `PrefabSwapCoordinator.cs`, `SpriteSwapCoordinator.cs`, `SwappableAnchorBase.cs`, `SwappablePrefabAnchor.cs`, `SwappableSpriteAnchor.cs`)
      - UI/
        - Demo/ (`DemoLoaderView.cs`)
        - Dropdown/ (`DropdownCaptionOverlay.cs`, `DropdownItemImageLayout.cs`, `DropdownItemImageSlotInstaller.cs`, `DropdownLoadedFlagPresenter.cs`, `DropdownOptionsPopulator.cs`, `SceneSelectionButtonGate.cs`)
        - Preview/ (`AssetLivePreviewer.cs`, `GameObjectHierarchyLayerSetter.cs`, `PrefabPreviewRenderer.cs`, `PrefabPreviewSpinner.cs`, `PreviewLightPlacer.cs`, `PreviewPanelSwitcher.cs`, `RendererBoundsCalculator.cs`, `RenderTextureAllocator.cs`)
        - Shared/ (`AddressablesDiagnosticsToUiBridge.cs`, `UILogScrollList.cs`, `UILogView.cs`, `ExitToMainSceneButton.cs`)
    - Installers/
       - `AddressablesInstaller.cs`

---