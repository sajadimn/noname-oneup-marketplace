#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BazaarPlugin;
using UnityEngine;
using UnityEngine.Networking;

public class CafeBazaar : Marketplace
{
    private CafeBazaarConfig config;
    private bool state = false;

    public override void Init(MarketplaceConfig _config)
    {
        config = _config as CafeBazaarConfig;
        marketplaceType = MarketplaceType.Cafebazaar;
        skuDetailsSucceedData.Clear();
    }

    public override void LoadMarketplace()
    {
        if (config == null)
            return;

        Debug.Log("CafeBazaar - LoadMarketplace");
        DestroyEvents();
        IABEventManager.billingSupportedEvent += BillingSupportedEvent;
        IABEventManager.billingNotSupportedEvent += BillingNotSupportedEvent;
        BazaarIAB.init(config.key);
        BazaarIAB.enableLogging(Debug.isDebugBuild);
    }

    protected override void SetEvents()
    {
        if (state == false)
            return;
        IABEventManager.querySkuDetailsSucceededEvent += SkuDetailsSucceededEvent;
        IABEventManager.purchaseSucceededEvent += PurchaseProductSucceededEvent;
        IABEventManager.consumePurchaseSucceededEvent += ConsumePurchaseSucceededEvent;
        IABEventManager.queryPurchasesSucceededEvent += QueryPurchasesSucceededEvent;
        IABEventManager.querySkuDetailsFailedEvent += SkuDetailsFailedEvent;
        IABEventManager.purchaseFailedEvent += PurchaseProductFailedEvent;
        IABEventManager.consumePurchaseFailedEvent += ConsumePurchaseFailedEvent;
        IABEventManager.queryPurchasesFailedEvent += QueryPurchasesFailedEvent;

        Debug.Log("CafeBazaar - SetEvents");
    }

    private void BillingNotSupportedEvent(string exception)
    {
        Debug.Log($"CafeBazaar - Init - GetError : {exception}");

        state = false;
    }

    private void BillingSupportedEvent()
    {
        Debug.Log("CafeBazaar - Init - Succeed");

        state = true;
        SetEvents();
    }

    public override void SkuDetails(string[] _SKU)
    {
        if (state == false)
        {
            SkuDetailsFailedEvent("State Is False");
            return;
        }

        Debug.Log("CafeBazaar - SkuDetails");

        bool callQuerySkuDetails = false;
        foreach (var sku in _SKU)
        {
            if (skuDetailsSucceedData.Exists(x => x.productId == sku))
                continue;
            callQuerySkuDetails = true;
            break;
        }

        if (callQuerySkuDetails)
        {
            BazaarIAB.querySkuDetails(_SKU);
        }
        else
        {
            SkuDetailsSucceed?.Invoke(skuDetailsSucceedData);
        }
    }


    private void SkuDetailsSucceededEvent(List<BazaarSkuInfo> _skuInfo)
    {
        Debug.Log("CafeBazaar - SkuDetailsSucceededEvent");
        foreach (var sku in _skuInfo)
        {
            SkuDetailsSucceedData skuDetails = new SkuDetailsSucceedData();
            skuDetails.productId = sku.ProductId;
            skuDetails.priceType = config.priceType;
            skuDetails.price = GetSkuPrice(sku.Price);
            int skuIndex = skuDetailsSucceedData.FindIndex(x => x.productId == sku.ProductId);
            if (skuIndex == -1)
            {
                skuDetailsSucceedData.Add(skuDetails);
            }
            else
            {
                skuDetailsSucceedData[skuIndex] = skuDetails;
            }
        }

        SkuDetailsSucceed?.Invoke(skuDetailsSucceedData);
    }

    private float GetSkuPrice(string _price)
    {
        return MarketplacePriceParser.Parse(_price, 10f);
    }

    private void SkuDetailsFailedEvent(string exception)
    {
        Debug.Log($"CafeBazaar - SkuDetailsFailedEvent : {exception}");

        SkuDetailsFailed?.Invoke(exception);
    }

    public override void PurchaseProduct(string _SKU)
    {
        if (state == false)
        {
            PurchaseProductFailedEvent("State Is False");
            return;
        }

        Debug.Log($"CafeBazaar - PurchaseProduct : {_SKU}");
        BazaarIAB.purchaseProduct(_SKU);
    }

