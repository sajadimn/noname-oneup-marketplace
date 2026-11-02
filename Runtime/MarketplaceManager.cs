using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketplaceManager : MonoSingleton<MarketplaceManager>
{
    private const string PurchaseFailed = "PURCHASE_FAILED";
    private const string ConsumeFailed = "CONSUME_FAILED";
    private const string PaymentNotAvailable = "PAYMENT_NOT_AVAILABLE";
    private const float DefaultTimeOut = 5f;

    public event Action<PurchaseProductSucceedData> PurchaseRecovered;

    protected Marketplace marketplace = null;
    protected MarketplaceConfig marketplaceConfig = null;
    protected MarketplaceType marketPlaceType;
    protected MarketplaceStoreData marketPlaceData;

    private MarketplaceData config;
    private Canvas loader;
    private readonly MarketplaceFlow flow = new MarketplaceFlow();
    private readonly MarketplaceSkuCache skuCache = new MarketplaceSkuCache();
    private Func<string> userIdProvider;
    private Coroutine timeoutCoroutine;
    private bool waitingForMarket = false;
    private Action<List<QueryPurchasesSucceedData>> restoreOnDone;
    private Action<string> restoreOnError;

    public bool HasLoader => loader != null;

    private List<MarketplaceStoreData> Marketplaces => config != null ? config.marketplaces : new List<MarketplaceStoreData>();

    public void Init(Func<string> getUserId = null)
    {
        userIdProvider = getUserId ?? userIdProvider;
        if (marketplace != null)
            return;

        config = Resources.Load<MarketplaceData>(MarketplaceData.FileName);
        loader = MarketplaceLoader.Create(config != null ? config.loaderPrefab : null, transform);
        StartMarketplace(GetResolvedType());
        SetMarketplaceEvents();
        marketplace.LoadMarketplace();
    }

    public string GetUserId()
    {
        return userIdProvider?.Invoke() ?? string.Empty;
    }

    private static MarketplaceType GetBuiltType()
    {
#if IN_GAME
        return MarketplaceType.InGame;
#elif CAFE_BAZAAR
        return MarketplaceType.Cafebazaar;
#elif MY_KET
        return MarketplaceType.Myket;
#elif ZARIN_PAL
        return MarketplaceType.Zarinpal;
#elif GOOGLE_PLAY
        return MarketplaceType.GooglePlay;
#elif APP_STORE
        return MarketplaceType.AppStore;
#elif TESTER
        return MarketplaceType.Tester;
#else
        return MarketplaceType.InGame;
#endif
    }

    private static MarketplaceType GetResolvedType()
    {
        MarketplaceType built = GetBuiltType();
        bool isIran = built == MarketplaceType.GooglePlay && RegionManager.Instance.GetUserRegion() == UserRegion.Iran;
#if UNITY_ANDROID
        const bool isAndroid = true;
#else
        const bool isAndroid = false;
#endif
#if UNITY_IOS
        const bool isIos = true;
#else
        const bool isIos = false;
#endif
        MarketplaceType resolved = MarketplaceResolver.Resolve(built, isIran, isAndroid, isIos);
        if (resolved == MarketplaceType.InGame && built != MarketplaceType.InGame)
            Debug.LogError($"MarketplaceManager - {built} cannot run on this platform, using InGame");
        return resolved;
    }

    private void StartMarketplace(MarketplaceType type)
    {
        marketPlaceData = Marketplaces.Find(market => market.type == type)
                          ?? Marketplaces.Find(market => market.type == MarketplaceType.InGame)
                          ?? new MarketplaceStoreData { type = type };
        marketplaceConfig = marketPlaceData.marketplaceConfig;
        marketPlaceType = type == MarketplaceType.Tester ? MarketplaceType.Cafebazaar : type;
        marketplace = CreateStore(type);
        marketplace.Init(marketplaceConfig);
    }

    private static Marketplace CreateStore(MarketplaceType type)
    {
        switch (type)
        {
#if UNITY_ANDROID
            case MarketplaceType.Cafebazaar:
            case MarketplaceType.Tester:
                return new GameObject("CafeBazaar").AddComponent<CafeBazaar>();
            case MarketplaceType.Myket:
                return new GameObject("MyKet").AddComponent<MyKet>();
#endif
#if UNITY_ANDROID && ONEUP_PURCHASING && ONEUP_APPUPDATE
            case MarketplaceType.GooglePlay:
                return new GameObject("GooglePlay").AddComponent<GooglePlay>();
#endif
            case MarketplaceType.Zarinpal:
                return new GameObject("ZarinPal").AddComponent<ZarinPal>();
            default:
                return new GameObject("InGame").AddComponent<InGameMarketplace>();
        }
    }

    private void SetMarketplaceEvents()
    {
        marketplace.SkuDetailsSucceed += SkuDetailsSucceed;
        marketplace.SkuDetailsFailed += SkuDetailsFailed;
        marketplace.PurchaseProductSucceed += PurchaseProductSucceed;
        marketplace.PurchaseProductFailed += PurchaseProductFailed;
        marketplace.ConsumeProductSucceed += ConsumeProductSucceed;
        marketplace.ConsumeProductFailed += ConsumeProductFailed;
        marketplace.QueryPurchasesSucceed += QueryPurchasesSucceed;
        marketplace.QueryPurchasesFailed += QueryPurchasesFailed;
    }

    public Marketplace GetCurrentMarketplace()
    {
        return marketplace;
    }

    public MarketplaceConfig GetCurrentMarketplaceConfig()
    {
        return marketplaceConfig;
    }

    public MarketplaceType GetCurrentMarketPlaceType()
    {
        return marketPlaceType;
    }

    public MarketplaceStoreData GetCurrentMarketPlaceData()
    {
        return marketPlaceData;
    }

    protected override void OnDestroySingleton()
    {
        if (marketplace == null)
            return;

        marketplace.SkuDetailsSucceed -= SkuDetailsSucceed;
        marketplace.SkuDetailsFailed -= SkuDetailsFailed;
        marketplace.PurchaseProductSucceed -= PurchaseProductSucceed;
        marketplace.PurchaseProductFailed -= PurchaseProductFailed;
        marketplace.ConsumeProductSucceed -= ConsumeProductSucceed;
        marketplace.ConsumeProductFailed -= ConsumeProductFailed;
        marketplace.QueryPurchasesSucceed -= QueryPurchasesSucceed;
        marketplace.QueryPurchasesFailed -= QueryPurchasesFailed;
    }

    #region GetSKU

    public void GetSKUDetail(string sku, Action<SkuDetailsSucceedData> callBack, bool forceUpdate = false)
    {
        SendSKUDetailToMarket(skuCache.Request(sku, callBack, forceUpdate));
    }

    public void GetSKUDetail(string[] skus, Action<SkuDetailsSucceedData> callBack, bool forceUpdate = false)
    {
        foreach (string sku in skus)
            GetSKUDetail(sku, callBack, forceUpdate);
    }

    public void GetSKUDetail(List<string> skus, Action<List<SkuDetailsSucceedData>> callBack, bool forceUpdate = false)
    {
        SendSKUDetailToMarket(skuCache.Request(skus, callBack, forceUpdate));
    }

    private void SendSKUDetailToMarket(string[] skus)
    {
        if (skus.Length == 0)
            return;

        if (marketplace != null)
            marketplace.SkuDetails(skus);
        else
            skuCache.Failed();
    }

    private void SkuDetailsSucceed(List<SkuDetailsSucceedData> skuDetails)
    {
        skuCache.Succeed(skuDetails);
    }

    private void SkuDetailsFailed(string exception)
    {
        skuCache.Failed();
    }

    public void RemoveGetSkuEvent(List<string> skus, Action<SkuDetailsSucceedData> callBack)
    {
        if (skus == null)
            return;
        foreach (string sku in skus)
            RemoveGetSkuEvent(sku, callBack);
    }

    public void RemoveGetSkuEvent(string[] skus, Action<SkuDetailsSucceedData> callBack)
    {
        if (skus == null)
            return;
        foreach (string sku in skus)
            RemoveGetSkuEvent(sku, callBack);
    }

    public void RemoveGetSkuEvent(string sku, Action<SkuDetailsSucceedData> callBack)
    {
        skuCache.RemoveCallback(sku, callBack);
    }

    #endregion

    #region Purchase

    public void PurchaseProduct(string sku, bool hideLoadingAfterPurchase,
        Action<PurchaseProductSucceedData> onDone, Action<string> onError, Action<string> notAvailable)
    {
        MarketplaceOperation operation = new MarketplaceOperation
        {
            sku = sku,
            hideLoading = hideLoadingAfterPurchase,
            onPurchaseDone = onDone,
            onError = onError,
            onNotAvailable = notAvailable
        };

        if (marketplace == null || !flow.TryBegin(MarketplaceFlowState.Purchasing, operation))
        {
            onError?.Invoke(PurchaseFailed);
            return;
        }

        ShowLoader();
        StartTimeout();
        marketplace.PurchaseProduct(sku);
    }

    private void PurchaseProductSucceed(PurchaseProductSucceedData purchaseProductSucceedData)
    {
        MarketplaceOperation operation =
            flow.Finish(MarketplaceFlowState.Purchasing, purchaseProductSucceedData?.productId);
        if (operation == null)
        {
            PurchaseRecovered?.Invoke(purchaseProductSucceedData);
            return;
        }

        StopTimeout();
        if (operation.hideLoading)
            HideLoader();
        operation.onPurchaseDone?.Invoke(purchaseProductSucceedData);
    }

    private void PurchaseProductFailed(string exception)
    {
        MarketplaceOperation operation = flow.Finish(MarketplaceFlowState.Purchasing, null);
        if (operation == null)
            return;

        StopTimeout();
        HideLoader();
        operation.onError?.Invoke(exception);
    }

    #endregion

    #region Consume

    public void SendConsume(string sku, bool hideLoading, Action<ConsumeProductSucceedData> onDone,
        Action<string> onError, Action<string> notAvailable)
    {
        MarketplaceOperation operation = new MarketplaceOperation
        {
            sku = sku,
            hideLoading = hideLoading,
            onConsumeDone = onDone,
            onError = onError,
            onNotAvailable = notAvailable
        };

        if (marketplace == null || !flow.TryBegin(MarketplaceFlowState.Consuming, operation))
        {
            HideLoader();
            onError?.Invoke(ConsumeFailed);
            return;
        }

        StartTimeout();
        marketplace.ConsumeProduct(sku);
        if (hideLoading)
            HideLoader();
    }

    private void ConsumeProductSucceed(ConsumeProductSucceedData consumeSuccess)
    {
        MarketplaceOperation operation = flow.Finish(MarketplaceFlowState.Consuming, consumeSuccess?.productId);
        if (operation == null)
            return;

        StopTimeout();
        HideLoader();
        operation.onConsumeDone?.Invoke(consumeSuccess);
    }

    private void ConsumeProductFailed(string consumeFail)
    {
        MarketplaceOperation operation = flow.Finish(MarketplaceFlowState.Consuming, null);
        if (operation == null)
            return;

        StopTimeout();
        HideLoader();
        operation.onError?.Invoke(consumeFail);
    }

    #endregion

    #region Restore

    public void RestorePurchases(Action<List<QueryPurchasesSucceedData>> onDone, Action<string> onError)
    {
        if (marketplace == null)
        {
            onError?.Invoke(PaymentNotAvailable);
            return;
        }

        restoreOnDone += onDone;
        restoreOnError += onError;
        marketplace.QueryPurchases();
    }

    private void QueryPurchasesSucceed(List<QueryPurchasesSucceedData> purchases)
    {
        Action<List<QueryPurchasesSucceedData>> done = restoreOnDone;
        ResetRestore();
        done?.Invoke(purchases ?? new List<QueryPurchasesSucceedData>());
    }

    private void QueryPurchasesFailed(string exception)
    {
        Action<string> error = restoreOnError;
        ResetRestore();
        error?.Invoke(exception);
    }

    private void ResetRestore()
    {
        restoreOnDone = null;
        restoreOnError = null;
    }

    #endregion

    #region Loader

    private void ShowLoader()
    {
        if (loader == null)
            return;

        loader.worldCamera = Camera.main;
        loader.gameObject.SetActive(true);
    }

    private void HideLoader()
    {
        if (loader != null)
            loader.gameObject.SetActive(false);
    }

    private void StartTimeout()
    {
        waitingForMarket = true;
        RestartTimeout();
    }

    private void RestartTimeout()
    {
        if (timeoutCoroutine != null)
            StopCoroutine(timeoutCoroutine);

        timeoutCoroutine = StartCoroutine(MarketTimeout());
    }

    private void StopTimeout()
    {
        waitingForMarket = false;
        if (timeoutCoroutine != null)
            StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = null;
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause && waitingForMarket)
            RestartTimeout();
    }

    private IEnumerator MarketTimeout()
    {
        yield return new WaitForSecondsRealtime(config != null ? config.showLoaderTimeOut : DefaultTimeOut);
        MarketplaceOperation operation = flow.Finish(flow.State, null);
        StopTimeout();
        HideLoader();
        operation?.onNotAvailable?.Invoke(PaymentNotAvailable);
    }

    #endregion
}
