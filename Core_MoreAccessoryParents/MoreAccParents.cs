using BepInEx;
using BepInEx.Logging;

namespace KK_MoreAccessoryParents
{
    [BepInPlugin(GUID, "More Accessory Parents", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class MoreAccParents : BaseUnityPlugin
    {
        public const string GUID = "marco.MoreAccParents";
        public const string Version = "2.0";
        internal static new ManualLogSource Logger;

        private void Start()
        {
            Logger = base.Logger;

            Hooks.Initialize();
        }
    }
}
