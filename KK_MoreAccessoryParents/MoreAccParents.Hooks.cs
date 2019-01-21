using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using ChaCustom;
using Harmony;
using UnityEngine;
using UnityEngine.UI;
using Logger = BepInEx.Logger;

namespace KK_MoreAccessoryParents
{
    public partial class MoreAccParents
    {
        internal static class Hooks
        {
            private static readonly Type EnumAccesoryParentKeyType = typeof(ChaAccessoryDefine.AccessoryParentKey);
            private static readonly Type EnumRefObjKeyType = typeof(ChaReference.RefObjKey);
            private static readonly string[] SelectParentHookCache;
            private static readonly FieldInfo FieldTglParent;
            internal static readonly int AccessoryParentKeyOriginalCount;
            internal static readonly int RefObjKeyOriginalCount;

            static Hooks()
            {
                FieldTglParent = AccessTools.Field(typeof(CustomAcsParentWindow), "tglParent");

                // Do this before hooking!
                SelectParentHookCache = (from key in Enum.GetNames(EnumAccesoryParentKeyType)
                                         where key != "none"
                                         select key).ToArray();

                AccessoryParentKeyOriginalCount = Enum.GetValues(EnumAccesoryParentKeyType).Length;
                RefObjKeyOriginalCount = Enum.GetValues(EnumRefObjKeyType).Length;
            }

            public static void Initialize()
            {
                var hi = HarmonyInstance.Create(GUID);
                hi.PatchAll(typeof(Hooks));

                UpdateChaAccessoryDefine();
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
            [HarmonyPatch(typeof(ChaAccessoryDefine), nameof(ChaAccessoryDefine.GetReverseParent), new[] { typeof(string) })]
            public static void GetReverseParentPrefix(string key, ref string __result)
            {
                if (__result == string.Empty)
                    __result = InterfaceEntries.FindReverseBone(key);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaAccessoryDefine), nameof(ChaAccessoryDefine.GetReverseParent), new[] { typeof(ChaAccessoryDefine.AccessoryParentKey) })]
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

            [HarmonyPatch(typeof(CustomScene), "Start")]
            [HarmonyPrefix]
            public static void CustomScene_Start()
            {
                if (Singleton<CustomBase>.Instance != null)
                    OnMakerStart();
            }

            [HarmonyPatch(typeof(CustomScene), "OnDestroy")]
            [HarmonyPrefix]
            public static void CustomScene_Destroy()
            {
                OnMakerExit();
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
            public static void GetNameHook(Type enumType, object value, string __result)
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
            [HarmonyPatch(typeof(Enum), nameof(Enum.Parse), new Type[] { typeof(Type), typeof(string), typeof(bool) })]
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
            [HarmonyPatch(typeof(ChaReference), nameof(ChaReference.CreateReferenceInfo))]
            public static void CreateReferenceInfoHook(ChaReference __instance, ulong flags, GameObject objRef)
            {
                if (null == objRef || (int)(flags - 1UL) != 0) return;

                CreateReferenceImpl(__instance, objRef);
            }

            private static void CreateReferenceImpl(ChaReference __instance, GameObject objRef)
            {
                var findAssist = new FindAssist();
                findAssist.Initialize(objRef.transform);

                var dict = (Dictionary<ChaReference.RefObjKey, GameObject>)
                    AccessTools.Field(typeof(ChaReference), "dictRefObj").GetValue(__instance);

                for (var i = 0; i < InterfaceEntries.AllBones.Length; i++)
                {
                    dict[(ChaReference.RefObjKey)(RefObjKeyOriginalCount + i)] =
                        findAssist.GetObjectFromName(InterfaceEntries.AllBones[i]);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(ChaReference), nameof(ChaReference.ReleaseRefObject))]
            public static void ReleaseRefObjectHook(ChaReference __instance, ulong flags)
            {
                if ((int)(flags - 1UL) != 0)
                    return;

                var dict = (Dictionary<ChaReference.RefObjKey, GameObject>)AccessTools.Field(typeof(ChaReference), "dictRefObj").GetValue(__instance);

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
                {
                    var toggles = (Toggle[])FieldTglParent.GetValue(__instance);
                    toggles[num].isOn = true;
                }
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
