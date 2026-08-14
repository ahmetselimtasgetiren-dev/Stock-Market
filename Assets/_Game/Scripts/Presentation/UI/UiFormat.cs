using System.Globalization;

namespace StockMarket.Presentation.UI
{
    internal static class UiFormat
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        public static string Money(long minorUnits)
        {
            decimal value = minorUnits / 100m;
            return $"FC {value.ToString("N2", Culture)}";
        }

        public static string SignedMoney(long minorUnits)
        {
            string sign = minorUnits > 0 ? "+" : string.Empty;
            return sign + Money(minorUnits);
        }

        public static string Percent(double ratio)
        {
            string sign = ratio > 0d ? "+" : string.Empty;
            return sign + (ratio * 100d).ToString("0.00", Culture) + "%";
        }
    }
}
