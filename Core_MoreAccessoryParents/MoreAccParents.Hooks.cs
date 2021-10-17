using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using ChaCustom;
using HarmonyLib;
using KKAPI.Maker;
using UniRx;
using UnityEngine;

namespace KK_MoreAccessoryParents
{
    public partial class MoreAccParents
    {
        internal static class Hooks
        {
            private static Type EnumAccesoryParentKeyType = typeof(ChaAccessoryDefine.AccessoryParentKey);
            private static Type EnumRefObjKeyType = typeof(ChaReference.RefObjKey);
            private static string[] SelectParentHookCache;
            internal static int AccessoryParentKeyOriginalCount;
            internal static int RefObjKeyOriginalCount;

            public static void Initialize()
            {
                // Do this before hooking!
                SelectParentHookCache = (from key in Enum.GetNames(EnumAccesoryParentKeyType)
                                         where key != "none"
                                         select key).ToArray();
                AccessoryParentKeyOriginalCount = Enum.GetValues(EnumAccesoryParentKeyType).Length;
                RefObjKeyOriginalCount = Enum.GetValues(EnumRefObjKeyType).Length;

                Harmony.CreateAndPatchAll(typeof(Hooks));

                UpdateChaAccessoryDefine();

                MakerAPI.MakerBaseLoaded += (s, e) =>
                {
                    Interface.CreateInterface();

                    Interface.Selection.Subscribe(
                        Observer.Create<SelectionChangedInfo>(
                            info => SetCurrentAccessoryParent(info.AccessoryParentKey)));
                };
                MakerAPI.MakerExiting += (s, e) => Interface.DestroyInterface();
            }

            private static void SetCurrentAccessoryParent(ChaAccessoryDefine.AccessoryParentKey accessoryParentKey)
            {
                var window = FindObjectOfType<CustomAcsParentWindow>();
                if (!window.updateWin)
                {
                    var selAcc = window.cvsAccessory[(int)window.slotNo];
                    selAcc.UpdateSelectAccessoryParent((int)accessoryParentKey - 1);
                }
            }

            private static void UpdateChaAccessoryDefine()
            {
                // todo ChaAccessoryDefine.GetReverseParent
                var length = Enum.GetValues(EnumAccesoryParentKeyType).Length;
                var accNames = ChaAccessoryDefine.AccessoryParentName.Concat(InterfaceEntries.FancyBoneNames).ToArray();
                var num = accNames.Length;
                if (length == num)
                {
                    for (var j = 0; j < length; j++)
                    {
                        ChaAccessoryDefine.dictAccessoryParent[j] = accNames[j];
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Error, "Invalid ChaAccessoryDefine.AccessoryParentName or Enum.GetValues(typeof(ChaAccessoryDefine.AccessoryParentKey))");
#if(DEBUG)
                    throw new Exception();
#endif
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaAccessoryDefine), nameof(ChaAccessoryDefine.GetReverseParent), typeof(string))]
            public static void GetReverseParentPrefix(string key, ref string __result)
            {
                if (__result == string.Empty)
                    __result = InterfaceEntries.FindReverseBone(key);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaAccessoryDefine), nameof(ChaAccessoryDefine.GetReverseParent), typeof(ChaAccessoryDefine.AccessoryParentKey))]
            public static void GetReverseParentPrefix(ChaAccessoryDefine.AccessoryParentKey key, ref ChaAccessoryDefine.AccessoryParentKey __result)
            {
                if (__result == ChaAccessoryDefine.AccessoryParentKey.none)
                {
                    try
                    {
                        __result = (ChaAccessoryDefine.AccessoryParentKey)Enum.Parse(EnumAccesoryParentKeyType, InterfaceEntries.FindReverseBone(key.ToString()));
                    }
                    catch (Exception e)
                    {
                        Logger.Log(LogLevel.Error, e);
                    }
                }
            }

            /// <summary>
            /// Used to add new items to game enums
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Enum), nameof(Enum.GetValues))]
            public static void GetValuesHook(Type enumType, ref Array __result)
            {
                if (enumType == EnumAccesoryParentKeyType)
                {
                    var stock = new ArrayList(__result);
                    stock.AddRange(
                        Enumerable.Range(0, InterfaceEntries.AllBones.Length)
                            .Select(x => Enum.ToObject(enumType, AccessoryParentKeyOriginalCount + x))
                            .ToArray());
                    __result = stock.ToArray();
                }
                else if (enumType == EnumRefObjKeyType)
                {
                    var stock = new ArrayList(__result);
                    stock.AddRange(
                        Enumerable.Range(0, InterfaceEntries.AllBones.Length)
                            .Select(x => Enum.ToObject(enumType, RefObjKeyOriginalCount + x))
                            .ToArray());
                    __result = stock.ToArray();
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Enum), nameof(Enum.GetNames))]
            public static void GetNamesHook(Type enumType, ref string[] __result)
            {
                if (enumType == EnumAccesoryParentKeyType || enumType == EnumRefObjKeyType)
                {
                    var stock = new List<string>(__result);
                    stock.AddRange(InterfaceEntries.AllBones);
                    __result = stock.ToArray();
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Enum), nameof(Enum.GetName))]
            public static void GetNameHook(Type enumType, object value, ref string __result)
            {
                if (__result != null) return;

                if (enumType == EnumAccesoryParentKeyType)
                {
                    var index = Convert.ToInt32(value) - AccessoryParentKeyOriginalCount;
                    if (InterfaceEntries.AllBones.Length > index && index >= 0)
                        __result = InterfaceEntries.AllBones[index];
                }
                else if (enumType == EnumRefObjKeyType)
                {
                    var index = Convert.ToInt32(value) - RefObjKeyOriginalCount;
                    if (InterfaceEntries.AllBones.Length > index && index >= 0)
                        __result = InterfaceEntries.AllBones[index];
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Enum), nameof(Enum.Parse), typeof(Type), typeof(string), typeof(bool))]
            public static bool ParseHook(Type enumType, string value, bool ignoreCase, ref object __result)
            {
                if (enumType == EnumAccesoryParentKeyType)
                {
                    var index = Array.IndexOf(InterfaceEntries.AllBones, value);
                    if (index >= 0)
                    {
                        __result = Enum.ToObject(enumType, AccessoryParentKeyOriginalCount + index);
                        return false;
                    }
                }
                else if (enumType == EnumRefObjKeyType)
                {
                    var index = Array.IndexOf(InterfaceEntries.AllBones, value);
                    if (index >= 0)
                    {
                        __result = Enum.ToObject(enumType, RefObjKeyOriginalCount + index);
                        return false;
                    }
                }
                return true;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaReference), nameof(ChaReference.CreateReferenceInfo), typeof(ulong), typeof(GameObject))]
            public static void CreateReferenceInfoHook(ChaReference __instance, ulong flags, GameObject objRef)
            {
                if (null == objRef || (int)(flags - 1UL) != 0) return;

                var findAssist = new FindAssist();
                findAssist.Initialize(objRef.transform);

                var dict = __instance.dictRefObj;

                for (var i = 0; i < InterfaceEntries.AllBones.Length; i++)
                {
                    dict[(ChaReference.RefObjKey)(RefObjKeyOriginalCount + i)] = findAssist.GetObjectFromName(InterfaceEntries.AllBones[i]);
                }
            }

