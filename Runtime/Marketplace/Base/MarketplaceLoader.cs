using UnityEngine;
using UnityEngine.UI;

public static class MarketplaceLoader
{
    private const int SortingOrder = 1000;
    private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.6f);

    public static Canvas Create(Canvas prefab, Transform parent)
    {
        Canvas loader = prefab != null ? Object.Instantiate(prefab, parent) : CreateDefault(parent);
        loader.gameObject.SetActive(false);
        return loader;
    }

    public static Canvas CreateDefault(Transform parent)
    {
        GameObject loaderObject = new GameObject("Loader");
        loaderObject.transform.SetParent(parent);
        Canvas loader = loaderObject.AddComponent<Canvas>();
        loader.renderMode = RenderMode.ScreenSpaceOverlay;
        loader.sortingOrder = SortingOrder;
        loaderObject.AddComponent<GraphicRaycaster>();

        GameObject dimObject = new GameObject("Dim");
        dimObject.transform.SetParent(loaderObject.transform, false);
        Image dim = dimObject.AddComponent<Image>();
        dim.color = DimColor;
        RectTransform rect = dim.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return loader;
    }
}
