#if UNITY_EDITOR || !UNITY_WEBGL
#define THREADS_SUPPORTED
#endif

using Hikaria.QC.Internal;
using Hikaria.QC.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hikaria.QC
{
    public enum LoggingLevel
    {
        None = 0,
        Errors = 1,
        Warnings = 2,
        Full = 3
    }

    /// <summary>The core processor of Quantum Console handling command table generation and invocation.</summary>
    public static partial class QuantumConsoleProcessor
    {
        /// <summary>The logging level to use during operation of the Quantum Console Processor.</summary>
        public static LoggingLevel loggingLevel = LoggingLevel.Full;

        private static readonly QuantumParser _parser = new QuantumParser();
        private static readonly QuantumPreprocessor _preprocessor = new QuantumPreprocessor();
        private static readonly QuantumScanRuleset _scanRuleset = new QuantumScanRuleset();

        // Mapping of all command keys cmdName(argCount) to the CommandData objects
        // If the command has a dynamic number of parameters, such as by using the params keyword, then the key will be cmdName(#)
        private static readonly ConcurrentDictionary<string, CommandData> _commandTable = new ConcurrentDictionary<string, CommandData>();
        private static readonly List<CommandData> _commandCache = new List<CommandData>();

        public static bool TableGenerated { get; private set; }
        public static bool TableIsGenerating { get; private set; }

        [Command("command-count", "Gets the number of loaded commands")]
        public static int LoadedCommandCount => _loadedCommandCount;
        private static int _loadedCommandCount = 0;
        private static bool _commandCacheDirty = true;

        private static readonly Assembly[] _loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        /// <summary>
        /// Gets all loaded commands.
        /// </summary>
        /// <returns>All loaded commands.</returns>
        public static IEnumerable<CommandData> GetAllCommands()
        {
            if (_commandCacheDirty)
            {
                lock (_commandCache)
                {
                    _commandCache.Clear();
                    _commandCache.AddRange(_commandTable.Values);
                    _commandCacheDirty = false;
                }
            }

            return _commandCache;
        }

        #region Table Generation
        /// <summary>
        /// Generates the command table so that commands can be invoked.
        /// </summary>
        /// <param name="deployThread">If set to <c>true</c> a second thread will be deployed for the table generation.</param>
        /// <param name="forceReload">If set to <c>true</c> then the table will be cleared and generated again.</param>
        public static void GenerateCommandTable(bool deployThread = false, bool forceReload = false)
        {
#if THREADS_SUPPORTED
            if (deployThread)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        GenerateCommandTable(false, forceReload);
                    }
                    catch (Exception e)
                    {
                        Logs.Exception(e);
                    }
                });

                return;
            }
