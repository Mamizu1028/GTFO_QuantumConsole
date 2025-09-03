using System.Collections.Generic;

namespace Hikaria.QC.Localization;

internal class CommandLocalizationData
{
    public string Description { get; set; }

    public Dictionary<string, string> ParameterDescriptions { get; set; }
}
