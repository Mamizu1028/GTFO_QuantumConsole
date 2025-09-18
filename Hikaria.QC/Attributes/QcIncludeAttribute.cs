using System;

namespace Hikaria.QC
{
    /// <summary>
    /// Instructs QC to include this entity when scanning the code base for commands.
    /// This can be used to optimise QCs loading times in large codebases when there are large entities that do not have any commands present.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class QcIncludeAttribute : Attribute { }
}