using System.Collections.Generic;
using UnityEngine;

public class ZarinPalConfig : MarketplaceConfig
{
    public string key = "";
    [Header("Web Store")] public string paymentStartUrl = "";
    public string paymentPendingUrl = "";
    [Header("Callback")] public string callbackScheme = "";
    public string callbackHost = "";
    public string callbackActivity = "com.unity3d.player.UnityPlayerActivity";
    [Header("SKU")] public List<ZarinpalSKU> skus = new List<ZarinpalSKU>();
}
