using System;
using System.Collections.Generic;
using UnityEngine;

public enum MarketplaceType
{
    Tester = -1,
    InGame = 0,
    Cafebazaar = 1,
    Myket = 2,
    GooglePlay = 3,
    AppStore = 4,
    AbrStudio = 5,
    Zarinpal = 6,
}

[Serializable]
public class MarketplaceStoreData
{
    public MarketplaceType type;
    public MarketplaceConfig marketplaceConfig;
    public StoreConfigs storeConfig;
}

[Serializable]
public class SkuDetailsSucceedData
{
    public string productId;
    public string priceType;
    public float price;
}

[Serializable]
public class PurchaseProductSucceedData
{
    public string productId;
    public string purchaseToken;
}

[Serializable]
public class ConsumeProductSucceedData
{
    public string productId;
    public string purchaseToken;
}

[Serializable]
public class QueryPurchasesSucceedData
{
    public string productId;
    public string purchaseToken;
}

[Serializable]
public class ZarinPalPurchaseData
{
    public string SKU;
    public long price;
    public string description;
}

public enum MarketplaceFlowState
{
    Idle = 0,
    Purchasing = 1,
    Consuming = 2
}

public enum ZarinPalPaymentResult
{
    Ignored = 0,
    Paid = 1,
    Failed = 2
}

public class MarketplaceOperation
{
    public string sku;
    public bool hideLoading;
    public Action<PurchaseProductSucceedData> onPurchaseDone;
    public Action<ConsumeProductSucceedData> onConsumeDone;
    public Action<string> onError;
    public Action<string> onNotAvailable;
}

public class SkuListRequest
{
    public List<string> skus;
    public Action<List<SkuDetailsSucceedData>> callBack;
}

public class SKUData
{
    public Action<SkuDetailsSucceedData> callBack;
    public SkuDetailsSucceedData skuDetailData = null;
    public bool requested;

    public SKUData()
    {
        callBack = null;
        skuDetailData = null;
    }

    public SKUData(Action<SkuDetailsSucceedData> callback)
    {
        callBack += callback;
        skuDetailData = null;
    }
}

[Serializable]
public class MyKetAuthResponse
{
    public string token;
}

[Serializable]
public class MyKetApplicationInfo
{
    public MyKetVersion version;
}

[Serializable]
public class MyKetVersion
{
    public string name;
    public int code;
    public bool isIncompatible;
}

[Serializable]
public class MarketUpdateResult
{
    public string currentVersionName;
    public int currentVersionCode;
    public string marketVersionName;
    public int marketVersionCode;
    public bool updateAvailable;
}

[Serializable]
public class BazaarApplicationInfo
{
    public int versionCode;
}

[Serializable]
public class BazaarDownloadResponse
{
    public BazaarSingleReply singleReply;
}

[Serializable]
public class BazaarSingleReply
{
    public BazaarDownloadInfo appDownloadInfoReply;
}

[Serializable]
public class BazaarDownloadInfo
{
    public int versionCode;
}

public class GooglePlayUpdateCallback : AndroidJavaProxy
{
    private readonly Action<bool> _callback;

    public GooglePlayUpdateCallback(Action<bool> callback, string packageName) : base(packageName +
        ".UpdateChecker$Callback")
    {
        _callback = callback;
    }

    public void onResult(bool updateAvailable)
    {
        _callback?.Invoke(updateAvailable);
    }
}

public class ZarinpalPurchaseData
{
    public string displayname;
    public float price;
    public string userid;
    public string appversion;
    public string platform;
    public string gamepkg;
    public string callbackurl;
    public string autoverify;
    public string merchant_id;
}

public class ZarinpalPendingData
{
    public string userid;
    public string gamepkg;
}

[Serializable]
public class ZarinpalPendingResponse
{
    public bool status;
    public ZarinpalPendingResponseData[] data;
}

[Serializable]
public class ZarinpalPendingResponseData
{
    public string payment_token;
    public string store;
    public string user_id;
    public string user_callback;
    public string price;
    public string currency;
    public string platform;
    public string package_display_name;
    public string status;
}

[Serializable]
public class ZarinpalSKU
{
    public string sku;
    public float price;
    public string displayName;
}

public class ZarinpalPurchase
{
    public string ProductID;
    public string Authority;
}