using System.Collections.Generic;
using UnityEngine;

public class MarketplaceData : ScriptableObject
{
    public const string FileName = "MarketplaceData";

    public List<MarketplaceStoreData> marketplaces = new List<MarketplaceStoreData>();
    public Canvas loaderPrefab;
    public float showLoaderTimeOut = 5;
}
