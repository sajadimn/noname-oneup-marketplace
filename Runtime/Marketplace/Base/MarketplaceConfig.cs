using UnityEngine;

public abstract class MarketplaceConfig : ScriptableObject
{
    public MarketplaceType marketplaceType;
    public string priceType = "USD";
    [HideInInspector]public string currencyCode = "USD";
    public string packageName;
    public string PackageName => string.IsNullOrEmpty(packageName) ? Application.identifier : packageName;
}