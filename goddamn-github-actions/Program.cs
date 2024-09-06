using System.Globalization;
using goddamn_github_actions;

var uiThread = new Thread(() => new TestUi().Run())
{
    CurrentCulture = CultureInfo.InvariantCulture, // We don't want locale-specific numbers
    CurrentUICulture = CultureInfo.InvariantCulture
};
uiThread.Start();