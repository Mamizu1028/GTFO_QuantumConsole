using Il2CppInterop.Runtime.Attributes;
using System;
using TheArchive.Loader;
using UnityEngine;

namespace Hikaria.QC
{
    [Serializable]
    [Il2CppImplements(typeof(ISerializationCallbackReceiver))]
    public abstract class TypeFormatter : Il2CppSystem.Object
    {
        public Type Type { get; private set; }
        private string _type;

        protected TypeFormatter(Type type) : base(LoaderWrapper.ClassInjector.DerivedConstructorPointer<TypeFormatter>()) 
        {
            Type = type;
            LoaderWrapper.ClassInjector.DerivedConstructorBody(this);
        }

        public TypeFormatter(IntPtr ptr) : base(ptr) { }

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
        public TypeColorFormatter(IntPtr ptr) : base(ptr) { }
    }

    [Serializable]
    public class CollectionFormatter : TypeFormatter
    {
        public string SeperatorString = ",";
        public string LeftScoper = "[";
        public string RightScoper = "]";

        public CollectionFormatter(Type type) : base(type) { }
        public CollectionFormatter(IntPtr ptr) : base(ptr) { }
    }
}