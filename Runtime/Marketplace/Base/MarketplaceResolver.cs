public static class MarketplaceResolver
{
    public static MarketplaceType Resolve(MarketplaceType built, bool isIran, bool isAndroid, bool isIos)
    {
        switch (built)
        {
            case MarketplaceType.Cafebazaar:
            case MarketplaceType.Myket:
            case MarketplaceType.Tester:
                return isAndroid ? built : MarketplaceType.InGame;
            case MarketplaceType.GooglePlay:
                if (isIran)
                    return MarketplaceType.Zarinpal;
                return isAndroid ? built : MarketplaceType.InGame;
            case MarketplaceType.AppStore:
                return isIos ? built : MarketplaceType.InGame;
            case MarketplaceType.Zarinpal:
                return built;
            default:
                return MarketplaceType.InGame;
        }
    }
}
