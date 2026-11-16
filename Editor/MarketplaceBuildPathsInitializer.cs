using UnityEditor;

[InitializeOnLoad]
public static class MarketplaceBuildPathsInitializer
{
    static MarketplaceBuildPathsInitializer()
    {
        EditorApplication.delayCall += MarketplaceBuildPaths.ApplyToProject;
    }

    [MenuItem("OneUp/Marketplace/Set Build Config Paths")]
    public static void Apply()
    {
        MarketplaceBuildPaths.ApplyToProject();
    }
}
