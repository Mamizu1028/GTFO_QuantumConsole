using Hikaria.QC.Utilities;
using System;
using System.Runtime.CompilerServices;

namespace Hikaria.QC
{
    /// <summary>
    /// Creates a prefix that will be prepended to all commands made within this class. Works recursively with sub-classes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class CommandPrefixAttribute : Attribute
    {
        public readonly string Prefix;
        public readonly bool Valid = true;

        private static readonly char[] _bannedAliasChars = { ' ', '(', ')', '{', '}', '[', ']', '<', '>' };

        public CommandPrefixAttribute([CallerMemberName] string prefixName = "")
        {
            Prefix = prefixName;
            foreach (var c in _bannedAliasChars)
            {
                if (Prefix.Contains(c))
                {
                    string errorMessage = QuantumGlobal.Localization.Format(41,
                        "Development Processor Error: Command prefix '{0}' contains the char '{1}' which is banned. Unexpected behaviour may occurr.", Prefix, c);
                    Logs.Error(errorMessage);

                    Valid = false;
                    throw new ArgumentException(errorMessage, nameof(prefixName));
                }
            }
        }
    }
}
