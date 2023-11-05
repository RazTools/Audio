using System;

namespace Audio.Models.Utils;
public static class ProgressHelper
{
    public static IProgress<double> Instance;

    public static void Reset()
    {
        Instance.Report(0);
    }

    public static void Report(int current, int total)
    {
        var value = current * 100d / total;
        Instance.Report(value);
    }
}
