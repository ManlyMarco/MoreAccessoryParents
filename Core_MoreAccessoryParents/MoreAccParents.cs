using System.Collections;
using BepInEx;
using BepInEx.Logging;
using ChaCustom;
using KKAPI.Maker;
using UniRx;

namespace KK_MoreAccessoryParents
{
    [BepInPlugin(GUID, "More Accessory Parents", Version)]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class MoreAccParents : BaseUnityPlugin
    {
        public const string GUID = "marco.MoreAccParents";
        public const string Version = "1.2";
        internal static new ManualLogSource Logger;

        private void Start()
        {
            Logger = base.Logger;

            Hooks.Initialize();
        }
    }
}
