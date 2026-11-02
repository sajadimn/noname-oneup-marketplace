using System.Linq;
using UnityEditor;

public class ZarinPalConfigWatcher : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (importedAssets.Any(ZarinPalManifest.IsConfig))
            ZarinPalManifest.FillInFolder(ZarinPalManifest.CurrentMarketFolder);
    }
}