    private void PurchaseProductSucceededEvent(BazaarPurchase _purchase)
    {
        Debug.Log("CafeBazaar - PurchaseProductSucceededEvent");

        PurchaseProductSucceedData purchaseSucceedData = new PurchaseProductSucceedData();
        purchaseSucceedData.productId = _purchase.ProductId;
        purchaseSucceedData.purchaseToken = _purchase.PurchaseToken;
        PurchaseProductSucceed?.Invoke(purchaseSucceedData);
    }

    private void PurchaseProductFailedEvent(string exception)
    {
        Debug.Log($"CafeBazaar - PurchaseProductFailedEvent : {exception}");
        PurchaseProductFailed?.Invoke(exception);
    }

    public override void ConsumeProduct(string _SKU)
    {
        if (state == false)
        {
            ConsumePurchaseFailedEvent("State Is False");
            return;
        }

        Debug.Log($"CafeBazaar - ConsumeProduct : {_SKU}");

        BazaarIAB.consumeProduct(_SKU);
    }

    private void ConsumePurchaseSucceededEvent(BazaarPurchase _consumePurchase)
    {
        Debug.Log("CafeBazaar - ConsumePurchaseSucceededEvent");
        ConsumeProductSucceedData consumeProductSucceedData = new ConsumeProductSucceedData();
        consumeProductSucceedData.productId = _consumePurchase.ProductId;
        consumeProductSucceedData.purchaseToken = _consumePurchase.PurchaseToken;
        ConsumeProductSucceed?.Invoke(consumeProductSucceedData);
    }

    private void ConsumePurchaseFailedEvent(string exception)
    {
        Debug.Log($"CafeBazaar - ConsumePurchaseFailedEvent : {exception}");

        ConsumeProductFailed?.Invoke(exception);
    }

    public override void QueryPurchases()
    {
        if (state == false)
        {
            QueryPurchasesFailedEvent("State Is False");
            return;
        }

        Debug.Log($"CafeBazaar - QueryPurchases");

        BazaarIAB.queryPurchases();
    }

    private void QueryPurchasesSucceededEvent(List<BazaarPurchase> _purchases)
    {
        Debug.Log("CafeBazaar - QueryPurchasesSucceededEvent");

        List<QueryPurchasesSucceedData> queryPurchasesSucceedData = new List<QueryPurchasesSucceedData>();
        foreach (var purchase in _purchases)
        {
            QueryPurchasesSucceedData queryPurchases = new QueryPurchasesSucceedData();
            queryPurchases.productId = purchase.ProductId;
            queryPurchases.purchaseToken = purchase.PurchaseToken;
            queryPurchasesSucceedData.Add(queryPurchases);
        }

        QueryPurchasesSucceed?.Invoke(queryPurchasesSucceedData);
    }

    private void QueryPurchasesFailedEvent(string exception)
    {
        Debug.Log($"CafeBazaar - QueryPurchasesFailedEvent : {exception}");

        QueryPurchasesFailed?.Invoke(exception);
    }

    public override void Rate()
    {
        Debug.Log($"CafeBazaar - Rate");

        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");

        AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");

        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_EDIT"));
        string url = "bazaar://details?id=" + config.PackageName;
        intentObject.Call<AndroidJavaObject>("setData", uriClass.CallStatic<AndroidJavaObject>("parse", url));
        intentObject.Call<AndroidJavaObject>("setPackage", "com.farsitel.bazaar");

        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
        currentActivity.Call("startActivity", intentObject);
    }

    public override void AppPage()
    {
        Debug.Log($"CafeBazaar - AppPage");

        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
        AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
        intentObject.Call<AndroidJavaObject>("setAction", intentObject.GetStatic<string>("ACTION_VIEW"));
        string url = "bazaar://details?id=" + config.PackageName;
        intentObject.Call<AndroidJavaObject>("setData", uriClass.CallStatic<AndroidJavaObject>("parse", url));
        intentObject.Call<AndroidJavaObject>("setPackage", "com.farsitel.bazaar");
        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
        currentActivity.Call("startActivity", intentObject);
    }

    protected void OnDestroy()
    {
        DestroyEvents();
    }

