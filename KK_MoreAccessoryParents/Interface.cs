using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace KK_MoreAccessoryParents
{
    internal static class Interface
    {
        public static Subject<SelectionChangedInfo> Selection { get; private set; }

        private static readonly List<TMP_Dropdown> Dropdowns = new List<TMP_Dropdown>();
        private static readonly List<Toggle> Toggles = new List<Toggle>();

        public static void CreateInterface()
        {
            Selection = new Subject<SelectionChangedInfo>();
            Selection.Subscribe(OnUpdateInterface);

            var accw = GameObject.Find("AcsParentWindow");

            var windBack = accw.transform.Find("BasePanel/imgWindowBack");
            var windRt = windBack.GetComponent<RectTransform>();

            const int dropdownHeight = 32;

            windRt.offsetMin = new Vector2(windRt.offsetMin.x, windRt.offsetMin.y - 15 - dropdownHeight * InterfaceEntries.BoneList.Length);

            var toggleParent = accw.transform.Find("grpParent");

            CreateHeader(toggleParent);

            var originalToggle = toggleParent.Find("imgRbCol51");
            var toggleGroup = originalToggle.GetComponent<Toggle>().group;
            var originalDropdown = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/06_SystemTop/tglConfig/ConfigTop/ddRamp").transform;

            for (var i = 0; i < InterfaceEntries.BoneList.Length; i++)
            {
                var list = InterfaceEntries.BoneList[i];
                var offset = dropdownHeight * i;

                // Create the toggle
                var toggleCopy = Object.Instantiate(originalToggle, toggleParent, true);
                toggleCopy.name = "imgRbColAdditional" + i;
                var toggleRect = toggleCopy.GetComponent<RectTransform>();
                toggleRect.offsetMin = new Vector2(toggleRect.offsetMin.x, toggleRect.offsetMin.y - 53 - offset);
                toggleRect.offsetMax = new Vector2(toggleRect.offsetMax.x, toggleRect.offsetMax.y - 53 - offset);

                var newToggle = toggleRect.GetComponent<Toggle>();
                newToggle.group = toggleGroup;
                newToggle.onValueChanged.AddListener(
                    val =>
                    {
                        if (val)
                            OnSelectionChanged();
                    });
                var textMesh = toggleRect.GetComponentInChildren<TextMeshProUGUI>();
                textMesh.text = list.Name;
                newToggle.image.raycastTarget = true;

                Toggles.Add(newToggle);

                // Create the dropdown
                var dropdownCopy = Object.Instantiate(originalDropdown, toggleParent, false);
                dropdownCopy.gameObject.SetActive(false);
                dropdownCopy.name = "ddListAdditional" + i;
                var dropdownRt = dropdownCopy.GetComponent<RectTransform>();
                dropdownRt.anchorMin = Vector2.zero;
                dropdownRt.anchorMax = Vector2.zero;
                dropdownRt.offsetMin = new Vector2(0, -47 - offset);
                dropdownRt.offsetMax = new Vector2(380, -7 - offset);

                dropdownCopy.Find("textKindTitle").gameObject.SetActive(false);

                var dropdown = dropdownCopy.GetComponentInChildren<TMP_Dropdown>();
                dropdown.onValueChanged.RemoveAllListeners();
                dropdown.ClearOptions();
                dropdown.GetComponent<Image>().raycastTarget = true;

                dropdown.options.AddRange(list.GetBoneNames(true).Select(x => new TMP_Dropdown.OptionData(x)));
                dropdown.value = 0;
                var dropdownIndex = i;
                dropdown.onValueChanged.AddListener(_ =>
                {
                    if (Toggles[dropdownIndex].isOn)
                        OnSelectionChanged();
                    else
                        Toggles[dropdownIndex].isOn = true;
                });

                Dropdowns.Add(dropdown);

                dropdownCopy.gameObject.SetActive(true);
            }

            toggleGroup.StartCoroutine(FixDropdownFormat());
        }

        private static IEnumerator FixDropdownFormat()
        {
            foreach (var tmpDropdown in Dropdowns)
                tmpDropdown.transform.Find("Template").gameObject.SetActive(true);
            yield return null;
            foreach (var tmpDropdown in Dropdowns)
                tmpDropdown.transform.Find("Template").gameObject.SetActive(false);
        }

        private static void CreateHeader(Transform toggleParent)
        {
            var text = toggleParent.Find("textKokan");
            var copy = Object.Instantiate(text, toggleParent, true);
            copy.transform.name = "textAdditionalMod";
            var textRt = copy.GetComponent<RectTransform>();
            textRt.offsetMin = new Vector2(textRt.offsetMin.x, textRt.offsetMin.y - 45);
            textRt.offsetMax = new Vector2(textRt.offsetMax.x, textRt.offsetMax.y - 45);
            copy.GetComponentInChildren<TextMeshProUGUI>().text = "---------- Additional attachment points ----------";
            copy.gameObject.SetActive(true);
        }

        public static void DestroyInterface()
        {
            if (!Dropdowns.Any()) return;

            foreach (var dd in Dropdowns)
            {
                if (dd != null && dd.transform != null && dd.transform.parent != null)
                    Object.DestroyImmediate(dd.transform.parent.gameObject);
            }
            Dropdowns.Clear();

            foreach (var toggle in Toggles)
            {
                if (toggle != null)
                    Object.DestroyImmediate(toggle.gameObject);
            }
            Toggles.Clear();

            Object.DestroyImmediate(GameObject.Find("textAdditionalMod"));

            Selection.Dispose();
        }

        public static void SetByName(string boneName)
        {
            int FindIndex(IEnumerable<string> strings, string item)
            {
                var retVal = 0;
                foreach (var str in strings)
                {
                    if (str == item) return retVal;
                    retVal++;
                }
                return -1;
            }

            for (var groupIndex = 0; groupIndex < InterfaceEntries.BoneList.Length; groupIndex++)
            {
                var bones = InterfaceEntries.BoneList[groupIndex].GetBoneNames(false);
                var elementIndex = FindIndex(bones, boneName);
                if (elementIndex >= 0)
                {
                    Dropdowns[groupIndex].value = elementIndex;
                    return;
                }
            }
        }

        private static int GetApkEnumIndex(int groupIndex, int elementIndex)
        {
            return GetBoneEnumOffset(groupIndex, elementIndex) + MoreAccParents.Hooks.AccessoryParentKeyOriginalCount;
        }

        private static int GetRokEnumIndex(int groupIndex, int elementIndex)
        {
            return GetBoneEnumOffset(groupIndex, elementIndex) + MoreAccParents.Hooks.RefObjKeyOriginalCount;
        }

        private static int GetBoneEnumOffset(int groupIndex, int elementIndex)
        {
            var count = 0;
            for (var i = 0; i < groupIndex; i++)
                count += InterfaceEntries.BoneList[i].GetBoneNames(false).Count();

            count += elementIndex;
            return count;
        }

        private static bool GetBoneIndex(ChaAccessoryDefine.AccessoryParentKey accessoryParentKey, out int group, out int element)
        {
            var i = (int)accessoryParentKey - MoreAccParents.Hooks.AccessoryParentKeyOriginalCount;

            if (i >= 0)
            {
                for (var groupIndex = 0; groupIndex < InterfaceEntries.BoneList.Length; groupIndex++)
                {
                    var boneLen = InterfaceEntries.BoneList[groupIndex].GetBoneNames(false).Count();
                    if (boneLen > i)
                    {
                        group = groupIndex;
                        element = i;
                        return true;
                    }

                    i -= boneLen;
                }
            }

            group = 0;
            element = 0;
            return false;
        }

        private static void OnSelectionChanged()
        {
            var i = Toggles.FindIndex(toggle => toggle.isOn);
            if (i < 0)
                return;

            var i2 = Dropdowns[i].value;

            Selection.OnNext(new SelectionChangedInfo(GetApkEnumIndex(i, i2), GetRokEnumIndex(i, i2)));
        }

        private static void OnUpdateInterface(SelectionChangedInfo value)
        {
            if (!GetBoneIndex(value.AccessoryParentKey, out var ddIndex, out var elementIndex))
                return;

            Dropdowns[ddIndex].value = elementIndex;
            Toggles[ddIndex].isOn = true;
        }
    }
}
