#if ONEUP_PURCHASING && ONEUP_APPUPDATE
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Google.Play.AppUpdate;
using Google.Play.Common;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

public class GooglePlay : Marketplace, IStoreListener
{
    private GooglePlayConfig config;
    private bool state = false;
    private IStoreController storeController;
    private Product product = null;
    public string environment = "production";


    public override void Init(MarketplaceConfig _config)
    {
        Debug.Log("GooglePlay - Init");
        config = _config as GooglePlayConfig;
        if (config == null)
            return;
        marketplaceType = MarketplaceType.GooglePlay;
        skuDetailsSucceedData.Clear();
        InitGamingServices();
    }

    public override void LoadMarketplace()
    {
        if (config == null)
            return;

        Debug.Log("GooglePlay - LoadMarketplace");
    }

    private async void InitGamingServices()
    {
        try
        {
            var options = new InitializationOptions()
                .SetEnvironmentName(environment);

            await UnityServices.InitializeAsync(options);
            Debug.Log("GooglePlay - InitGamingServices - Succeed");
        }
        catch (Exception exception)
        {
            Debug.Log(
                "GooglePlay - InitGamingServices - GetError - error: An error occurred during services initialization.");
        }
    }

    private void InitUnityPurchasing(string[] _SKU)
    {
#if UNITY_ANDROID
        ConfigurationBuilder builder =
            ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.GooglePlay));
#endif

