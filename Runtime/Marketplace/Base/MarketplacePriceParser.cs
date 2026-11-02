using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public static class MarketplacePriceParser
{
    private const string PersianDigits = "۰۱۲۳۴۵۶۷۸۹";
    private const string ArabicDigits = "٠١٢٣٤٥٦٧٨٩";

    public static float Parse(string price, float divisor)
    {
        if (string.IsNullOrEmpty(price))
            return 0f;

        string digits = Regex.Match(ToLatinDigits(price).Replace(",", string.Empty).Replace("٬", string.Empty), "[0-9]+").Value;
        return float.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out float value)
            ? value / divisor
            : 0f;
    }

    private static string ToLatinDigits(string text)
    {
        StringBuilder result = new StringBuilder(text.Length);
        foreach (char character in text)
        {
            int digit = PersianDigits.IndexOf(character);
            if (digit < 0)
                digit = ArabicDigits.IndexOf(character);
            result.Append(digit < 0 ? character : (char)('0' + digit));
        }

        return result.ToString();
    }
}
