using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;

namespace Hikaria.QC;

[ArchiveModule(QuantumGlobal.GUID, QuantumGlobal.NAME, QuantumGlobal.VERSION)]
public class EntryPoint : IArchiveModule
{
    public string ModuleGroup => QuantumGlobal.GUID;

    public ILocalizationService LocalizationService { get; set; }

    public IArchiveLogger Logger { get; set; }

    public void Init()
    {
        QuantumGlobal.Setup(this);
    }
}
