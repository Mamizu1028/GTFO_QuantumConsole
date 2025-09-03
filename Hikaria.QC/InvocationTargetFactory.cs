using Hikaria.QC.Comparators;
using Hikaria.QC.Utilities;
using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hikaria.QC
{
    public static class InvocationTargetFactory
    {
        private static readonly Dictionary<(MonoTargetType, Type), object> TargetCache = new Dictionary<(MonoTargetType, Type), object>();

        public static IEnumerable<T> FindTargets<T>(MonoTargetType method) where T : MonoBehaviour
        {
            foreach (object target in FindTargets(typeof(T), method))
            {
                yield return target as T;
            }
        }

        public static IEnumerable<object> FindTargets(Type classType, MonoTargetType method)
        {
            switch (method)
            {
                case MonoTargetType.Single:
                {
#if UNITY_6000_0_OR_NEWER
                    Object target = Object.FindFirstObjectByType(classType);
#else
                    Object target = Object.FindObjectOfType(Il2CppType.From(classType));
#endif
                    return target == null ? Enumerable.Empty<object>() : target.Yield();
                }
                case MonoTargetType.SingleInactive:
                {
                    return WrapSingleCached(classType, method, type =>
                    {
                        return Resources.FindObjectsOfTypeAll(Il2CppType.From(type))
                            .FirstOrDefault(x => !x.hideFlags.HasFlag(HideFlags.HideInHierarchy));
                    });
                }
                case MonoTargetType.All:
                {
#if UNITY_6000_0_OR_NEWER
                    return Object.FindObjectsByType(classType, FindObjectsSortMode.None)
#else
                    return Object.FindObjectsOfType(Il2CppType.From(classType))
#endif
                        .OrderBy(x => x.name, new AlphanumComparator());
                }
                case MonoTargetType.AllInactive:
                {
                    return Resources.FindObjectsOfTypeAll(Il2CppType.From(classType))
                        .Where(x => !x.hideFlags.HasFlag(HideFlags.HideInHierarchy))
                        .OrderBy(x => x.name, new AlphanumComparator());
                }
                case MonoTargetType.Registry:
                {
                    return QuantumRegistry.GetRegistryContents(classType);
                }
                case MonoTargetType.Singleton:
                {
                    return GetSingletonInstance(classType).Yield();
                }
                default:
                {
                    throw new ArgumentException(QuantumGlobal.Localization.Format(73, method));
                }
            }
        }

        private static IEnumerable<object> WrapSingleCached(Type classType, MonoTargetType method, Func<Type, object> targetFinder)
        {
            if (!TargetCache.TryGetValue((method, classType), out object target) || target as Object == null)
            {
                target = targetFinder(classType);
                TargetCache[(method, classType)] = target;
            }

            return target == null ? Enumerable.Empty<object>() : target.Yield();
        }

        public static object InvokeOnTargets(MethodInfo invokingMethod, IEnumerable<object> targets, object[] arguments)
        {
            int returnCount = 0;
            int invokeCount = 0;
            Dictionary<object, object> resultsParts = new Dictionary<object, object>();

            foreach (object target in targets)
            {
                invokeCount++;
                object result = invokingMethod.Invoke(target, arguments);

                if (result != null)
                {
                    resultsParts.Add(target, result);
                    returnCount++;
                }
            }

            if (returnCount > 1)
            {
                return resultsParts;
            }

            if (returnCount == 1)
            {
                return resultsParts.Values.First();
            }

            if (invokeCount == 0)
            {
                string typeName = invokingMethod.DeclaringType.GetDisplayName();
                throw new Exception(QuantumGlobal.Localization.Format(74, typeName));
            }

            return null;
        }

        private static string FormatInvocationMessage(int invocationCount, object lastTarget = null)
        {
            switch (invocationCount)
            {
                case 0:
                    throw new Exception(QuantumGlobal.Localization.Get(75));
                case 1:
                {
                    string name;
                    if (lastTarget is Object obj)
                    {
                        name = obj.name;
                    }
                    else
                    {
                        name = lastTarget?.ToString();
                    }
                    return QuantumGlobal.Localization.Format(76, name);
                }
                default:
                    return QuantumGlobal.Localization.Format(77, invocationCount);
            }
        }

        private static object GetSingletonInstance(Type classType)
        {
            if (QuantumRegistry.GetRegistrySize(classType) > 0)
            {
                return QuantumRegistry.GetRegistryContents(classType).First();
            }

            object target = CreateCommandSingletonInstance(classType);
            QuantumRegistry.RegisterObject(classType, target);

            return target;
        }

        private static Component CreateCommandSingletonInstance(Type classType)
        {
            GameObject obj = new GameObject($"{classType}Singleton");
            Object.DontDestroyOnLoad(obj);
            return obj.AddComponent(Il2CppType.From(classType, true)).Cast<Component>();
        }
    }
}
