using Hikaria.QC.Suggestors.Tags;
using Hikaria.QC.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hikaria.QC
{
    public static partial class QuantumConsoleProcessor
    {
        [Command("help", "Shows a basic help guide for Quantum Console")]
        private static string GetHelp()
        {
            return QuantumGlobal.Localization.Get(12);
        }

        [Command("manual")]
        [Command("man")]
        private static string ManualHelp()
        {
            return QuantumGlobal.Localization.Get(13);
        }

        [CommandDescription("Generates a user manual for any given command, including built in ones. To use the man command, simply put the desired command name infront of it. For example, 'man my-command' will generate the manual for 'my-command'")]
        [Command("help")]
        [Command("manual")]
        [Command("man")]
        private static string GenerateCommandManual([CommandName] string commandName)
        {
            string[] matchingCommands = 
                _commandTable
                    .Keys
                    .Where(key => key.Split('(')[0] == commandName)
                    .OrderBy(key => key)
                    .ToArray();

            if (matchingCommands.Length == 0)
            {
                throw new ArgumentException(QuantumGlobal.Localization.Format(14, commandName));
            }

            Dictionary<string, ParameterInfo> foundParams = new Dictionary<string, ParameterInfo>();
            Dictionary<string, Type> foundGenericArguments = new Dictionary<string, Type>();
            Dictionary<string, CommandParameterDescriptionAttribute> foundParamDescriptions = new Dictionary<string, CommandParameterDescriptionAttribute>();
            List<Type> declaringTypes = new List<Type>(1);

            string manual = QuantumGlobal.Localization.Format(15, commandName);

            for (int i = 0; i < matchingCommands.Length; i++)
            {
                CommandData currentCommand = _commandTable[matchingCommands[i]];
                declaringTypes.Add(currentCommand.MethodData.DeclaringType);

                manual += $"\n   - {currentCommand.CommandSignature}";
                if (!currentCommand.IsStatic) { manual += $" (mono-target = {currentCommand.MonoTarget.ToString().ToLower()})"; }
                for (int j = 0; j < currentCommand.ParamCount; j++)
                {
                    ParameterInfo param = currentCommand.MethodParamData[j];
                    if (!foundParams.ContainsKey(param.Name)) { foundParams.Add(param.Name, param); }
                    if (!foundParamDescriptions.ContainsKey(param.Name))
                    {
                        CommandParameterDescriptionAttribute descriptionAttribute = param.GetCustomAttribute<CommandParameterDescriptionAttribute>();
                        if (descriptionAttribute != null && descriptionAttribute.Valid) { foundParamDescriptions.Add(param.Name, descriptionAttribute); }
                    }
                }

                if (currentCommand.IsGeneric)
                {
                    Type[] genericArgs = currentCommand.GenericParamTypes;
                    for (int j = 0; j < genericArgs.Length; j++)
                    {
                        Type arg = genericArgs[j];
                        if (!foundGenericArguments.ContainsKey(arg.Name)) { foundGenericArguments.Add(arg.Name, arg); }
                    }
                }
            }

            if (foundParams.Count > 0)
            {
                manual += QuantumGlobal.Localization.Get(17);
                ParameterInfo[] commandParams = foundParams.Values.ToArray();
                for (int i = 0; i < commandParams.Length; i++)
                {
                    ParameterInfo currentParam = commandParams[i];
                    string typeName = currentParam.ParameterType.GetDisplayName();

                    // Add keywords
                    if (currentParam.HasAttribute<ParamArrayAttribute>())
                    {
                        typeName = $"params {typeName}";
                    }

                    manual += $"\n   - {currentParam.Name}: {typeName}";
                }
            }

            string genericConstraintInformation = "";
            if (foundGenericArguments.Count > 0)
            {
                Type[] genericArgs = foundGenericArguments.Values.ToArray();
                for (int i = 0; i < genericArgs.Length; i++)
                {
                    Type arg = genericArgs[i];
                    Type[] typeConstraints = arg.GetGenericParameterConstraints();
                    GenericParameterAttributes attributes = arg.GenericParameterAttributes;

                    List<string> formattedConstraints = new List<string>();
                    if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)) { formattedConstraints.Add("struct"); }
                    if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint)) { formattedConstraints.Add("class"); }
                    for (int j = 0; j < typeConstraints.Length; j++) { formattedConstraints.Add(typeConstraints[i].GetDisplayName()); }
                    if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)) { formattedConstraints.Add("new()"); }

                    if (formattedConstraints.Count > 0)
                    {
                        genericConstraintInformation += $"\n   - {arg.Name}: {string.Join(", ", formattedConstraints)}";
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(genericConstraintInformation)) { manual += QuantumGlobal.Localization.Format(18, genericConstraintInformation); }

            for (int i = 0; i < matchingCommands.Length; i++)
            {
                CommandData currentCommand = _commandTable[matchingCommands[i]];
                if (currentCommand.TryGetLocalization(out var localization) && !string.IsNullOrEmpty(localization.Description))
                {
                    manual += QuantumGlobal.Localization.Format(19, localization.Description);
                    i = matchingCommands.Length;
                }
                else if (currentCommand.HasDescription)
                {
                    manual += QuantumGlobal.Localization.Format(19, currentCommand.CommandDescription);
                    i = matchingCommands.Length;
                }
            }

            if (foundParamDescriptions.Count > 0)
            {
                manual += QuantumGlobal.Localization.Get(20);
                ParameterInfo[] commandParams = foundParams.Values.ToArray();
                for (int i = 0; i < commandParams.Length; i++)
                {
                    ParameterInfo currentParam = commandParams[i];
                    if (foundParamDescriptions.ContainsKey(currentParam.Name))
                    {
                        manual += $"\n - {currentParam.Name}: {foundParamDescriptions[currentParam.Name].Description}";
                    }
                }
            }

            declaringTypes = declaringTypes.Distinct().ToList();
            manual += QuantumGlobal.Localization.Get(21);
            if (declaringTypes.Count == 1) { manual += $" {declaringTypes[0].GetDisplayName(true)}"; }
            else
            {
                manual += ":";
                foreach (Type type in declaringTypes)
                {
                    manual += $"\n   - {type.GetDisplayName(true)}";
                }
            }

            return manual;
        }

        /// <summary>
        /// Gets all loaded unique commands. Unique excludes multiple overloads of the same command from appearing.
        /// </summary>
        /// <returns>All loaded unique commands.</returns>
        public static IEnumerable<CommandData> GetUniqueCommands()
        {
            return Utilities.CollectionExtensions.DistinctBy(GetAllCommands(), x => x.CommandName)
                .OrderBy(x => x.CommandName);
        }

        [CommandDescription("Generates a list of all commands currently loaded by the Quantum Console Processor")]
        [Command("commands")]
        [Command("all-commands")]
        private static string GenerateCommandList()
        {
            string output = QuantumGlobal.Localization.Get(22);
            foreach (CommandData command in GetUniqueCommands())
            {
                output += $"\n   - {command.CommandName}";
            }

            return output;
        }

        [Command("user-commands", "Generates a list of all commands added by the user")]
        private static IEnumerable<string> GenerateUserCommandList()
        {
            return GetUniqueCommands()
                .Where(x => !x.MethodData.DeclaringType.Assembly.FullName.StartsWith("Hikaria.QC"))
                .Select(x => $"   - {x.CommandName}");
        }
    }
}
