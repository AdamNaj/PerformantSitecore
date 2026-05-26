using System;

namespace PerformantSitecore.Foundation.ManagedCache.Extensions;

public static class LongExtensions
{
    static readonly string[] SizeSuffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

    public static string ToSizeString(this long value, int decimalPlaces = 2)
    {
        if (value < 0) { return "-" + (-value).ToSizeString(decimalPlaces); }

        if (value == 0) { return "0 Bytes"; }

        int mag = (int)Math.Log(value, 1024);
        decimal adjustedSize = (decimal)value / (1L << (mag * 10));

        if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
        {
            mag += 1;
            adjustedSize /= 1024;
        }

        if (decimalPlaces > 0)
        {
            var sizeString = string.Format("{0:n" + decimalPlaces + "}", adjustedSize).Trim('0').Trim('.');
            return $"{sizeString} {SizeSuffixes[mag]}";
        }

        return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
    }
}
