// Checks Java files with Code Market's own compiler (JavaMarket.Java.dll), outside the game.
//
// The IDE only knows the stubs; the game's interpreter supports just part of the JDK and has its own
// rules. This compiles each file the way the game would (after deleting "extends GameScript", as you
// would when pasting) and reports the game's own error messages.
// See tools/README.md for how to run it.
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

static class Check
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: dotnet Check.dll <Code Market_Data/Managed dir> <file.java>...");
            return 2;
        }
        var dir = args[0];
        Console.OutputEncoding = Encoding.UTF8;
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
        {
            var p = Path.Combine(dir, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(p) ? Assembly.LoadFrom(p) : null;
        };
        var game = Assembly.LoadFrom(Path.Combine(dir, "JavaMarket.dll"));
        var java = Assembly.LoadFrom(Path.Combine(dir, "JavaMarket.Java.dll"));
        java.GetType("JavaMarket.Loc").GetProperty("Current", All)
            .SetValue(null, Enum.Parse(java.GetType("JavaMarket.GameLanguage"), "English"));

        var compile = java.GetType("JavaMarket.Java.JavaEngine").GetMethod("Compile", All);
        var features = java.GetType("JavaMarket.Java.FeatureSet").GetProperty("All", All).GetValue(null);

        var failed = 0;
        foreach (var file in args.Skip(1))
        {
            var source = AsInGame(File.ReadAllText(file));
            var res = compile.Invoke(null, new object[] { source, BuildHost(game, java), features, null, Path.GetFileName(file) });
            if ((bool)res.GetType().GetProperty("Success").GetValue(res))
            {
                Console.WriteLine(file + ": OK");
                continue;
            }
            failed++;
            var err = res.GetType().GetField("Error").GetValue(res);
            var line = (int)err.GetType().GetProperty("Line").GetValue(err);
            var col = (int)err.GetType().GetProperty("Column").GetValue(err);
            var msg = err is Exception ? ((Exception)err).Message : err.ToString();
            Console.WriteLine(file + ":" + line + ":" + col + ": error: " + msg);
        }
        return failed == 0 ? 0 : 1;
    }

    // The game has no GameScript class, so a pasted script has "extends GameScript" deleted. Do the same,
    // blanking it with spaces so the game's line and column numbers still match the file.
    static string AsInGame(string source)
    {
        return System.Text.RegularExpressions.Regex.Replace(source, @"\bextends\s+(?:javax\.codemarket\.)?GameScript\b",
            m => new string(m.Value.Select(c => c == '\n' || c == '\r' ? c : ' ').ToArray()));
    }

    // Registers what GameScripting.CreateHost does (enums, script types, the robot and store globals)
    // without the live Unity robot and store that CreateHost needs. Enough to compile, not to run.
    static object BuildHost(Assembly game, Assembly java)
    {
        var hostType = java.GetType("JavaMarket.Java.HostEnvironment");
        var host = hostType.GetMethod("CreateStandard", All).Invoke(null, null);
        var gs = game.GetType("JavaMarket.Scripting.GameScripting");
        foreach (DictionaryEntry e in (IDictionary)gs.GetField("Enums", All).GetValue(null))
            hostType.GetMethod("RegisterEnum", All).Invoke(host, new object[] { e.Key, ((IEnumerable)e.Value).Cast<string>().ToArray() });
        foreach (var t in new[] { "Robot", "Store", "Order", "Customer", "Shelf", "Cell" })
            hostType.GetMethod("RegisterType", All).Invoke(host, new object[] { t, null });
        hostType.GetMethod("RegisterGlobal", All).Invoke(host, new[] { "robot", FormatterServices.GetUninitializedObject(game.GetType("JavaMarket.Scripting.RobotApi")) });
        hostType.GetMethod("RegisterGlobal", All).Invoke(host, new[] { "store", FormatterServices.GetUninitializedObject(game.GetType("JavaMarket.Scripting.StoreApi")) });
        return host;
    }
}
