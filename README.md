Marketplace Unity Package
===

## Description
This package contains the store layer for in-app purchases, update checks and rating on Cafe Bazaar, Myket, ZarinPal and Google Play. Keys stay in your project, never in this package.

## Dependencies
Add these to `Packages/manifest.json` before this package, or it cannot compile:
- [noname-oneup-singleton](https://github.com/sajadimn/noname-oneup-singleton)
- [noname-oneup-region](https://github.com/sajadimn/noname-oneup-region)

## How to use
1. Add this package. It creates the store configs and `MarketplaceData` in `Assets/Settings/Marketplace/Resources`.
2. `com.unity.purchasing`, `com.google.play.appupdate` and the OpenUPM scope install automatically. To change versions, edit `Assets/Settings/Marketplace/MarketplaceSdkVersionsConfig.asset`, then run `OneUp/Marketplace/Update SDKs`.
3. Enter your values under `OneUp/Marketplace/Configs`. An empty package name uses the app identifier.
4. Add a build define: `CAFE_BAZAAR`, `MY_KET`, `ZARIN_PAL`, `GOOGLE_PLAY`, `APP_STORE`, `IN_GAME` or `TESTER`. A store that cannot run on the platform falls back to `InGame`.
5. Call `MarketplaceManager.Instance.Init(getUserId)` after `RegionManager` is ready. No scene object or prefab is needed.
6. The package creates `MarketplaceLoader.prefab` in `Assets/Settings/Marketplace` and attaches it to `loaderPrefab` in `MarketplaceData`. `Init` creates it under `MarketplaceManager`. Edit that prefab to change how it looks; its root needs a Canvas.

## Android files
Each store's jar and manifest are in `Stores~`, which Unity ignores because the Bazaar and Myket jars share classes. At build time copy the chosen store into `Assets/Plugins/Android/Market`. The package fills the ZarinPal manifest itself before the build.

ZarinPal returns to the game with a deep link. Set `callbackScheme`, `callbackHost` and `callbackActivity` in `ZarinPalConfig`. Use `Stores~/ZarinPal` for ZarinPal builds and for Google Play builds, because Google Play builds use ZarinPal in Iran. Each build fills the link from the config, and saving the config updates the current copy. With an empty callback the link is left out.

## Examples

✅️ Buy and consume
```c#
MarketplaceManager.Instance.PurchaseProduct(sku, false,
    purchase => MarketplaceManager.Instance.SendConsume(purchase.productId, true, GiveReward, OnError, OnNotAvailable),
    OnError, OnNotAvailable);
```

✅️ Recover paid purchases at startup
```c#
MarketplaceManager.Instance.PurchaseRecovered += GiveRewardForPurchase;
MarketplaceManager.Instance.RestorePurchases(GiveRewards, OnError);
```

Only one purchase or consume runs at a time. A late success after a timeout goes to `PurchaseRecovered`.

## Security
A ZarinPal deep link only shows that the player returned to the game. Confirm each payment on your server before giving a reward.
