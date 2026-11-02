using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

[InitializeOnLoad]
public static class MarketplaceSdkInstaller
{
    private const string ManifestPath = "Packages/manifest.json";
    private const string AttemptsKey = "OneUp.Marketplace.InstallAttempts";
    private const int MaxAttempts = 2;

    static MarketplaceSdkInstaller()
    {
        if (!Application.isBatchMode)
            EditorApplication.delayCall += InstallMissing;
    }

    [MenuItem("OneUp/Marketplace/Install SDKs")]
    public static void Install()
    {
        SessionState.SetInt(AttemptsKey, 0);
        InstallMissing();
    }

    [MenuItem("OneUp/Marketplace/Update SDKs")]
    public static void UpdateSdks()
    {
        string manifest = File.ReadAllText(ManifestPath);
        WriteManifest(MarketplaceManifestEditor.ApplyVersions(manifest, MarketplaceSdkVersionsConfig.Load()));
    }

    private static void InstallMissing()
    {
        string manifest = File.ReadAllText(ManifestPath);
        int attempts = SessionState.GetInt(AttemptsKey, 0);
        if (MarketplaceManifestEditor.HasSdks(manifest) || attempts >= MaxAttempts)
            return;

        SessionState.SetInt(AttemptsKey, attempts + 1);
        Debug.Log("MarketplaceSdkInstaller - Installing missing store SDKs");
        WriteManifest(MarketplaceManifestEditor.AddSdks(manifest, MarketplaceSdkVersionsConfig.Load()));
    }

    private static void WriteManifest(string manifest)
    {
        File.WriteAllText(ManifestPath, manifest);
        Client.Resolve();
    }
}
