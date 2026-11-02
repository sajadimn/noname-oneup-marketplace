using System;
using System.Collections.Generic;

public abstract class Marketplace : MonoSingleton<Marketplace>
{
    protected MarketplaceType marketplaceType;
    protected MarketUpdateResult marketUpdateResult = null;

    public MarketplaceType GetMarketplaceType()
    {
        return marketplaceType;
    }

    public abstract void Init(MarketplaceConfig _config);
    public abstract void LoadMarketplace();
    public abstract void Rate();
    public abstract void AppPage();
    protected abstract void SetEvents();
    protected abstract void DestroyEvents();
    public abstract void SkuDetails(string[] _SKU);
    public Action<List<SkuDetailsSucceedData>> SkuDetailsSucceed;
    public Action<string> SkuDetailsFailed;
    public abstract void PurchaseProduct(string _SKU);
    public Action<PurchaseProductSucceedData> PurchaseProductSucceed;
    public Action<string> PurchaseProductFailed;
    public abstract void ConsumeProduct(string _SKU);
    public Action<ConsumeProductSucceedData> ConsumeProductSucceed;
    public Action<string> ConsumeProductFailed;

    public abstract void QueryPurchases();
    public Action<List<QueryPurchasesSucceedData>> QueryPurchasesSucceed;
    public Action<string> QueryPurchasesFailed;
    protected List<SkuDetailsSucceedData> skuDetailsSucceedData = new List<SkuDetailsSucceedData>();
    public abstract void CheckForUpdate(Action<MarketUpdateResult> callback);

    public static string GetSeparatedPrice(float price)
    {
        return String.Format("{0:#,##0.##}", price);
    }
}
