using System.Linq;
using UnityEditor;

public class MarketplaceBuildPathsWatcher : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (importedAssets.Any(path => path.StartsWith(MarketplaceBuildPaths.BuildPipelineFolder + "/") && path.EndsWith(".asset")))
            EditorApplication.delayCall += MarketplaceBuildPaths.ApplyToProject;
    }
}
