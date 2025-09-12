using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;

namespace Hikaria.QC;

[ArchiveDependency(EnhancedScroller.PluginInfo.GUID)]
[ArchiveModule(QuantumGlobal.GUID, QuantumGlobal.NAME, QuantumGlobal.VERSION)]
internal class EntryPoint : IArchiveModule
{
    public ILocalizationService LocalizationService { get; set; }

    public IArchiveLogger Logger { get; set; }

    public void Init()
    {
        QuantumGlobal.Setup(this);
    }
}
