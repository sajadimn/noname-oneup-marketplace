using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarketplaceSkuCache
{
    private readonly Dictionary<string, SKUData> items = new Dictionary<string, SKUData>();
    private readonly List<SkuListRequest> listRequests = new List<SkuListRequest>();

    public string[] Request(string sku, Action<SkuDetailsSucceedData> callBack, bool forceUpdate)
    {
        SKUData item = GetItem(sku);
        if (item.skuDetailData != null)
        {
            callBack?.Invoke(item.skuDetailData);
            return Array.Empty<string>();
        }

        item.callBack += callBack;
        if (item.requested && !forceUpdate)
            return Array.Empty<string>();

        item.requested = true;
        return new[] { sku };
    }

    public string[] Request(List<string> skus, Action<List<SkuDetailsSucceedData>> callBack, bool forceUpdate)
    {
        if (!forceUpdate && skus.All(sku => GetItem(sku).skuDetailData != null))
        {
            callBack?.Invoke(GetCached(skus));
            return Array.Empty<string>();
        }

        listRequests.Add(new SkuListRequest { skus = skus, callBack = callBack });
        foreach (string sku in skus)
            GetItem(sku).requested = true;
        return skus.ToArray();
    }

    public void RemoveCallback(string sku, Action<SkuDetailsSucceedData> callBack)
    {
        if (items.TryGetValue(sku, out SKUData item))
            item.callBack -= callBack;
    }

    public void Succeed(List<SkuDetailsSucceedData> details)
    {
        foreach (SkuDetailsSucceedData detail in details)
        {
            SKUData item = GetItem(detail.productId);
            item.skuDetailData = detail;
            item.requested = false;
            Answer(item, detail);
        }

        if (listRequests.Count == 0)
            return;

        List<SkuListRequest> finished = listRequests
            .Where((request, index) => index == 0 || request.skus.All(sku => GetItem(sku).skuDetailData != null))
            .ToList();
        foreach (SkuListRequest request in finished)
        {
            listRequests.Remove(request);
            AnswerList(request);
        }
    }

    public void Failed()
    {
        foreach (SKUData item in items.Values.Where(item => item.skuDetailData == null).ToList())
        {
            item.requested = false;
            Answer(item, null);
        }

        List<SkuListRequest> finished = listRequests.ToList();
        listRequests.Clear();
        foreach (SkuListRequest request in finished)
            AnswerList(request);
    }

    private SKUData GetItem(string sku)
    {
        if (!items.TryGetValue(sku, out SKUData item))
        {
            item = new SKUData();
            items.Add(sku, item);
        }

        return item;
    }

    private List<SkuDetailsSucceedData> GetCached(List<string> skus)
    {
        return skus.Select(sku => GetItem(sku).skuDetailData).Where(detail => detail != null).ToList();
    }

    private static void Answer(SKUData item, SkuDetailsSucceedData detail)
    {
        Action<SkuDetailsSucceedData> callBack = item.callBack;
        item.callBack = null;
        try
        {
            callBack?.Invoke(detail);
        }
        catch (Exception exception)
        {
            Debug.LogError("MarketplaceSkuCache - Answer - Exception" + exception);
        }
    }

    private void AnswerList(SkuListRequest request)
    {
        try
        {
            request.callBack?.Invoke(GetCached(request.skus));
        }
        catch (Exception exception)
        {
            Debug.LogError("MarketplaceSkuCache - AnswerList - Exception" + exception);
        }
    }
}
