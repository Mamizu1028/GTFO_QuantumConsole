using System;
using UnityEngine;
using UnityEngine.Serialization;
using Il2CppInterop.Runtime.Attributes;


#region Preserve Fix
#if UNITY_2018_4_OR_NEWER
using UnityEngine.Scripting;
#else
/// <summary>
///   <para>PreserveAttribute prevents byte code stripping from removing a class, method, field, or property.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
public sealed class PreserveAttribute : Attribute
{
}
#endif
#endregion

namespace Hikaria.QC
{
    [Serializable]
    [Il2CppImplements(typeof(ISerializationCallbackReceiver))]
    public abstract class TypeFormatter
    {
        public Type Type { get; private set; }
        private string _type;

        protected TypeFormatter(Type type) { Type = type; }

        public void OnAfterDeserialize()
        {
            Type = Type.GetType(_type, false);
            if (Type == null) { Type = QuantumParser.ParseType(_type.Split(',')[0]); }
        }

        public void OnBeforeSerialize()
        {
            if (Type != null) { _type = Type.AssemblyQualifiedName; }
        }
    }

    [Serializable]
    public class TypeColorFormatter : TypeFormatter
    {
        public Color Color = Color.white;

        public TypeColorFormatter(Type type) : base(type) { }
    }

    [Serializable]
    public class CollectionFormatter : TypeFormatter
    {
        public string SeperatorString = ",";
        public string LeftScoper = "[";
        public string RightScoper = "]";

        public CollectionFormatter(Type type) : base(type) { }
    }
}