#if KKS
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaReference), nameof(ChaReference.CreateReferenceInfo), typeof(ulong), typeof(ChaLoad.ChaPreparationBodyBone.BoneInfo[]))]
            public static void CreateReferenceInfoHook(ChaReference __instance, ulong flags, ChaLoad.ChaPreparationBodyBone.BoneInfo[] boneInfos)
            {
                if (null == boneInfos || (int)(flags - 1UL) != 0) return;

                var dict = __instance.dictRefObj;

                for (var i = 0; i < InterfaceEntries.AllBones.Length; i++)
                {
                    dict[(ChaReference.RefObjKey)(RefObjKeyOriginalCount + i)] = boneInfos.First(x => x.name == InterfaceEntries.AllBones[i]).gameObject;
                }
            }
#endif

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaReference), nameof(ChaReference.ReleaseRefObject))]
            public static void ReleaseRefObjectHook(ChaReference __instance, ulong flags)
            {
                if ((int)(flags - 1UL) != 0)
                    return;

                var dict = __instance.dictRefObj;

                for (var i = 0; i < InterfaceEntries.AllBones.Length; i++)
                {
                    dict.Remove((ChaReference.RefObjKey)(RefObjKeyOriginalCount + i));
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CustomAcsParentWindow), nameof(CustomAcsParentWindow.SelectParent))]
            public static bool SelectParentHook(CustomAcsParentWindow __instance, string parentKey, ref int __result)
            {
                if (TrySetSelectedBone(parentKey, ref __result))
                    return false;

                // Fall back to stock logic
                var num = Array.IndexOf(SelectParentHookCache, parentKey);

                if (num != -1)
                    __instance.tglParent[num].isOn = true;

                __result = num;
                return false;
            }

            private static bool TrySetSelectedBone(string parentKey, ref int resultEnumId)
            {
                var myIndex = Array.IndexOf(InterfaceEntries.AllBones, parentKey);
                if (myIndex >= 0)
                {
                    Interface.SetByName(parentKey);

                    resultEnumId = AccessoryParentKeyOriginalCount - 1 + myIndex; // -1 skip none element
                    return true;
                }

                return false;
            }
        }
    }
}
