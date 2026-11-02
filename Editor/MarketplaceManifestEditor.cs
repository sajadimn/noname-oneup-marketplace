using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class MarketplaceManifestEditor
{
    public const string PurchasingVersion = "5.4.4";
    public const string AppUpdateVersion = "1.8.5";
    public const string OpenUpmRegistryUrl = "https://package.openupm.com";

    private const string PurchasingPackage = "com.unity.purchasing";
    private const string AppUpdatePackage = "com.google.play.appupdate";
    private const string GoogleScope = "com.google";

    public static string AddSdks(string manifestJson)
    {
        return AddSdks(manifestJson, new MarketplaceSdkVersions());
    }

    public static string AddSdks(string manifestJson, MarketplaceSdkVersions versions)
    {
        JObject manifest = JObject.Parse(manifestJson);
        if (manifest["dependencies"] is not JObject dependencies)
        {
            dependencies = new JObject();
            manifest["dependencies"] = dependencies;
        }

        foreach (KeyValuePair<string, string> package in GetPackages(versions))
        {
            if (dependencies[package.Key] == null)
                dependencies[package.Key] = package.Value;
        }

        AddGoogleScope(manifest);
        return manifest.ToString(Formatting.Indented);
    }

    public static string ApplyVersions(string manifestJson, MarketplaceSdkVersions versions)
    {
        JObject manifest = JObject.Parse(manifestJson);
        foreach (KeyValuePair<string, string> package in GetPackages(versions))
        {
            if (manifest["dependencies"]?[package.Key] != null)
                manifest["dependencies"][package.Key] = package.Value;
        }

        return manifest.ToString(Formatting.Indented);
    }

    public static bool HasSdks(string manifestJson)
    {
        JToken dependencies = JObject.Parse(manifestJson)["dependencies"];
        return dependencies != null && GetPackages(new MarketplaceSdkVersions()).Keys.All(x => dependencies[x] != null);
    }

    private static Dictionary<string, string> GetPackages(MarketplaceSdkVersions versions)
    {
        return new Dictionary<string, string>
        {
            {PurchasingPackage, versions.purchasing},
            {AppUpdatePackage, versions.appUpdate},
        };
    }

    private static void AddGoogleScope(JObject manifest)
    {
        if (manifest["scopedRegistries"] is not JArray registries)
        {
            registries = new JArray();
            manifest["scopedRegistries"] = registries;
        }

        if (registries.OfType<JObject>().Any(x => x["scopes"].Any(y => (string)y == GoogleScope)))
            return;

        JObject registry = registries.OfType<JObject>().FirstOrDefault(x => (string)x["url"] == OpenUpmRegistryUrl);
        if (registry == null)
        {
            registry = new JObject {["name"] = "OpenUPM", ["url"] = OpenUpmRegistryUrl, ["scopes"] = new JArray()};
            registries.Add(registry);
        }

        ((JArray)registry["scopes"]).Add(GoogleScope);
    }
}
