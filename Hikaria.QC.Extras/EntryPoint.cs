using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.Localization;
using TheArchive.Interfaces;
using TheArchive.Loader;

namespace Hikaria.QC.Extras;

[ArchiveDependency(QuantumGlobal.GUID)]
[ArchiveModule(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
internal class EntryPoint : IArchiveModule
{
    public ILocalizationService LocalizationService { get; set; }
    public IArchiveLogger Logger { get; set; }

    public void Init()
    {
        Logs.Setup(Logger);

        LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<CoroutineCommands>();
        LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<KeyBinderModule>();

        QuantumConsole.QuantumConsoleReady += (console) =>
        {
            var obj = console.gameObject;
            QuantumRegistry.RegisterObject(obj.AddComponent<CoroutineCommands>());
            QuantumRegistry.RegisterObject(obj.AddComponent<KeyBinderModule>());
        };
    }
}
