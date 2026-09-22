using System.Xml;
using System.Xml.Linq;

namespace Asterion.Core;

public record Binding(string Map, string Action, string Input, string Status);
public static class Bindings
{
    public static List<Binding> Parse(string xml)
    {
        using var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 8_000_000 });
        var document = XDocument.Load(reader);
        var result = new List<Binding>();
        foreach (var map in document.Descendants("actionmap"))
        foreach (var action in map.Elements("action"))
        foreach (var bind in action.Elements("rebind"))
        {
            string input = (string?)bind.Attribute("input") ?? "";
            // Multi-tap / activation modes require explicit user configuration; never silently reduce them to taps.
            bool complex = bind.Attributes().Any(a => a.Name.LocalName != "input" && a.Value != "0")
                || action.Attributes().Any(a=>a.Name.LocalName!="name" && a.Value!="0");
            result.Add(new((string?)map.Attribute("name") ?? "", (string?)action.Attribute("name") ?? "", input,
                complex ? "UNSUPPORTED ACTIVATION" : KeyChord.TryParse(input, out _) ? "BOUND" : input == "" ? "UNBOUND" : "UNSUPPORTED INPUT"));
        }
        return result;
    }
}

public record KeyChord(ushort[] ScanCodes, int MouseButton = 0)
{
    static readonly Dictionary<string, ushort> Keys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["escape"]=1,["1"]=2,["2"]=3,["3"]=4,["4"]=5,["5"]=6,["6"]=7,["7"]=8,["8"]=9,["9"]=10,["0"]=11,
        ["minus"]=12,["equals"]=13,["backspace"]=14,["tab"]=15,["q"]=16,["w"]=17,["e"]=18,["r"]=19,["t"]=20,["y"]=21,["u"]=22,["i"]=23,["o"]=24,["p"]=25,
        ["lbracket"]=26,["rbracket"]=27,["enter"]=28,["lctrl"]=29,["a"]=30,["s"]=31,["d"]=32,["f"]=33,["g"]=34,["h"]=35,["j"]=36,["k"]=37,["l"]=38,
        ["semicolon"]=39,["apostrophe"]=40,["tilde"]=41,["lshift"]=42,["backslash"]=43,["z"]=44,["x"]=45,["c"]=46,["v"]=47,["b"]=48,["n"]=49,["m"]=50,
        ["comma"]=51,["period"]=52,["slash"]=53,["rshift"]=54,["lalt"]=56,["space"]=57,["capslock"]=58,
        ["f1"]=59,["f2"]=60,["f3"]=61,["f4"]=62,["f5"]=63,["f6"]=64,["f7"]=65,["f8"]=66,["f9"]=67,["f10"]=68,["f11"]=87,["f12"]=88,
        ["rctrl"]=0x11d,["ralt"]=0x138,["up"]=0x148,["down"]=0x150,["left"]=0x14b,["right"]=0x14d,["home"]=0x147,["end"]=0x14f,["pgup"]=0x149,["pgdn"]=0x151,["insert"]=0x152,["delete"]=0x153
    };
    public static bool TryParse(string input, out KeyChord chord)
    {
        chord = new([]);
        if (!input.StartsWith("kb1_", StringComparison.OrdinalIgnoreCase)) return false;
        var parts = input[4..].Split('+', StringSplitOptions.TrimEntries);
        if (parts.Length is < 1 or > 4) return false;
        var codes = new List<ushort>(); int mouse = 0;
        foreach (string part in parts)
        {
            if (part is "mouse4" or "mouse5") { if (parts.Length != 1) return false; mouse = part == "mouse4" ? 1 : 2; }
            else if (Keys.TryGetValue(part, out var code)) codes.Add(code);
            else return false;
        }
        if (codes.Distinct().Count() != codes.Count) return false;
        // Modifier-first ordering matters for chords written as f6+lalt by the game.
        codes = codes.OrderBy(c => c is 29 or 42 or 54 or 56 or 0x11d or 0x138 ? 0 : 1).ToList();
        chord = new(codes.ToArray(), mouse); return true;
    }
}
