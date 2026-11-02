using UnityEditor;

public static class MarketplaceConfigMenu
{
    private const string Folder = MarketplaceConfigGenerator.ParentFolder;
    private const string CafeBazaar = Folder + "/CafeBazaarConfig.asset";
    private const string MyKet = Folder + "/MyKetConfig.asset";
    private const string GooglePlay = Folder + "/GooglePlayConfig.asset";
    private const string ZarinPal = Folder + "/ZarinPalConfig.asset";
    private const string InGame = Folder + "/InGameConfig.asset";
    private const string Manager = Folder + "/" + MarketplaceData.FileName + ".asset";

    public static readonly string[] Paths =
    {
        CafeBazaar, MyKet, GooglePlay, ZarinPal, InGame, Manager, MarketplaceSdkVersionsConfig.AssetPath,
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

    [MenuItem("OneUp/Marketplace/Configs/Manager")]
    private static void OpenManager() => Open(Manager);

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