    protected override void DestroyEvents()
    {
        IABEventManager.billingSupportedEvent -= BillingSupportedEvent;
        IABEventManager.billingNotSupportedEvent -= BillingNotSupportedEvent;
        IABEventManager.querySkuDetailsSucceededEvent -= SkuDetailsSucceededEvent;
        IABEventManager.querySkuDetailsFailedEvent -= SkuDetailsFailedEvent;
        IABEventManager.purchaseSucceededEvent -= PurchaseProductSucceededEvent;
        IABEventManager.purchaseFailedEvent -= PurchaseProductFailedEvent;
        IABEventManager.consumePurchaseSucceededEvent -= ConsumePurchaseSucceededEvent;
        IABEventManager.consumePurchaseFailedEvent -= ConsumePurchaseFailedEvent;
        IABEventManager.queryPurchasesSucceededEvent -= QueryPurchasesSucceededEvent;
        IABEventManager.queryPurchasesFailedEvent -= QueryPurchasesFailedEvent;
    }

    public override void CheckForUpdate(Action<MarketUpdateResult> callback)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (marketUpdateResult != null)
        {
            callback?.Invoke(marketUpdateResult);
            return;
        }
        StartCoroutine(CheckForUpdateCoroutine(callback));
#else
        callback?.Invoke(null);
#endif
    }

    private IEnumerator CheckForUpdateCoroutine(Action<MarketUpdateResult> callback)
    {
        BazaarApplicationInfo appInfo = null;
        yield return GetPackageInfo(config.PackageName, result => { appInfo = result; });
        if (appInfo == null)
        {
            Debug.LogError("CafeBazaar - CheckForUpdateCoroutine - GetPackageInfo Failed");
            callback?.Invoke(null);
            yield break;
        }

        int currentVersionCode = AndroidVersionCode.Get();
        string currentVersionName = Application.version;
        int bazaarVersionCode = appInfo.versionCode;
        bool updateAvailable = bazaarVersionCode > currentVersionCode;
        marketUpdateResult = new MarketUpdateResult
        {
            updateAvailable = updateAvailable,
            currentVersionName = currentVersionName,
            currentVersionCode = currentVersionCode,
            marketVersionName = string.Empty,
            marketVersionCode = bazaarVersionCode,
        };

        Debug.Log($"CafeBazaar - CheckForUpdate\n"
                  + $"Current VersionName: {currentVersionName}\n"
                  + $"Current VersionCode: {currentVersionCode}\n"
                  + $"CafeBazaar VersionCode: {bazaarVersionCode}\n"
                  + $"Update Available: {updateAvailable}");

        callback?.Invoke(marketUpdateResult);
    }

    private IEnumerator GetPackageInfo(string packageName, Action<BazaarApplicationInfo> callback)
    {
        string url = "https://api.cafebazaar.ir/rest-v1/process/AppDownloadInfoRequest";

        string json = $@"{{
        ""properties"": {{
            ""language"": 2,
            ""clientVersionCode"": 1100301,
            ""androidClientInfo"": {{
                ""sdkVersion"": 28,
                ""cpu"": ""arm64-v8a,armeabi-v7a,armeabi""
            }},
            ""clientVersion"": ""11.3.1"",
            ""isKidsEnabled"": false
        }},
        ""singleRequest"": {{
            ""appDownloadInfoRequest"": {{
                ""downloadStatus"": 1,
                ""packageName"": ""{packageName}"",
                ""referrers"": []
            }}
        }}
        }}";

        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            yield return request.SendWebRequest();
            string response = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"CafeBazaar - GetPackageInfo - Package Info Failed\n" +
                               $"HTTP: {request.responseCode}\n" +
                               $"Error: {request.error}\n" +
                               $"Response:\n{response}");

                callback?.Invoke(null);
                yield break;
            }

            BazaarDownloadResponse data;

            try
            {
                data = JsonUtility.FromJson<BazaarDownloadResponse>(response);
            }
            catch (Exception e)
            {
                Debug.LogError($"CafeBazaar - GetPackageInfo - Response Parse Error:\n" +
                               $"{e}\n\n" +
                               $"Raw Response:\n{response}");

                callback?.Invoke(null);
                yield break;
            }

            if (data == null || data.singleReply == null || data.singleReply.appDownloadInfoReply == null)
            {
                Debug.LogError("CafeBazaar - GetPackageInfo - App Download Info not found");
                callback?.Invoke(null);
                yield break;
            }

            BazaarDownloadInfo info = data.singleReply.appDownloadInfoReply;
            callback?.Invoke(new BazaarApplicationInfo
            {
                versionCode = info.versionCode
            });
        }
    }
}

#endif