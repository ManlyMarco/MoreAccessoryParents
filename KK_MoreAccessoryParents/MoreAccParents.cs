using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using ChaCustom;
using HarmonyLib;
using MoreAccessoriesKOI;
using UniRx;

namespace KK_MoreAccessoryParents
{
    [BepInDependency("com.joan6694.illusionplugins.moreaccessories")]
    [BepInPlugin(GUID, "More Accessory Parents", Version)]
    public partial class MoreAccParents : BaseUnityPlugin
    {
        public const string GUID = "marco.MoreAccParents";
        public const string Version = "1.0";
        internal new static ManualLogSource Logger;
        private static readonly MethodInfo GetCvsAccessory = AccessTools.Method(typeof(MoreAccessories), "GetCvsAccessory");

        private static MoreAccParents _instance;

        private void Start()
        {
            _instance = this;
            Logger = base.Logger;

            Hooks.Initialize();
        }

        private static void OnMakerStart()
        {
            _instance.StartCoroutine(OnMakerStartCo());
        }

        private static IEnumerator OnMakerStartCo()
        {
            for (var i = 0; i < 5; i++)
                yield return null;

            Interface.CreateInterface();

            Interface.Selection.Subscribe(
                Observer.Create<SelectionChangedInfo>(
                    info => SetCurrentAccessoryParent(info.AccessoryParentKey)));
        }

        private static void OnMakerExit()
        {
            Interface.DestroyInterface();
        }

        private static void SetCurrentAccessoryParent(ChaAccessoryDefine.AccessoryParentKey accessoryParentKey)
        {
            var window = FindObjectOfType<CustomAcsParentWindow>();
            var cat = typeof(CustomAcsParentWindow);

            if (!(bool)AccessTools.Field(cat, "updateWin").GetValue(window))
            {
                var selAcc = (CvsAccessory)GetCvsAccessory.Invoke(MoreAccessories._self, new object[] { (int)window.slotNo });
                selAcc.UpdateSelectAccessoryParent((int)accessoryParentKey - 1);
            }
        }
    }
}