#if UNITY_IOS
ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.AppleAppStore));
#endif

        builder.Configure<IGooglePlayConfiguration>().SetServiceDisconnectAtInitializeListener(() =>
        {
            Debug.Log(
                "GooglePlay - InitUnityPurchasing - GetError - error: Unable to connect to the Google Play Billing service. User may not have a Google account on their device.");
        });
        foreach (var sku in _SKU)
        {
            builder.AddProduct(sku, ProductType.Consumable);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    protected override void SetEvents()
    {
        if (state == false)
            return;
        Debug.Log("GooglePlay - SetEvents");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("GooglePlay - Init - Succeed");
        state = true;
        storeController = controller;
        SetEvents();
        SkuDetailsSucceededEvent();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log($"GooglePlay - Init - GetError - error: {error}");
        state = false;
        SkuDetailsFailedEvent(error.ToString());
    }

    public void OnInitializeFailed(InitializationFailureReason error, string? message = null)
    {
        Debug.Log($"GooglePlay - Init - GetError - error: {error} - message: {message}");
        state = false;
        SkuDetailsFailedEvent($"{error} {message}");
    }

    public override void SkuDetails(string[] _SKU)
    {
        Debug.Log("GooglePlay - SkuDetails");
        if (storeController == null || storeController.products.all.Length == 0 ||
            _SKU.ToList().Exists(x => !skuDetailsSucceedData.Exists(y => y.productId == x)))
        {
            InitUnityPurchasing(_SKU);
        }
        else
        {
            SkuDetailsSucceededEvent();
        }
    }

    private void SkuDetailsSucceededEvent()
    {
        Debug.Log("GooglePlay - SkuDetailsSucceededEvent");
        foreach (var _product in storeController.products.all)
        {
            if (!skuDetailsSucceedData.Exists(x => x.productId == _product.definition.id))
            {
                SkuDetailsSucceedData skuDetails = new SkuDetailsSucceedData();
                skuDetails.productId = _product.definition.id;
                skuDetails.priceType = _product.metadata.localizedPriceString;
                skuDetails.price = GetSkuPrice(_product.metadata.localizedPrice);
                skuDetailsSucceedData.Add(skuDetails);
                config.currencyCode = _product.metadata.isoCurrencyCode;
            }
        }

        SkuDetailsSucceed?.Invoke(skuDetailsSucceedData);
    }

    private float GetSkuPrice(decimal _price)
    {
        return (float)_price;
    }

    private void SkuDetailsFailedEvent(string exception)
    {
        Debug.Log($"GooglePlay - SkuDetailsFailedEvent : {exception}");

        SkuDetailsFailed?.Invoke(exception);
    }

    public override void PurchaseProduct(string _SKU)
    {
        if (state == false)
        {
            PurchaseProductFailed?.Invoke("State Is False");
            return;
        }

        Debug.Log($"GooglePlay - PurchaseProduct : {_SKU}");
        storeController.InitiatePurchase(_SKU);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log($"GooglePlay - ProcessPurchase : {args.purchasedProduct.definition.id}");
        product = args.purchasedProduct;
        OnPurchaseSucceed(product);
        return PurchaseProcessingResult.Pending;
    }

    private void OnPurchaseSucceed(Product _product)
    {
        Debug.Log("GooglePlay - OnPurchaseSucceed");
        PurchaseProductSucceedData purchaseSucceedData = new PurchaseProductSucceedData();
        purchaseSucceedData.productId = _product.definition.id;
        purchaseSucceedData.purchaseToken = _product.transactionID;
        PurchaseProductSucceed?.Invoke(purchaseSucceedData);
    }

    public void OnPurchaseFailed(Product _product, PurchaseFailureReason _failureReason)
    {
        string exception =
            $"GooglePlay - OnPurchaseFailed - PurchaseFailureReason: {_failureReason}";
        Debug.Log(exception);

        if (_failureReason == PurchaseFailureReason.ExistingPurchasePending ||
            _failureReason == PurchaseFailureReason.DuplicateTransaction)
        {
            product = _product;
            OnPurchaseSucceed(_product);
            return;
        }

        PurchaseProductFailed?.Invoke(exception);
    }

    public override void ConsumeProduct(string _SKU)
    {
        if (state == false || product == null)
        {
            ConsumePurchaseFailedEvent("State Is False");
            return;
        }

        Debug.Log($"GooglePlay - ConsumeProduct : {_SKU}");
        storeController.ConfirmPendingPurchase(product);
        ConsumePurchaseSucceededEvent(product);
    }

    private void ConsumePurchaseSucceededEvent(Product _product)
    {
        Debug.Log("GooglePlay - ConsumePurchaseSucceededEvent");
        ConsumeProductSucceedData consumeProductSucceedData = new ConsumeProductSucceedData();
        consumeProductSucceedData.productId = _product.definition.id;
        consumeProductSucceedData.purchaseToken = _product.transactionID;
        ConsumeProductSucceed?.Invoke(consumeProductSucceedData);
    }

    private void ConsumePurchaseFailedEvent(string exception)
    {
        Debug.Log($"GooglePlay - ConsumePurchaseFailedEvent : {exception}");
        ConsumeProductFailed?.Invoke(exception);
    }

    public override void QueryPurchases()
    {
        if (state == false)
        {
            QueryPurchasesFailedEvent("State Is False");
            return;
        }

        Debug.Log($"GooglePlay - QueryPurchases");
        QueryPurchasesSucceededEvent();
    }

    private void QueryPurchasesSucceededEvent()
    {
        Debug.Log("GooglePlay - QueryPurchasesSucceededEvent");
        QueryPurchasesSucceed?.Invoke(new System.Collections.Generic.List<QueryPurchasesSucceedData>());
    }

    private void QueryPurchasesFailedEvent(string exception)
    {
        Debug.Log($"GooglePlay - QueryPurchasesFailedEvent : {exception}");

        QueryPurchasesFailed?.Invoke(exception);
    }

    public override void Rate()
    {
        Debug.Log($"GooglePlay - Rate");
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
        Debug.Log($"GooglePlay - AppPage");
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
        AppUpdateManager appUpdateManager = new AppUpdateManager();
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation =
            appUpdateManager.GetAppUpdateInfo();

        yield return appUpdateInfoOperation;
        if (!appUpdateInfoOperation.IsSuccessful)
        {
            Debug.LogError($"Google Play Update Check Failed: " + $"{appUpdateInfoOperation.Error}");
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
}
#endif
