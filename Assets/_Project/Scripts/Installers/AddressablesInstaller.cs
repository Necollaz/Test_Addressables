using Zenject;

public class AddressablesInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<AllowedKeyFilter>().FromMethod(_ => new AllowedKeyFilter(new []
            {
                GroupNameKeys.KEY_GROUP_CHARACTERS, GroupNameKeys.KEY_GROUP_CHARACTER, GroupNameKeys.KEY_GROUP_UI,
                GroupNameKeys.KEY_GROUP_BUILDINGS, GroupNameKeys.KEY_GROUP_EFFECTS, GroupNameKeys.KEY_GROUP_SCENES
            })).AsSingle();
        
        Container.Bind<AddressableKeyNormalizer>().AsSingle();
        Container.Bind<AddressableKeyCatalog>().AsSingle();
        Container.Bind<AssetTypeProbe>().AsSingle();
        Container.Bind<AddressablesInitializer>().AsSingle();
        Container.Bind<AddressablesDiagnostics>().AsSingle();
        Container.Bind<AddressablesAssetLoader>().AsSingle();
        Container.Bind<AddressablesSceneLoader>().AsSingle();
        Container.Bind<PrefabSwapCoordinator>().AsSingle();
        Container.Bind<SpriteSwapCoordinator>().AsSingle(); 
    }
}