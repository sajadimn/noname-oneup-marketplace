using UnityEditor;

public static class MarketplaceConfigMenu
{
    private const string Folder = MarketplaceConfigGenerator.ParentFolder;
    private const string CafeBazaar = Folder + "/CafeBazaarConfig.asset";
    private const string MyKet = Folder + "/MyKetConfig.asset";
    private const string GooglePlay = Folder + "/GooglePlayConfig.asset";
    private const string ZarinPal = Folder + "/ZarinPalConfig.asset";
    private const string InGame = Folder + "/InGameConfig.asset";
    private const string Data = Folder + "/" + MarketplaceData.FileName + ".asset";
    private const string IranianStore = Folder + "/IranianStoreConfig.asset";
    private const string GlobalStore = Folder + "/GlobalStoreConfig.asset";
    private const string TesterStore = Folder + "/TesterStoreConfig.asset";

    public static readonly string[] Paths =
    {
        CafeBazaar, MyKet, GooglePlay, ZarinPal, InGame, Data, IranianStore, GlobalStore, TesterStore, MarketplaceSdkVersionsConfig.AssetPath,
    };

    [MenuItem("OneUp/Marketplace/Configs/CafeBazaar")]
    private static void OpenCafeBazaar() => Open(CafeBazaar);

    [MenuItem("OneUp/Marketplace/Configs/MyKet")]
    private static void OpenMyKet() => Open(MyKet);

    [MenuItem("OneUp/Marketplace/Configs/GooglePlay")]
    private static void OpenGooglePlay() => Open(GooglePlay);

    [MenuItem("OneUp/Marketplace/Configs/ZarinPal")]
    private static void OpenZarinPal() => Open(ZarinPal);

    [MenuItem("OneUp/Marketplace/Configs/InGame")]
    private static void OpenInGame() => Open(InGame);

    [MenuItem("OneUp/Marketplace/Configs/Marketplace Data")]
    private static void OpenData() => Open(Data);

    [MenuItem("OneUp/Marketplace/Configs/Store - Iranian")]
    private static void OpenIranianStore() => Open(IranianStore);

    [MenuItem("OneUp/Marketplace/Configs/Store - Global")]
    private static void OpenGlobalStore() => Open(GlobalStore);

    [MenuItem("OneUp/Marketplace/Configs/Store - Tester")]
    private static void OpenTesterStore() => Open(TesterStore);

    [MenuItem("OneUp/Marketplace/Configs/SDK Versions")]
    private static void OpenSdkVersions() => Open(MarketplaceSdkVersionsConfig.AssetPath);

    private static void Open(string path)
    {
        MarketplaceConfigGenerator.Generate();
        UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(path);
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }
}
