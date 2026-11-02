using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if ONEUP_APPUPDATE
using Google.Play.AppUpdate;
using Google.Play.Common;
#endif
using UnityEngine;
using UnityEngine.Networking;

public class ZarinPal : Marketplace
{
    private const float PriceDivisor = 10f;

    private ZarinPalConfig config;
    private bool state = false;
    private string purchasedSku;

    protected override void Awake()
    {
        base.Awake();
        Application.deepLinkActivated += OnDeepLink;
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            OnDeepLink(Application.absoluteURL);
        }
    }

    public override void Init(MarketplaceConfig _config)
    {
        config = _config as ZarinPalConfig;
        marketplaceType = MarketplaceType.Zarinpal;
        skuDetailsSucceedData.Clear();
    }

    public override void LoadMarketplace()
    {
        if (config == null)
            return;

        Debug.Log("ZarinPal - LoadMarketplace");
        BillingSupportedEvent();
    }

    protected override void SetEvents()
    {
        if (state == false)
            return;

        Debug.Log("ZarinPal - SetEvents");
    }

    private void BillingNotSupportedEvent(string exception)
    {
        Debug.Log($"ZarinPal - Init - GetError : {exception}");

        state = false;
    }

    private void BillingSupportedEvent()
    {
        Debug.Log("ZarinPal - Init - Succeed");

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

        Debug.Log("ZarinPal - SkuDetails");
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
            SkuDetailsSucceededEvent(_SKU.ToList());
        }
        else
        {
            SkuDetailsSucceed?.Invoke(skuDetailsSucceedData);
        }
    }

    private void SkuDetailsSucceededEvent(List<string> _SKU)
    {
        Debug.Log("ZarinPal - SkuDetailsSucceededEvent");
        foreach (var sku in _SKU)
        {
            SkuDetailsSucceedData skuDetails = new SkuDetailsSucceedData();
            skuDetails.productId = sku;
            skuDetails.priceType = config.priceType;
            skuDetails.price = (config.skus.Find(x => x.sku == sku)?.price ?? 0f) / PriceDivisor;
            int skuIndex = skuDetailsSucceedData.FindIndex(x => x.productId == sku);
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

    private void SkuDetailsFailedEvent(string exception)
    {
        Debug.Log($"ZarinPal - SkuDetailsFailedEvent : {exception}");

        SkuDetailsFailed?.Invoke(exception);
    }

    public override void PurchaseProduct(string _SKU)
    {
        if (state == false)
        {
            PurchaseProductFailedEvent("State Is False");
            return;
        }

        Debug.Log($"ZarinPal - PurchaseProduct : {_SKU}");
        ZarinpalSKU currentSKU = config.skus.Find(x => x.sku == _SKU);
        if (currentSKU == null)
        {
            purchasedSku = string.Empty;
            PurchaseProductFailedEvent("SKU Not Found");
            return;
        }

        if (string.IsNullOrEmpty(config.paymentStartUrl) || string.IsNullOrEmpty(config.callbackScheme))
        {
            purchasedSku = string.Empty;
            PurchaseProductFailedEvent("Web Store Config Missing");
            return;
        }

        purchasedSku = _SKU;
        ZarinpalPurchaseData purchaseData = new ZarinpalPurchaseData()
        {
            displayname = currentSKU.displayName,
            price = currentSKU.price,
            userid = MarketplaceManager.Instance.GetUserId(),
            appversion = Application.version,
            platform = Application.platform == RuntimePlatform.Android ? "android" : "ios",
            gamepkg = config.PackageName,
            callbackurl = $"{config.callbackScheme}://{config.callbackHost}/return",
            autoverify = "0",
            merchant_id = config.key
        };
        StartCoroutine(PurchaseProduct(purchaseData));
    }

    private IEnumerator PurchaseProduct(ZarinpalPurchaseData purchaseData)
    {
        string url = ZarinPalRequest.BuildStartUrl(config.paymentStartUrl, purchaseData);
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Zarinpal - PurchaseProduct - Response - URL: " + request.url);
                Application.OpenURL(request.url);
            }
            else
            {
                purchasedSku = string.Empty;
                Debug.Log("Zarinpal - PurchaseProduct - Response - Error: " + request.error);
                PurchaseProductFailedEvent("Response Error:" + request.error);
            }
        }
    }

    private void OnDeepLink(string url)
    {
        if (string.IsNullOrEmpty(purchasedSku))
            return;

        if (string.IsNullOrEmpty(url))
        {
            purchasedSku = string.Empty;
            PurchaseProductFailedEvent("Cancel Purchase");
            return;
        }

        ZarinPalPaymentResult result =
            ZarinPalRequest.ReadPayment(url, config.callbackScheme, config.callbackHost, out string paymentToken);
        if (result == ZarinPalPaymentResult.Ignored)
            return;

        string sku = purchasedSku;
        purchasedSku = string.Empty;
        if (result == ZarinPalPaymentResult.Paid)
        {
            PurchaseProductSucceededEvent(new ZarinpalPurchase { ProductID = sku, Authority = paymentToken });
        }
        else
        {
            PurchaseProductFailedEvent("Response Error");
        }
    }

    private void PurchaseProductSucceededEvent(ZarinpalPurchase purchase)
    {
        Debug.Log("ZarinPal - PurchaseProductSucceededEvent");

        PurchaseProductSucceedData purchaseSucceedData = new PurchaseProductSucceedData();
        purchaseSucceedData.productId = purchase.ProductID;
        purchaseSucceedData.purchaseToken = purchase.Authority;
        PurchaseProductSucceed?.Invoke(purchaseSucceedData);
    }

    private void PurchaseProductFailedEvent(string exception)
    {
        Debug.Log($"ZarinPal - PurchaseProductFailedEvent - {exception}");

        PurchaseProductFailed?.Invoke(exception);
    }

    public override void ConsumeProduct(string _SKU)
    {
        if (state == false)
        {
            ConsumePurchaseFailedEvent("State Is False");
            return;
        }

        Debug.Log($"ZarinPal - ConsumeProduct : {_SKU}");
        ConsumePurchaseSucceededEvent();
    }

    private void ConsumePurchaseSucceededEvent()
    {
        Debug.Log("ZarinPal - ConsumePurchaseSucceededEvent");
        ConsumeProductSucceed?.Invoke(null);
    }

    private void ConsumePurchaseFailedEvent(string exception)
    {
        Debug.Log($"ZarinPal - ConsumePurchaseFailedEvent : {exception}");

        ConsumeProductFailed?.Invoke(exception);
    }

    public override void QueryPurchases()
    {
        if (state == false)
        {
            QueryPurchasesFailedEvent("State Is False");
            return;
        }

        Debug.Log($"ZarinPal - QueryPurchases");
        QueryPurchasesSucceededEvent();
    }

    private void QueryPurchasesSucceededEvent()
    {
        Debug.Log("ZarinPal - QueryPurchasesSucceededEvent");

        QueryPurchasesSucceed?.Invoke(new List<QueryPurchasesSucceedData>());
    }

    private void QueryPurchasesFailedEvent(string exception)
    {
        Debug.Log($"ZarinPal - QueryPurchasesFailedEvent : {exception}");

        QueryPurchasesFailed?.Invoke(exception);
    }

    public void GetPaymentPending(Action<ZarinpalPendingResponse> callback)
    {
        if (state == false)
        {
            callback?.Invoke(null);
            return;
        }

        ZarinpalPendingData zarinpalPendingData = new ZarinpalPendingData()
        {
            userid = MarketplaceManager.Instance.GetUserId(),
            gamepkg = config.PackageName
        };
        StartCoroutine(PendingPayment(zarinpalPendingData, callback));
    }

    private IEnumerator PendingPayment(ZarinpalPendingData pendingData, Action<ZarinpalPendingResponse> callback)
    {
        string url = ZarinPalRequest.BuildPendingUrl(config.paymentPendingUrl, pendingData.userid, pendingData.gamepkg);
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Zarinpal - PendingPayment - Response - Success: " + request.downloadHandler.text);
                ZarinpalPendingResponse zarinpalPendingResponse =
                    JsonUtility.FromJson<ZarinpalPendingResponse>(request.downloadHandler.text);
                if (zarinpalPendingResponse != null && zarinpalPendingResponse.status &&
                    zarinpalPendingResponse.data.Length > 0)
                {
                    callback?.Invoke(zarinpalPendingResponse);
                }
                else
                {
                    Debug.Log("Zarinpal - PendingPayment - Response - null");
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.Log("Zarinpal - PendingPayment - Response - Error: " + request.error);
                callback?.Invoke(null);
            }
        }
    }

    public override void Rate()
    {
        Debug.Log($"ZarinPal - Rate");
        string packageName = Application.identifier;
        try
        {
            using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
            using (AndroidJavaObject intent =
                   new AndroidJavaObject("android.content.Intent", intentClass.GetStatic<string>("ACTION_VIEW")))
            using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
            {
                AndroidJavaObject uri =
                    uriClass.CallStatic<AndroidJavaObject>("parse", $"market://details?id={packageName}");
                intent.Call<AndroidJavaObject>("setData", uri);
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    activity.Call("startActivity", intent);
                }
            }
        }
        catch (Exception)
        {
            Application.OpenURL($"https://play.google.com/store/apps/details?id={packageName}");
        }
    }

    public override void AppPage()
    {
        Debug.Log($"ZarinPal - AppPage");
        string packageName = Application.identifier;
        try
        {
            using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
            using (AndroidJavaObject intent =
                   new AndroidJavaObject("android.content.Intent", intentClass.GetStatic<string>("ACTION_VIEW")))
            using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
            {
                AndroidJavaObject uri =
                    uriClass.CallStatic<AndroidJavaObject>("parse", $"market://details?id={packageName}");
                intent.Call<AndroidJavaObject>("setData", uri);
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    activity.Call("startActivity", intent);
                }
            }
        }
        catch (Exception)
        {
            Application.OpenURL($"https://play.google.com/store/apps/details?id={packageName}");
        }
    }

    protected void OnDestroy()
    {
        DestroyEvents();
    }

    protected override void DestroyEvents()
    {
        Application.deepLinkActivated -= OnDeepLink;
    }

    public override void CheckForUpdate(Action<MarketUpdateResult> callback)
    {
#if UNITY_ANDROID && !UNITY_EDITOR && ONEUP_APPUPDATE
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

#if ONEUP_APPUPDATE
    private IEnumerator CheckForUpdateCoroutine(Action<MarketUpdateResult> callback)
    {
        AppUpdateManager appUpdateManager = new AppUpdateManager();
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation =
            appUpdateManager.GetAppUpdateInfo();

        yield return appUpdateInfoOperation;
        if (!appUpdateInfoOperation.IsSuccessful)
        {
            Debug.LogError($"ZarinPal Update Check Failed: " + $"{appUpdateInfoOperation.Error}");
            callback?.Invoke(null);
            yield break;
        }

        AppUpdateInfo appUpdateInfo = appUpdateInfoOperation.GetResult();
        bool updateAvailable = appUpdateInfo.UpdateAvailability == UpdateAvailability.UpdateAvailable;
        int currentVersionCode = AndroidVersionCode.Get();
        string currentVersionName = Application.version;
        marketUpdateResult = new MarketUpdateResult
        {
            updateAvailable = updateAvailable,
            currentVersionName = currentVersionName,
            currentVersionCode = currentVersionCode,
            marketVersionName = string.Empty,
            marketVersionCode = 0
        };

        Debug.Log(
            $"GooglePlay - CheckForUpdate\n" +
            $"Current VersionName: {currentVersionName}\n" +
            $"Current VersionCode: {currentVersionCode}\n" +
            $"Update Availability: {appUpdateInfo.UpdateAvailability}\n" +
            $"Update Available: {updateAvailable}");

        callback?.Invoke(marketUpdateResult);
    }
#endif
}
