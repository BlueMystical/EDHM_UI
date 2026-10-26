using System;
using System.IO;
using System.Reflection;

internal static class LaunchProbe
{
    [STAThread]
    private static void Main()
    {
        var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var temporary = Path.Combine(directory, "launch-result.tmp");
        File.WriteAllText(temporary, Environment.CurrentDirectory);
        File.Move(temporary, Path.Combine(directory, "launch-result.txt"));
    }
}
