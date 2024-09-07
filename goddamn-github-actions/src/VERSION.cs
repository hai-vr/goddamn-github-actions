using System.Globalization;
using System.Reflection;

namespace goddamn_github_actions;

internal class VERSION
{
    // ReSharper disable once InconsistentNaming
    public static string version { get; private set; }

    static VERSION()
    {
        var v = Assembly.GetExecutingAssembly().GetName().Version;
        version = string.Format(CultureInfo.InvariantCulture, "v{0}.{1}.{2}", v.Major, v.Minor, v.Build);
    }
}