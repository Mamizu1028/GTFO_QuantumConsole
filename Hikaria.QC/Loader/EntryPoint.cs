using TheArchive.Core;
using TheArchive.Core.FeaturesAPI;

namespace Hikaria.QC.Loader
{
    public class EntryPoint : IArchiveModule
    {
        public bool ApplyHarmonyPatches => false;

        public bool UsesLegacyPatches => false;

        public ArchiveLegacyPatcher Patcher { get; set; }

        public string ModuleGroup => FeatureGroups.GetOrCreateModuleGroup("Quantum Console");

        public void Init()
        {
        }

        public void OnExit()
        {
        }

        public void OnLateUpdate()
        {
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
        }
    }
}
