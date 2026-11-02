using UnityEditor;
using UnityEngine;

public class MarketplaceSdkVersionsConfig : ScriptableObject
{
    public const string AssetPath = "Assets/Settings/Marketplace/MarketplaceSdkVersionsConfig.asset";

    public MarketplaceSdkVersions versions = new MarketplaceSdkVersions();

    public static MarketplaceSdkVersions Load()
    {
        MarketplaceSdkVersionsConfig config = AssetDatabase.LoadAssetAtPath<MarketplaceSdkVersionsConfig>(AssetPath);
        return config != null ? config.versions : new MarketplaceSdkVersions();
    }
}
