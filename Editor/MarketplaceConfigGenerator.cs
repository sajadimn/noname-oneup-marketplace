using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class MarketplaceConfigGenerator
{
    public const string ParentFolder = "Assets/Settings/Marketplace/Resources";
    private const string LoaderPrefabPath = "Assets/Settings/Marketplace/MarketplaceLoader.prefab";
    private const string ManagerConfigPath = ParentFolder + "/" + MarketplaceData.FileName + ".asset";

    static MarketplaceConfigGenerator()
    {
        EditorApplication.delayCall += Generate;
    }

    [MenuItem("OneUp/Marketplace/Create Missing Configs")]
    public static void Generate()
    {
        CreateFolder("Assets", "Settings");
        CreateFolder("Assets/Settings", "Marketplace");
        CreateFolder("Assets/Settings/Marketplace", "Resources");

        CafeBazaarConfig cafeBazaar = Create<CafeBazaarConfig>(MarketplaceType.Cafebazaar);
        MyKetConfig myKet = Create<MyKetConfig>(MarketplaceType.Myket);
        GooglePlayConfig googlePlay = Create<GooglePlayConfig>(MarketplaceType.GooglePlay);
        ZarinPalConfig zarinPal = Create<ZarinPalConfig>(MarketplaceType.Zarinpal);
        InGameConfig inGame = Create<InGameConfig>(MarketplaceType.InGame);
        StoreConfigs iranian = CreateStore("IranianStoreConfig");
        StoreConfigs global = CreateStore("GlobalStoreConfig");
        StoreConfigs tester = CreateStore("TesterStoreConfig");
        CreateVersionsConfig();

        Canvas loader = CreateLoaderPrefab(LoaderPrefabPath);
        if (AssetExists(ManagerConfigPath))
        {
            AttachLoader(AssetDatabase.LoadAssetAtPath<MarketplaceData>(ManagerConfigPath), loader);
            return;
        }

        MarketplaceData config = ScriptableObject.CreateInstance<MarketplaceData>();
        config.loaderPrefab = loader;
        config.marketplaces = new List<MarketplaceStoreData>
        {
            new MarketplaceStoreData {type = MarketplaceType.Cafebazaar, marketplaceConfig = cafeBazaar, storeConfig = iranian},
            new MarketplaceStoreData {type = MarketplaceType.Myket, marketplaceConfig = myKet, storeConfig = iranian},
            new MarketplaceStoreData {type = MarketplaceType.GooglePlay, marketplaceConfig = googlePlay, storeConfig = global},
            new MarketplaceStoreData {type = MarketplaceType.InGame, marketplaceConfig = inGame, storeConfig = global},
            new MarketplaceStoreData {type = MarketplaceType.Tester, marketplaceConfig = cafeBazaar, storeConfig = tester},
            new MarketplaceStoreData {type = MarketplaceType.Zarinpal, marketplaceConfig = zarinPal, storeConfig = iranian},
        };
        AssetDatabase.CreateAsset(config, ManagerConfigPath);
    }

    public static Canvas CreateLoaderPrefab(string path)
    {
        if (AssetExists(path))
            return AssetDatabase.LoadAssetAtPath<Canvas>(path);

        Canvas loader = MarketplaceLoader.CreateDefault(null);
        loader.gameObject.name = "MarketplaceLoader";
        loader.gameObject.SetActive(false);
        Canvas saved = PrefabUtility.SaveAsPrefabAsset(loader.gameObject, path).GetComponent<Canvas>();
        Object.DestroyImmediate(loader.gameObject);
        return saved;
    }

    private static void AttachLoader(MarketplaceData config, Canvas loader)
    {
        if (config == null || config.loaderPrefab != null)
            return;

        config.loaderPrefab = loader;
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
    }

    private static void CreateVersionsConfig()
    {
        if (!AssetExists(MarketplaceSdkVersionsConfig.AssetPath))
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MarketplaceSdkVersionsConfig>(), MarketplaceSdkVersionsConfig.AssetPath);
    }

    private static T Create<T>(MarketplaceType type) where T : MarketplaceConfig
    {
        string path = $"{ParentFolder}/{typeof(T).Name}.asset";
        if (AssetExists(path))
            return AssetDatabase.LoadAssetAtPath<T>(path);

        T config = ScriptableObject.CreateInstance<T>();
        config.marketplaceType = type;
        AssetDatabase.CreateAsset(config, path);
        Debug.Log($"MarketplaceConfigGenerator - Created {path}");
        return config;
    }

    private static StoreConfigs CreateStore(string name)
    {
        string path = $"{ParentFolder}/{name}.asset";
        if (AssetExists(path))
            return AssetDatabase.LoadAssetAtPath<StoreConfigs>(path);

        StoreConfigs config = ScriptableObject.CreateInstance<StoreConfigs>();
        AssetDatabase.CreateAsset(config, path);
        return config;
    }

    private static bool AssetExists(string path)
    {
        return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets));
    }

    private static void CreateFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
            AssetDatabase.CreateFolder(parent, name);
    }
}
