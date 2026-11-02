using System;
using System.Globalization;

public static class ZarinPalRequest
{
    private const string Store = "Zarinpal";
    private const string PaidStatus = "OK";

    public static string BuildStartUrl(string baseUrl, ZarinpalPurchaseData data)
    {
        string price = ((decimal)data.price).ToString(CultureInfo.InvariantCulture);
        return $"{baseUrl}?store={Store}&displayname={Escape(data.displayname)}&price={price}" +
               $"&userid={Escape(data.userid)}&appversion={Escape(data.appversion)}&platform={Escape(data.platform)}" +
               $"&gamepkg={Escape(data.gamepkg)}&callbackurl={Escape(data.callbackurl)}" +
               $"&autoverify={Escape(data.autoverify)}&merchant_id={Escape(data.merchant_id)}";
    }

    public static string BuildPendingUrl(string baseUrl, string userId, string packageName)
    {
        return $"{baseUrl}?store={Store}&userid={Escape(userId)}&gamepkg={Escape(packageName)}";
    }

    public static ZarinPalPaymentResult ReadPayment(string url, string scheme, string host, out string paymentToken)
    {
        paymentToken = null;
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) ||
            !string.Equals(uri.Scheme, scheme, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(uri.Host, host, StringComparison.OrdinalIgnoreCase))
            return ZarinPalPaymentResult.Ignored;

        paymentToken = GetQueryParameter(uri, "payment_token");
        return !string.IsNullOrEmpty(paymentToken) && GetQueryParameter(uri, "status") == PaidStatus
            ? ZarinPalPaymentResult.Paid
            : ZarinPalPaymentResult.Failed;
    }

    private static string GetQueryParameter(Uri uri, string name)
    {
        foreach (string parameter in uri.Query.TrimStart('?').Split('&'))
        {
            string[] pair = parameter.Split('=');
            if (pair.Length == 2 && Uri.UnescapeDataString(pair[0]) == name)
                return Uri.UnescapeDataString(pair[1]);
        }

        return null;
    }

    private static string Escape(string value)
    {
        return Uri.EscapeDataString(value ?? string.Empty);
    }
}
