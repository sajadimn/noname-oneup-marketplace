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

## Android files and build configs
Two folders hold files that must not be in the game all the time, so Unity must ignore them. Their names end in `~`, which Unity does not import.

- `Stores~` (inside this package): the Android files of each store, a jar and a `Market.androidlib` with that store's manifest. The Bazaar and Myket jars contain the same classes, so they cannot both be in the project at once. A build copies only the chosen store into `Assets/Plugins/Android/Market`, and the build restores the project afterwards. Only CafeBazaar and MyKet builds need this (`hasMarketFile` on). ZarinPal and Google Play have no market files. `Stores~/ZarinPal` holds the manifest template the package fills from `ZarinPalConfig`.
- `Market~` (in your project, `Assets/Settings/BuildPipeline/Market~`, created by the build pipeline package): the game's own `AndroidManifest.xml` for store builds, kept out of `Assets/Plugins/Android` until a build copies it there. Every store config points at it with `manifestPath`.

The package sets these build config paths itself when it is imported and whenever a config in `Assets/Settings/BuildPipeline` is imported (menu `OneUp/Marketplace/Set Build Config Paths` does it by hand). `CafeBazaar*` and `MyKet*` configs get `marketFilesPath` = `Packages/noname-oneup-marketplace/Stores~/<Store>`. `CafeBazaar*`, `MyKet*`, `ZarinPal*` and `GooglePlay*` configs get `manifestPath` = `Assets/Settings/BuildPipeline/Market~/AndroidManifest.xml`. The `Packages/...` form matters: a tarball package is unpacked to `Library/PackageCache/<name>@<hash>`, and the hash is different on every computer, so that path breaks builds elsewhere. An empty value, or an old `Library/PackageCache/...` path, is replaced; any other value you set is kept. The build pipeline turns `Packages/...` into the real folder on each computer.

ZarinPal returns to the game with a deep link. Set `callbackScheme`, `callbackHost` and `callbackActivity` in `ZarinPalConfig`. The package fills the ZarinPal manifest from `Stores~/ZarinPal` before each build, and saving the config updates the current copy. With an empty callback the link is left out.

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
