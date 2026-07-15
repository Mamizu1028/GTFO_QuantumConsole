using Clonesoft.Json;
using Clonesoft.Json.Converters;
using Clonesoft.Json.Serialization;
using Hikaria.QC.Utilities;
using TheArchive.Core;
using TheArchive.Core.Localization;

namespace Hikaria.QC;

public static class QuantumGlobal
{
    public const string GUID = "Hikaria.QuantumConsole";

    public const string NAME = "QuantumConsole";

    public const string VERSION = "1.1.0";

    public const string QC_VERSION = "2.6.7";

    internal static ILocalizationService Localization { get; private set; }

    internal static JsonSerializerSettings JsonSerializerSettings { get; private set; }

    internal static void Setup(IArchiveModule module)
    {
        Logs.Setup(module.Logger);

        Localization = module.LocalizationService;

        JsonSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
            ContractResolver = new DefaultContractResolver(),
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            DateFormatString = "yyyy-MM-dd HH:mm:ss",

        };
        JsonSerializerSettings.Converters.Add(new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
        JsonSerializerSettings.Converters.Add(new StringEnumConverter());
    }
}
