#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MyketPlugin;
using UnityEngine;
using UnityEngine.Networking;

public class MyKet : Marketplace
{
    private MyKetConfig config;
    private bool state = false;
    private string authToken;

    public override void Init(MarketplaceConfig _config)
    {
        config = _config as MyKetConfig;
        marketplaceType = MarketplaceType.Myket;
        skuDetailsSucceedData.Clear();
    }

    public override void LoadMarketplace()
    {
        if (config == null)
            return;

        Debug.Log("MyKet - LoadMarketplace");
        DestroyEvents();
        IABEventManager.billingSupportedEvent += BillingSupportedEvent;
        IABEventManager.billingNotSupportedEvent += BillingNotSupportedEvent;
        MyketIAB.init(config.key);
        MyketIAB.enableLogging(Debug.isDebugBuild);
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

        Debug.Log("MyKet - SetEvents");
    }

    private void BillingNotSupportedEvent(string exception)
    {
        Debug.Log($"MyKet - Init - GetError : {exception}");

        state = false;
    }

    private void BillingSupportedEvent()
    {
        Debug.Log("MyKet - Init - Succeed");

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

        Debug.Log("MyKet - SkuDetails");
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
            MyketIAB.querySkuDetails(_SKU);
        }
        else
        {
            SkuDetailsSucceed?.Invoke(skuDetailsSucceedData);
        }
    }

    private void SkuDetailsSucceededEvent(List<MyketSkuInfo> _skuInfo)
    {
        Debug.Log("MyKet - SkuDetailsSucceededEvent");
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
        return MarketplacePriceParser.Parse(_price, 1f);
    }

    private void SkuDetailsFailedEvent(string exception)
    {
        Debug.Log($"MyKet - SkuDetailsFailedEvent : {exception}");

        SkuDetailsFailed?.Invoke(exception);
    }

    public override void PurchaseProduct(string _SKU)
    {
        if (state == false)
        {
            PurchaseProductFailedEvent("State Is False");
            return;
        }

        Debug.Log($"MyKet - PurchaseProduct : {_SKU}");
        MyketIAB.purchaseProduct(_SKU);
    }

    private void PurchaseProductSucceededEvent(MyketPurchase _purchase)
    {
        Debug.Log("MyKet - PurchaseProductSucceededEvent");

        PurchaseProductSucceedData purchaseSucceedData = new PurchaseProductSucceedData();
        purchaseSucceedData.productId = _purchase.ProductId;
        purchaseSucceedData.purchaseToken = _purchase.PurchaseToken;
        PurchaseProductSucceed?.Invoke(purchaseSucceedData);
    }

    private void PurchaseProductFailedEvent(string exception)
    {
        Debug.Log($"MyKet - PurchaseProductFailedEvent : {exception}");

        PurchaseProductFailed?.Invoke(exception);
    }

    public override void ConsumeProduct(string _SKU)
    {
        if (state == false)
        {
            ConsumePurchaseFailedEvent("State Is False");
            return;
        }

        Debug.Log($"MyKet - ConsumeProduct : {_SKU}");

        MyketIAB.consumeProduct(_SKU);
    }

    private void ConsumePurchaseSucceededEvent(MyketPurchase _consumePurchase)
    {
        Debug.Log("MyKet - ConsumePurchaseSucceededEvent");

        ConsumeProductSucceedData consumeProductSucceedData = new ConsumeProductSucceedData();
        consumeProductSucceedData.productId = _consumePurchase.ProductId;
        consumeProductSucceedData.purchaseToken = _consumePurchase.PurchaseToken;
        ConsumeProductSucceed?.Invoke(consumeProductSucceedData);
    }

    private void ConsumePurchaseFailedEvent(string exception)
    {
        Debug.Log($"MyKet - ConsumePurchaseFailedEvent : {exception}");

        ConsumeProductFailed?.Invoke(exception);
    }

    public override void QueryPurchases()
    {
        if (state == false)
        {
            QueryPurchasesFailedEvent("State Is False");
            return;
        }

        Debug.Log($"MyKet - QueryPurchases");

        MyketIAB.queryPurchases();
    }

    private void QueryPurchasesSucceededEvent(List<MyketPurchase> _purchases)
    {
        Debug.Log("MyKet - QueryPurchasesSucceededEvent");

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
        Debug.Log($"MyKet - QueryPurchasesFailedEvent : {exception}");

        QueryPurchasesFailed?.Invoke(exception);
    }

    public override void Rate()
    {
        Debug.Log($"MyKet - Rate");
        string url = "myket://comment?id=" + config.PackageName;
        Application.OpenURL(url);
    }

    public override void AppPage()
    {
        Debug.Log($"MyKet - AppPage");
        string url = "myket://details?id=" + config.PackageName;
        Application.OpenURL(url);
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
        yield return GetAuthToken();
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogError("MyKet - CheckForUpdateCoroutine - GetAuthToken Failed");
            callback?.Invoke(null);
            yield break;
        }

        MyKetApplicationInfo appInfo = null;
        yield return GetPackageInfo(config.PackageName, result => { appInfo = result; });

        if (appInfo == null)
        {
            Debug.LogError("MyKet - CheckForUpdateCoroutine - GetPackageInfo Failed");
            callback?.Invoke(null);
            yield break;
        }

        int currentVersionCode = AndroidVersionCode.Get();
        string currentVersionName = Application.version;
        int myketVersionCode = appInfo.version?.code ?? 0;
        string myketVersionName = appInfo.version != null ? appInfo.version.name : string.Empty;
        bool updateAvailable = myketVersionCode > currentVersionCode;
        marketUpdateResult = new MarketUpdateResult
        {
            updateAvailable = updateAvailable,
            currentVersionName = currentVersionName,
            currentVersionCode = currentVersionCode,
            marketVersionName = myketVersionName,
            marketVersionCode = myketVersionCode,
        };

        Debug.Log($"MyKet - CheckForUpdate\n"
                  + $"Current VersionName: {currentVersionName}\n"
                  + $"Current VersionCode: {currentVersionCode}\n"
                  + $"MyKet VersionName: {myketVersionName}\n"
                  + $"MyKet VersionCode: {myketVersionCode}\n"
                  + $"Update Available: {updateAvailable}");

        callback?.Invoke(marketUpdateResult);
    }

    private IEnumerator GetAuthToken()
    {
        string url = "https://apiserver.myket.ir/v1/devices/authorize/";

        string json = @"{
        ""acId"": """",
        ""acKey"": """",
        ""adId"": ""82d8810f-e83c-46c0-8bf5-080a83d635c6"",
        ""andId"": ""82ee24cd7aad5173"",
        ""api"": ""17"",
        ""brand"": ""Samsung"",
        ""cpuAbis"": [""armeabi-v7a"", ""armeabi""],
        ""dens"": 2.625,
        ""deviceModel"": ""Samsung GnuSmas"",
        ""deviceName"": ""GnuSmas"",
        ""deviceType"": ""normal"",
        ""dsize"": ""300"",
        ""hsh"": ""46da699f048122ab9549ff061f8aa44aa8aae69b"",
        ""imei"": """",
        ""imsi"": """",
        ""manufacturer"": ""GnuSmas"",
        ""product"": ""GnuSmas_WWW"",
        ""supportedAbis"": [
            ""arm64-v8a"",
            ""armeabi-v7a"",
            ""armeabi""
        ],
        ""uuid"": ""b31f07f1-60fd-489b-9d14-66187834a131""
        }";

        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Myket-Version", "914");
            yield return request.SendWebRequest();
            string response = request.downloadHandler.text;
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"MyKet - GetAuthToken - Result Not Success");
                authToken = null;
                yield break;
            }

            MyKetAuthResponse authResponse;

            try
            {
                authResponse = JsonUtility.FromJson<MyKetAuthResponse>(response);
            }
            catch (Exception e)
            {
                Debug.LogError($"MyKet - GetAuthToken - Auth JSON Parse Error:\n{e}");
                authToken = null;
                yield break;
            }

            if (authResponse == null || string.IsNullOrEmpty(authResponse.token))
            {
                Debug.LogError("MyKet - GetAuthToken - Token not found in response");
                authToken = null;
                yield break;
            }

            authToken = authResponse.token;
        }
    }

    private IEnumerator GetPackageInfo(string packageName, Action<MyKetApplicationInfo> callback)
    {
        string url = $"https://apiserver.myket.ir/v2/applications/{packageName}/";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Myket-Version", "914");
            if (!string.IsNullOrEmpty(authToken))
            {
                request.SetRequestHeader("Authorization", authToken);
            }

            yield return request.SendWebRequest();

            if (request.responseCode == 401)
            {
                Debug.Log("MyKet - GetPackageInfo - Token expired - Requesting a new token...");
                yield return GetAuthToken();

                if (string.IsNullOrEmpty(authToken))
                {
                    callback?.Invoke(null);
                    yield break;
                }

                yield return GetPackageInfo(packageName, callback);
                yield break;
            }

            string response = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"MyKet - GetPackageInfo - Package Info Failed\n" + $"HTTP: {request.responseCode}\n" +
                               $"Error: {request.error}");
                callback?.Invoke(null);
                yield break;
            }

            MyKetApplicationInfo data;

            try
            {
                data = JsonUtility.FromJson<MyKetApplicationInfo>(response);
            }
            catch (Exception e)
            {
                Debug.LogError($"MyKet - GetPackageInfo - Response Parse Error:\n" + $"{e}\n\n" +
                               $"Raw Response:\n{response}");
                callback?.Invoke(null);
                yield break;
            }

            if (data == null)
            {
                Debug.LogError("MyKet - GetPackageInfo - Empty response");
                callback?.Invoke(null);
                yield break;
            }

            if (data.version == null)
            {
                Debug.LogWarning("MyKet - GetPackageInfo - version object not found");
            }

            callback?.Invoke(data);
        }
    }
}

#endif