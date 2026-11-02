using System;
using System.Collections.Generic;

public class InGameMarketplace : Marketplace
{
    private const string NotAvailable = "Marketplace Not Available";

    public override void Init(MarketplaceConfig _config)
    {
        marketplaceType = _config != null ? _config.marketplaceType : MarketplaceType.InGame;
    }

    public override void LoadMarketplace()
    {
    }

    public override void Rate()
    {
    }

    public override void AppPage()
    {
    }

    protected override void SetEvents()
    {
    }

    protected override void DestroyEvents()
    {
    }

    public override void SkuDetails(string[] _SKU)
    {
        SkuDetailsFailed?.Invoke(NotAvailable);
    }

    public override void PurchaseProduct(string _SKU)
    {
        PurchaseProductFailed?.Invoke(NotAvailable);
    }

    public override void ConsumeProduct(string _SKU)
    {
        ConsumeProductFailed?.Invoke(NotAvailable);
    }

    public override void QueryPurchases()
    {
        QueryPurchasesSucceed?.Invoke(new List<QueryPurchasesSucceedData>());
    }

    public override void CheckForUpdate(Action<MarketUpdateResult> callback)
    {
        callback?.Invoke(null);
    }
}