#endif

            lock (_commandTable)
            {
                if (!TableGenerated || forceReload)
                {
                    TableIsGenerating = true;
                    {
                        if (forceReload && TableGenerated)
                        {
                            _commandTable.Clear();
                            _loadedCommandCount = 0;
                        }

#if THREADS_SUPPORTED
                        Parallel.ForEach(_loadedAssemblies, LoadCommandsFromAssembly);
#else
                        foreach (Assembly assembly in _loadedAssemblies)
                        {
                            LoadCommandsFromAssembly(assembly);
                        }
#endif
                    }

                    TableIsGenerating = false;
                    TableGenerated = true;
                    GC.Collect(3, GCCollectionMode.Forced, false, true);
                }
            }
        }

        private static IEnumerable<(MethodInfo method, MemberInfo member)> ExtractCommandMethods(Type type)
        {
            const BindingFlags flags = 
                  BindingFlags.Static 
                | BindingFlags.Instance 
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly;

            MethodInfo[] methods = type.GetMethods(flags);
            PropertyInfo[] properties = type.GetProperties(flags);
            FieldInfo[] fields = type.GetFields(flags);

            foreach (MethodInfo method in methods)
            {
                yield return (method, method);
            }

            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite)
                {
                    yield return (property.SetMethod, property);
                }
                if (property.CanRead)
                {
                    yield return (property.GetMethod, property);
                }
            }

            foreach (FieldInfo field in fields)
            {
                if (field.HasAttribute<CommandAttribute>())
                {
                    if (field.IsDelegate())
                    {
                        if (field.IsStrongDelegate())
                        {
                            FieldDelegateMethod executer = new FieldDelegateMethod(field);
                            yield return (executer, field);
                        }
                        else if (loggingLevel >= LoggingLevel.Warnings)
                        {
                            Logs.Warning(QuantumGlobal.Localization.Format(78, field.Name, field.DeclaringType));
                        }
                    }
                    else
                    {
                        FieldAutoMethod reader = new FieldAutoMethod(field, FieldAutoMethod.AccessType.Read);
                        yield return (reader, field);

                        if (!(field.IsLiteral || field.IsInitOnly))
                        {
                            FieldAutoMethod writer = new FieldAutoMethod(field, FieldAutoMethod.AccessType.Write);
                            yield return (writer, field);
                        }
                    }
                }
            }
        }

        private static bool GetCommandSupported(CommandData command, out string unsupportedReason)
        {
            for (int i = 0; i < command.ParamCount; i++)
            {
                ParameterInfo param = command.MethodParamData[i];
                Type paramType = param.ParameterType;

                if (!_parser.CanParse(paramType) && !paramType.IsGenericParameter)
                {
                    unsupportedReason = QuantumGlobal.Localization.Format(79, paramType);
                    return false;
                }
            }

            if (command.MonoTarget != MonoTargetType.Registry
                && !command.MethodData.IsStatic
                && !command.MethodData.DeclaringType.IsDerivedTypeOf(typeof(MonoBehaviour)))
            {
                unsupportedReason = QuantumGlobal.Localization.Format(80, command.MonoTarget);
                return false;
            }

            unsupportedReason = string.Empty;
            return true;
        }

        private static void LoadCommandsFromAssembly(Assembly assembly)
        {
            if (!_scanRuleset.ShouldScan(assembly))
            {
                return;
            }

            Type[] loadedTypes = assembly.GetTypes();
            foreach (Type type in loadedTypes)
            {
                try
                {
                    LoadCommandsFromType(type);
                }
                catch (TypeLoadException)
                {
                    // Issue under investigation

                    /*
                    if (loggingLevel >= LoggingLevel.Warnings)
                    {
                        Logs.Warning($"Unable to extract command data from type {type} in assembly {assembly.GetName().Name} as it may be corrupted. The following exception was thrown: {e.Message}");
                    }
                    */
                }
                catch (BadImageFormatException)
                {
                    // Confirmed to be an issue on Unity/Mono's side
                    // Extremely unlikely that it will ever occur in user code, so for this reason it is ignored silently
                    // QC Issue: https://bitbucket.org/QFSW/quantum-console/issues/67/add-protection-against-corrupt-dlls
                    // Unity Issue: https://issuetracker.unity3d.com/issues/badimageformatexception-is-thrown-when-calling-getcustomattributes-on-certain-memberinfo-instances
                    // Mono Issue: https://github.com/mono/mono/issues/17278

                    /*
                    if (loggingLevel >= LoggingLevel.Warnings)
                    {
                        Logs.Warning($"Unable to extract command data from type {type} in assembly {assembly.GetName().Name} as it may be corrupted. The following exception was thrown: {e.Message}");
                    }
                    */
                }
            }
        }

        private static void LoadCommandsFromType(Type type)
        {
            if (!_scanRuleset.ShouldScan(type))
            {
                return;
            }

            foreach ((MethodInfo method, MemberInfo member) in ExtractCommandMethods(type))
            {
                if (member.DeclaringType == type)
                {
                    LoadCommandsFromMember(member, method);
                }
            }
        }

        private static void LoadCommandsFromMember(MemberInfo member, MethodInfo method)
        {
            if (!_scanRuleset.ShouldScan(member))
            {
                return;
            }

            IEnumerable<CommandAttribute> commandAttributes = member.GetCustomAttributes<CommandAttribute>();
            CommandDescriptionAttribute descriptionAttribute = member.GetCustomAttribute<CommandDescriptionAttribute>();

            foreach (CommandAttribute commandAttribute in commandAttributes)
            {
                if (!commandAttribute.Valid)
                {
                    if (loggingLevel >= LoggingLevel.Warnings)
                    {
                        Logs.Warning(QuantumGlobal.Localization.Format(81, commandAttribute.Alias));
                    }
                }
                else
                {
                    CommandPlatformAttribute platformAttribute = member.GetCustomAttribute<CommandPlatformAttribute>();
                    Platform commandPlatforms = platformAttribute?.SupportedPlatforms ?? commandAttribute.SupportedPlatforms;
                    if (commandPlatforms.HasFlag(Application.platform.ToPlatform()))
                    {
                        IEnumerable<CommandData> newCommands = CreateCommandOverloads(method, commandAttribute, descriptionAttribute);
                        foreach (CommandData command in newCommands)
                        {
                            TryAddCommand(command);
                        }
                    }
                }
            }
        }

        private static IEnumerable<CommandData> CreateCommandOverloads(MethodInfo method, CommandAttribute commandAttribute, CommandDescriptionAttribute descriptionAttribute)
        {
            int defaultParameters = method.GetParameters().Count(x => x.HasDefaultValue);
            for (int i = 0; i < defaultParameters + 1; i++)
            {
                CommandData command = new CommandData(method, commandAttribute, descriptionAttribute, i);
                yield return command;
            }
        }

        private static string GenerateCommandKey(CommandData command)
        {
            string cmdName = command.CommandName;
            return command.HasParamsArgument
                ? $"{cmdName}(#)"
                : $"{cmdName}({command.ParamCount})";
        }

        /// <summary>
        /// Registers a new command.
        /// </summary>
        /// <param name="command">The command to register.</param>
        /// <returns>If the addition was successful.</returns>
        public static bool TryAddCommand(CommandData command)
        {
            if (!GetCommandSupported(command, out string reason))
            {
                if (loggingLevel >= LoggingLevel.Warnings)
                {
                    Logs.Warning(QuantumGlobal.Localization.Format(82, command.CommandSignature, command.MethodData.DeclaringType.GetDisplayName(), reason));
                }

                return false;
            }

            string key = GenerateCommandKey(command);
            bool alreadyExists = !_commandTable.TryAdd(key, command);

            if (alreadyExists)
            {
                if (loggingLevel >= LoggingLevel.Warnings)
                {
                    string fullMethodName = $"{command.MethodData.DeclaringType.FullName}.{command.MethodData.Name}";
                    Logs.Warning(QuantumGlobal.Localization.Format(83, fullMethodName, key));
                }

                return false;
            }

            _commandCacheDirty = true;
            Interlocked.Increment(ref _loadedCommandCount);
            return true;
        }

        /// <summary>
        /// Removes an existing command.
        /// </summary>
        /// <param name="command">The command to remove.</param>
        /// <returns>If the removal was successful.</returns>
        public static bool TryRemoveCommand(CommandData command)
        {
            string key = GenerateCommandKey(command);
            if (_commandTable.TryRemove(key, out _))
            {
                _commandCacheDirty = true;
                Interlocked.Decrement(ref _loadedCommandCount);
                return true;
            }

            return false;
        }
        #endregion

        #region Command Invocation
        /// <summary>Invokes a command on the QuantumConsoleProcessor.</summary>
        /// <returns>Return value of the invocation.</returns>
        /// <param name="commandString">The command to invoke.</param>
        public static object InvokeCommand(string commandString)
        {
            GenerateCommandTable();

            commandString = commandString.Trim();
            commandString = _preprocessor.Process(commandString);

            if (string.IsNullOrWhiteSpace(commandString)) { throw new ArgumentException(QuantumGlobal.Localization.Get(84)); }
            string[] commandParts = commandString.SplitScoped(' ');
            commandParts = commandParts.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            string commandName = commandParts[0];
            string[] commandParams = commandParts.SubArray(1, commandParts.Length - 1);
            int paramCount = commandParams.Length;

            string[] commandNameParts = commandName.Split(new[] { '<' }, 2);
            string genericSignature = commandNameParts.Length > 1 ? $"<{commandNameParts[1]}" : "";
            commandName = commandNameParts[0];

            CommandData command = FindCompatibleCommand(commandName, paramCount);
            Type[] genericTypes = ParseGenericTypes(command, genericSignature);

            // If we using a command with a dynamic parameter count, then reconstitute all the extra params
            // back into one parameter for the parser
            if (command.HasParamsArgument)
            {
                int paramsIndex = command.ParamCount - 1;
                IEnumerable<string> paramsParts = commandParams.Skip(paramsIndex);
                string paramsMerged = string.Join(",", paramsParts);

                string[] mergedCommandParams = new string[command.ParamCount];
                Array.Copy(commandParams, mergedCommandParams, command.ParamCount - 1);
                mergedCommandParams[paramsIndex] = paramsMerged;

                commandParams = mergedCommandParams;
            }

#if !UNITY_EDITOR && ENABLE_IL2CPP && !UNITY_2022_2_OR_NEWER
            if (genericTypes.Any((Type x) => x.IsValueType))
            {
                throw new NotSupportedException(QuantumGlobal.Localization.Get(89));
            }
#endif

            object[] parsedCommandParams = ParseParamData(command.MakeGenericArguments(genericTypes), commandParams);
            return command.Invoke(parsedCommandParams, genericTypes);
        }

        private static CommandData FindCompatibleCommand(string commandName, int paramCount)
        {
            CommandData commandCandidate;

            // First try finding an exact match
            string exactKey = $"{commandName}({paramCount})";
            if (_commandTable.TryGetValue(exactKey, out commandCandidate))
            {
                return commandCandidate;
            }

            // If that fails, try checking commands with dynamic parameter counts
            string dynamicKey = $"{commandName}(#)";
            if (_commandTable.TryGetValue(dynamicKey, out commandCandidate))
            {
                // Verify they have compatible parameter counts
                int commandRequiredParams = commandCandidate.ParamCount - 1;
                if (paramCount >= commandRequiredParams)
                {
                    return commandCandidate;
                }
            }

            // All failed, throw error
            bool overloadExists = _commandCache.Any(command => command.CommandName == commandName);
            throw overloadExists
                ? new ArgumentException(QuantumGlobal.Localization.Format(85, commandName, paramCount))
                : new ArgumentException(QuantumGlobal.Localization.Format(86, commandName));
        }

        private static Type[] ParseGenericTypes(CommandData command, string genericSignature)
        {
            // Handle non generic commands first
            if (!command.IsGeneric)
            {
                if (genericSignature != string.Empty)
                {
                    throw new ArgumentException(QuantumGlobal.Localization.Format(88, command.CommandName));
                }

                return Array.Empty<Type>();
            }

            // Validate the correct number of generics
            int expectedArgCount = command.GenericParamTypes.Length;
            string[] genericArgNames = genericSignature.ReduceScope('<', '>').SplitScoped(',');
            if (genericArgNames.Length != expectedArgCount)
            {
                throw new ArgumentException(QuantumGlobal.Localization.Format(87, command.CommandName, expectedArgCount, expectedArgCount == 1 ? "" : "s", genericArgNames.Length));
            }

            // Parse the actual types
            Type[] genericTypes = new Type[genericArgNames.Length];
            for (int i = 0; i < genericTypes.Length; i++)
            {
                genericTypes[i] = QuantumParser.ParseType(genericArgNames[i]);
            }

            return genericTypes;
        }

        private static object[] ParseParamData(Type[] paramTypes, string[] paramData)
        {
            object[] parsedData = new object[paramData.Length];
            for (int i = 0; i < parsedData.Length; i++)
            {
                parsedData[i] = _parser.Parse(paramData[i], paramTypes[i]);
            }

            return parsedData;
        }
        #endregion
    }
}
