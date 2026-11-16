using UnityEditor;
using UnityEngine;

public static class MarketplaceBuildPaths
{
    public const string BuildPipelineFolder = "Assets/Settings/BuildPipeline";
    public const string ManifestPath = BuildPipelineFolder + "/Market~/AndroidManifest.xml";
    private const string PackageName = "noname-oneup-marketplace";
    private const string MarketFilesField = "marketFilesPath";
    private const string ManifestField = "manifestPath";
    private const string PackageCacheMarker = "Library/PackageCache/" + PackageName + "@";
    private static readonly string[] MarketFileStores = { "CafeBazaar", "MyKet" };
    private static readonly string[] ManifestStores = { "CafeBazaar", "MyKet", "ZarinPal", "GooglePlay" };

    public static string MarketFilesPath(string store)
    {
        return $"Packages/{PackageName}/Stores~/{store}";
    }

    public static string MarketFilesStore(string configName)
    {
        return FindStore(configName, MarketFileStores);
    }

    public static bool UsesStoreManifest(string configName)
    {
        return FindStore(configName, ManifestStores) != null;
    }

    public static string Choose(string current, string desired)
    {
        bool isOwnMachinePath = !string.IsNullOrEmpty(current) && current.Replace('\\', '/').Contains(PackageCacheMarker);
        return string.IsNullOrEmpty(current) || isOwnMachinePath ? desired : current;
    }

    public static bool Apply(Object config)
    {
        if (config == null || !UsesStoreManifest(config.name))
            return false;

        SerializedObject serialized = new SerializedObject(config);
        bool changed = SetValue(serialized, ManifestField, ManifestPath);
        string store = MarketFilesStore(config.name);
        if (store != null)
            changed |= SetValue(serialized, MarketFilesField, MarketFilesPath(store));
        if (changed)
            serialized.ApplyModifiedPropertiesWithoutUndo();
        return changed;
    }

    public static void ApplyToProject()
    {
        if (!AssetDatabase.IsValidFolder(BuildPipelineFolder))
            return;

        bool changed = false;
        foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { BuildPipelineFolder }))
        {
            Object config = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));
            if (!Apply(config))
                continue;

            EditorUtility.SetDirty(config);
            changed = true;
        }

        if (changed)
            AssetDatabase.SaveAssets();
    }

    private static bool SetValue(SerializedObject serialized, string field, string desired)
    {
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null)
            return false;

        string value = field == ManifestField ? ChooseManifest(property.stringValue, desired) : Choose(property.stringValue, desired);
        if (value == property.stringValue)
            return false;

        property.stringValue = value;
        return true;
    }

    private static string ChooseManifest(string current, string desired)
    {
        return string.IsNullOrEmpty(current) ? desired : current;
    }

    private static string FindStore(string configName, string[] stores)
    {
        if (string.IsNullOrEmpty(configName))
            return null;

        foreach (string store in stores)
        {
            if (configName.StartsWith(store))
                return store;
        }

        return null;
    }
}
