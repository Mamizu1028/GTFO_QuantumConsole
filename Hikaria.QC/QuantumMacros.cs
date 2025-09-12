using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheArchive.Core.ModulesAPI;

namespace Hikaria.QC
{
    public static class QuantumMacros
    {
        private class MacroPreprocessor : IQcPreprocessor
        {
            public int Priority => 1000;

            public string Process(string text)
            {
                if (!text.StartsWith("#define", StringComparison.CurrentCulture))
                {
                    text = ExpandMacros(text);
                }

                return text;
            }
        }

        private static readonly CustomSetting<Dictionary<string, string>> _macroTable = new CustomSetting<Dictionary<string, string>>("macros", new());

        public static IReadOnlyDictionary<string, string> GetMacros()
        {
            return _macroTable.Value;
        }

        /// <summary>
        /// Expands all the macros in the given text.
        /// </summary>
        /// <returns>The macro expanded text.</returns>
        /// <param name="text">The text to expand the macros in.</param>
        /// <param name="maximumExpansions">The maximum number of macro expansions that can be performed before an exception is thrown.</param>
        public static string ExpandMacros(string text, int maximumExpansions = 1000)
        {
            if (_macroTable.Value.Count == 0)
            {
                return text;
            }

            KeyValuePair<string, string>[] orderedTableCache = null;

            int expansionCount = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '#')
                {
                    if (orderedTableCache == null)
                    {
                        orderedTableCache =
                            _macroTable.Value
                                .OrderByDescending(x => x.Key.Length)
                                .ToArray();
                    }

                    foreach (KeyValuePair<string, string> macro in orderedTableCache)
                    {
                        string key = macro.Key;
                        int keyLength = key.Length;
                        if (i + keyLength < text.Length)
                        {
                            if (string.CompareOrdinal(text, i + 1, key, 0, keyLength) == 0)
                            {
                                if (expansionCount >= maximumExpansions)
                                {
                                    throw new ArgumentException($"Maximum macro expansions of {maximumExpansions} was exhausted: infinitely recursive macro is likely.");
                                }

                                string start = text.Substring(0, i);
                                string end = text.Substring(i + 1 + keyLength);
                                text = $"{start}{macro.Value}{end}";

                                expansionCount++;
                                i--;
                            }
                        }
                    }
                }
            }

            return text;
        }

        [Command("#define")]
        [CommandDescription("Adds a macro to the macro table which can then be used in the Quantum Console. If the macro 'name' is added, " +
                            "then all instances of '#name' will be expanded into the full macro expansion. This allows you to define " +
                            "shortcuts for various things such as long type names or commonly used command strings.\n\n" +
                            "Macros may not contain hashtags or whitespace in their name.\n\n" +
                            "Note: macros will not be expanded when using #define, this is so that defining nested macros is possible.")]
        public static void DefineMacro(string macroName, string macroExpansion)
        {
            macroName = macroName.Trim();
            if (macroName.Contains(' ')) { throw new ArgumentException(QuantumGlobal.Localization.GetById(23, "Macro names cannot contain whitespace.")); }
            if (macroName.Contains('\n')) { throw new ArgumentException(QuantumGlobal.Localization.GetById(24, "Macro names cannot contain newlines.")); }
            if (macroName.Contains('#')) { throw new ArgumentException(QuantumGlobal.Localization.GetById(25, "Macro names cannot contain hashtags.")); }
            if (macroName == "define") { throw new ArgumentException(QuantumGlobal.Localization.GetById(26, "Macros cannot be named define")); }
            if (macroExpansion.Contains('\n')) { throw new ArgumentException(QuantumGlobal.Localization.GetById(27, "Macro expansions cannot contain newlines.")); }
            if (macroExpansion.Contains($"#{macroName}")) { throw new ArgumentException(QuantumGlobal.Localization.GetById(28, "Macros cannot contain themselves within the expansion.")); }

            if (_macroTable.Value.ContainsKey(macroName)) { _macroTable.Value[macroName] = macroExpansion; }
            else { _macroTable.Value.Add(macroName, macroExpansion); }
        }

        [Command("remove-macro")]
        [CommandDescription("Removes the specified macro from the macro table")]
        public static void RemoveMacro(string macroName)
        {
            if (_macroTable.Value.ContainsKey(macroName)) { _macroTable.Value.Remove(macroName); }
            else { throw new Exception(QuantumGlobal.Localization.Format(29, "Specified macro #{0} as it was not defined.", macroName)); }
        }

        [Command("clear-macros")]
        [CommandDescription("Clears the macro table")]
        public static void ClearMacros() { _macroTable.Value.Clear(); }

        [Command("all-macros", "Displays all of the macros currently stored in the macro table")]
        private static string GetAllMacros()
        {
            if (_macroTable.Value.Values.Count == 0) { return QuantumGlobal.Localization.GetById(30, "Macro table is empty"); }
            else { return QuantumGlobal.Localization.Format(31, "Macro table:\n{0}", string.Join("\n", _macroTable.Value.Select((x) => $"#{x.Key} = {x.Value}"))); }
        }

        [Command("dump-macros", "Creates a file dump of macro table which can the be loaded to repopulate the table using load-macros")]
        [CommandPlatform(Platform.AllPlatforms ^ (Platform.WebGLPlayer))]
        public static void DumpMacrosToFile(string filePath)
        {
            using (StreamWriter dumpFile = new StreamWriter(filePath))
            {
                foreach (KeyValuePair<string, string> macro in _macroTable.Value)
                {
                    dumpFile.WriteLine($"{macro.Key} {macro.Value}");
                }

                dumpFile.Flush();
                dumpFile.Close();
            }
        }

        [Command("load-macros", "Loads macros from an external file into the macro table")]
        [CommandPlatform(Platform.AllPlatforms ^ (Platform.WebGLPlayer))]
        public static string LoadMacrosFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException(QuantumGlobal.Localization.Format(32, "file at the specified path '{0}' did not exist.", filePath));
            }

            using (StreamReader macroFile = new StreamReader(filePath))
            {
                List<string> messages = new List<string>();
                while (!macroFile.EndOfStream)
                {
                    string line = macroFile.ReadLine();
                    string[] parts = line.Split(" ".ToCharArray(), 2);
                    if (parts.Length != 2)
                    {
                        messages.Add(QuantumGlobal.Localization.Format(33, "'{0}' is not a valid macro definition", line));
                    }

                    try
                    {
                        DefineMacro(parts[0], parts[1]);
                        messages.Add(QuantumGlobal.Localization.Format(34, "#{0} was successfully defined", parts[0]));
                    }
                    catch (Exception e)
                    {
                        messages.Add(QuantumGlobal.Localization.Format(35, "#{0} could not be defined: {1}", parts[0], e.Message));
                    }
                }

                macroFile.Close();
                return string.Join("\n", messages);
            }
        }
    }
}
