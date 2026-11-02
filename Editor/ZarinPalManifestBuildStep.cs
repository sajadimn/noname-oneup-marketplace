using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class ZarinPalManifestBuildStep : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.Android)
            ZarinPalManifest.FillInFolder(ZarinPalManifest.CurrentMarketFolder);
    }
}
