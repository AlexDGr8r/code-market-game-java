// Extracts Code Market's Java scripting API from the game's own assemblies and writes it as JSON.
//
// The game embeds the API as data tables (JavaMarket.Scripting.*Commands.Methods), so this reads
// them by reflection instead of guessing from decompiled code. See tools/README.md for how to run it.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

static class Extract
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    // Game-side owner names ("robot") to the Java type the player sees ("Robot").
    static readonly string[][] Owners =
    {
        new[] { "Robot", "robot" }, new[] { "Store", "store" }, new[] { "Shelf", "shelf" },
        new[] { "Cell", "cell" }, new[] { "Customer", "customer" }, new[] { "Order", "order" },
    };

    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: dotnet Extract.dll <Code Market_Data/Managed dir> <out.json> [language]");
            return 2;
        }
        var dir = args[0];
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
        {
            var p = Path.Combine(dir, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(p) ? Assembly.LoadFrom(p) : null;
        };
        var game = Assembly.LoadFrom(Path.Combine(dir, "JavaMarket.dll"));
        var java = Assembly.LoadFrom(Path.Combine(dir, "JavaMarket.Java.dll"));
        java.GetType("JavaMarket.Loc").GetProperty("Current", All)
            .SetValue(null, Enum.Parse(java.GetType("JavaMarket.GameLanguage"), args.Length > 2 ? args[2] : "English"));

        var gs = game.GetType("JavaMarket.Scripting.GameScripting");
        var entityNames = ToStringMap(gs.GetField("entityNames", All).GetValue(null));
        var productNames = ToStringMap(gs.GetField("productNames", All).GetValue(null));

        var j = new Json();
        j.Open('{');

        j.Key("gameVersion"); j.Str(GameVersion(dir));

        // Host types and their methods.
        j.Key("types"); j.Open('{');
        foreach (var o in Owners)
        {
            j.Key(o[0]); j.Open('{');
            j.Key("variable"); j.Str(o[1]);
            j.Key("methods"); j.Open('[');
            var list = game.GetType("JavaMarket.Scripting." + o[0] + "Commands").GetField("Methods", All).GetValue(null);
            foreach (var m in (IEnumerable)list)
            {
                j.Open('{');
                j.Key("name"); j.Str((string)Prop(m, "Name"));
                j.Key("returns"); j.Str((string)Prop(m, "Returns"));
                j.Key("kind"); j.Str(Prop(m, "Kind").ToString());
                j.Key("params"); j.Open('[');
                foreach (var p in (IEnumerable)Prop(m, "Parameters"))
                {
                    j.Open('{');
                    j.Key("type"); j.Str((string)p.GetType().GetField("Type").GetValue(p));
                    j.Key("name"); j.Str((string)p.GetType().GetField("Name").GetValue(p));
                    j.Close('}');
                }
                j.Close(']');
                j.Key("doc"); j.Str((string)Prop(m, "Description"));
                j.Close('}');
            }
            j.Close(']');
            j.Close('}');
        }
        j.Close('}');

        // Enums exposed to scripts.
        j.Key("enums"); j.Open('{');
        foreach (DictionaryEntry e in (IDictionary)gs.GetField("Enums", All).GetValue(null))
        {
            j.Key((string)e.Key); j.Open('[');
            foreach (string c in (IEnumerable)e.Value) j.Str(c);
            j.Close(']');
        }
        j.Close('}');

        // Methods callable on a Product constant (Product.BEER.getPrice()).
        j.Key("productMethods"); j.Open('[');
        foreach (var t in (IEnumerable)gs.GetField("ProductMethods", All).GetValue(null))
        {
            var tt = t.GetType();
            j.Open('{');
            j.Key("name"); j.Str(tt.GetField("Item1").GetValue(t).ToString());
            j.Key("returns"); j.Str(tt.GetField("Item2").GetValue(t).ToString());
            j.Key("doc"); j.Str(tt.GetField("Item3").GetValue(t).ToString());
            j.Close('}');
        }
        j.Close(']');

        // Static facts about each product, from the game's product catalog.
        var catalog = game.GetType("JavaMarket.World.ProductCatalog");
        var productEnum = game.GetType("JavaMarket.World.Product");
        Func<string, object, object> cat = (name, prod) => catalog.GetMethod(name, All).Invoke(null, new[] { prod });
        j.Key("products"); j.Open('[');
        foreach (var prod in Enum.GetValues(productEnum))
        {
            j.Open('{');
            j.Key("constant"); j.Str(productNames[prod.ToString()]);
            j.Key("name"); j.Str((string)cat("DisplayName", prod));
            j.Key("price"); j.Raw(cat("Price", prod).ToString());
            var rack = cat("Rack", prod).ToString();
            j.Key("rack"); j.Str(entityNames.ContainsKey(rack) ? entityNames[rack] : rack);
            var season = cat("SeasonOf", prod);
            j.Key("season"); if (season == null) j.Raw("null"); else j.Str(season.ToString().ToUpperInvariant());
            foreach (var flag in new[] { "IsAlcohol", "IsAdultsOnly", "IsPremium", "IsElite", "IsVending", "IsInfinite" })
            {
                j.Key(char.ToLowerInvariant(flag[0]) + flag.Substring(1));
                j.Raw(((bool)cat(flag, prod)) ? "true" : "false");
            }
            j.Close('}');
        }
        j.Close(']');

        // Reference notes for each entity (what the in-game "?" reference says about it).
        j.Key("entityNotes"); j.Open('{');
        var notes = (IDictionary)game.GetType("JavaMarket.App.ReferenceCatalog").GetField("EntityNotes", All).GetValue(null);
        foreach (DictionaryEntry e in notes)
        {
            var key = e.Key.ToString();
            j.Key(entityNames.ContainsKey(key) ? entityNames[key] : key);
            j.Str(e.Value.ToString());
        }
        j.Close('}');

        j.Key("starterProgram"); j.Str((string)gs.GetProperty("StarterProgram", All).GetValue(null));
        j.Close('}');

        File.WriteAllText(args[1], j.ToString(), new UTF8Encoding(false));
        Console.WriteLine("wrote " + args[1]);
        return 0;
    }

    static string GameVersion(string managedDir)
    {
        // Best effort: the Steam build id from steamapps/appmanifest_*.acf, which identifies the exact game build.
        try
        {
            var gameDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetFullPath(managedDir).TrimEnd('/', '\\')));
            var steamapps = Path.GetDirectoryName(Path.GetDirectoryName(gameDir));
            var installDir = "\"" + Path.GetFileName(gameDir) + "\"";
            foreach (var acf in Directory.GetFiles(steamapps, "appmanifest_*.acf"))
            {
                var lines = File.ReadAllLines(acf);
                if (!lines.Any(l => l.Contains("\"installdir\"") && l.Contains(installDir))) continue;
                var build = lines.FirstOrDefault(l => l.Contains("\"buildid\""));
                if (build != null) return "steam build " + build.Split('"').Where(s => s.Trim().Length > 0).Last();
            }
        }
        catch { }
        return "unknown";
    }

    static Dictionary<string, string> ToStringMap(object dict)
    {
        var r = new Dictionary<string, string>();
        foreach (DictionaryEntry e in (IDictionary)dict) r[e.Key.ToString()] = e.Value.ToString();
        return r;
    }

    static object Prop(object o, string name)
    {
        for (var t = o.GetType(); t != null; t = t.BaseType)
        {
            var p = t.GetProperty(name, All | BindingFlags.DeclaredOnly);
            if (p != null) return p.GetValue(o);
        }
        throw new MissingMemberException(o.GetType().Name, name);
    }

    // Minimal pretty JSON writer (the .NET Framework compiler this is built with has no JSON library on .NET 8's facade).
    class Json
    {
        readonly StringBuilder sb = new StringBuilder();
        readonly Stack<bool> first = new Stack<bool>();
        bool afterKey;

        void Sep()
        {
            if (afterKey) { afterKey = false; return; }
            if (first.Count > 0)
            {
                if (!first.Peek()) sb.Append(',');
                first.Pop(); first.Push(false);
                sb.Append('\n').Append(' ', first.Count * 2);
            }
        }
        public void Open(char c) { Sep(); sb.Append(c); first.Push(true); }
        public void Close(char c)
        {
            var empty = first.Pop();
            if (!empty) sb.Append('\n').Append(' ', first.Count * 2);
            sb.Append(c);
        }
        public void Key(string k) { Sep(); Quote(k); sb.Append(": "); afterKey = true; }
        public void Str(string s) { Sep(); if (s == null) sb.Append("null"); else Quote(s); }
        public void Raw(string s) { Sep(); sb.Append(s); }
        void Quote(string s)
        {
            sb.Append('"');
            foreach (var ch in s)
            {
                switch (ch)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (ch < 0x20) sb.Append("\\u").Append(((int)ch).ToString("x4")); else sb.Append(ch);
                        break;
                }
            }
            sb.Append('"');
        }
        public override string ToString() { return sb.ToString() + "\n"; }
    }
}
