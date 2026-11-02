using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class ZarinPalManifest
{
    private const string ConfigPath = "Assets/Settings/Marketplace/Resources/ZarinPalConfig.asset";
    private const string ManifestFile = "Market.androidlib/AndroidManifest.xml";
    private const string TemplatePath = "Packages/noname-oneup-marketplace/Stores~/ZarinPal/" + ManifestFile;
    private const string ManifestId = "com.oneup.marketplace.zarinpal";
    public const string CurrentMarketFolder = "Assets/Plugins/Android/Market";
    private const string CallbackBlock = "(?s)<!--callback-->.*?<!--/callback-->";

    public static string Fill(string template, string scheme, string host, string activity)
    {
        string manifest = template.Replace("{activity}", activity);
        if (string.IsNullOrEmpty(scheme) || string.IsNullOrEmpty(host))
            return Regex.Replace(manifest, CallbackBlock, string.Empty);

        return manifest.Replace("{scheme}", scheme).Replace("{host}", host);
    }

    public static void FillInFolder(string marketFolder)
    {
        string path = System.IO.Path.Combine(marketFolder, ManifestFile);
        if (!File.Exists(path) || !File.ReadAllText(path).Contains(ManifestId))
            return;

        ZarinPalConfig config = AssetDatabase.LoadAssetAtPath<ZarinPalConfig>(ConfigPath);
        string scheme = config != null ? config.callbackScheme : string.Empty;
        string host = config != null ? config.callbackHost : string.Empty;
        string activity = config != null ? config.callbackActivity : new ZarinPalConfigDefaults().callbackActivity;
        if (string.IsNullOrEmpty(scheme) || string.IsNullOrEmpty(host))
            Debug.LogWarning("ZarinPalManifest - Callback is empty in ZarinPalConfig, so ZarinPal cannot return to the game");

        string template = File.ReadAllText(System.IO.Path.GetFullPath(TemplatePath));
        File.WriteAllText(path, Fill(template, scheme, host, activity));
    }

    public static bool IsConfig(string path)
    {
        return path == ConfigPath;
    }

    private class ZarinPalConfigDefaults
    {
        public readonly string callbackActivity = "com.unity3d.player.UnityPlayerActivity";
    }
}
