using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GuildMaster.Runtime.UI.Legacy;
using GuildMaster.Runtime.UI.Headquarters;
using System.IO;

namespace GuildMaster.Editor.UI.Headquarters
{
    /// <summary>
    /// Phase 5A (Claude redesign): rebuilds QuartersDialog.prefab and LegacyCurrencyView.prefab
    /// with a clean, legacy-accurate visual hierarchy. Backend flow (QuartersDialog.cs) is
    /// untouched — this file only changes construction/layout/sizing.
    ///
    /// Fixes over the previous version:
    /// - Dialog root had no real size (LegacyDialogFrame's base RectTransform is 100x100 with no
    ///   ContentSizeFitter) — content was being laid out inside a near-invisible box. Now sized
    ///   explicitly to 720x620, matching the other Phase 4/5 popup conventions.
    /// - Currency coin icons pointed at "Assets/_Game/Art/UI/Generated/coin_*.png", which does not
    ///   exist (that folder holds Phase 2's generated border shapes, not Phase 1's currency art).
    ///   Real coin sprites live in Assets/_Game/Art/Legacy/Currency/ — now loaded through
    ///   LegacySpriteRegistry, the same registry every other screen uses.
    /// - "UPGRADE" was printed twice (once as a heading, once as the button label) — merged into
    ///   one clear benefit line; the button now only ever says "Upgrade" or "Max".
    /// - No fixed section separation between capacity/benefit/cost/actions — rebuilt as distinct
    ///   rows with consistent spacing so it reads top-to-bottom instead of a cramped stack.
    /// </summary>
    public static class QuartersDialogBuilder
    {
        private static readonly Vector2 DialogSize = new Vector2(720, 620);

        [MenuItem("Tools/Guild Master/Legacy UI/Build Quarters Dialog")]
        public static void BuildQuartersDialog()
        {
            string currencyPrefabPath = BuildCurrencyViewPrefab();
            BuildDialogPrefab(currencyPrefabPath);
            Debug.Log("[QuartersDialogBuilder] QuartersDialog + LegacyCurrencyView prefabs rebuilt (Phase 5A redesign).");
        }

        // ─── Currency row (Platinum/Gold/Silver/Copper, hides leading zero tiers) ────

        private static string BuildCurrencyViewPrefab()
        {
            string path = "Assets/_Game/Prefabs/UI/Legacy/LegacyCurrencyView.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            GameObject root = new GameObject("LegacyCurrencyView", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LegacyCurrencyView));
            var hlg = root.GetComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 24;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var fitter = root.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var view = root.GetComponent<LegacyCurrencyView>();
            string[] coinTypes = { "platinum", "gold", "silver", "copper" };
            var groups = new GameObject[4];
            var texts = new Text[4];

            for (int i = 0; i < 4; i++)
            {
                groups[i] = new GameObject($"{coinTypes[i]}_group", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                groups[i].transform.SetParent(root.transform, false);
                var subHlg = groups[i].GetComponent<HorizontalLayoutGroup>();
                subHlg.childAlignment = TextAnchor.MiddleLeft;
                subHlg.spacing = 6;
                subHlg.childControlWidth = false;
                subHlg.childControlHeight = false;
                groups[i].GetComponent<LayoutElement>().preferredHeight = 44;

                var iconGo = new GameObject($"{coinTypes[i]}_icon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(groups[i].transform, false);
                var img = iconGo.GetComponent<Image>();
                img.sprite = LegacySpriteRegistry.GetSprite($"coin_{coinTypes[i]}");
                img.preserveAspect = true;
                iconGo.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);

                var textGo = new GameObject($"{coinTypes[i]}_text", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
                textGo.transform.SetParent(groups[i].transform, false);
                var txt = textGo.GetComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 32;
                txt.color = LegacyUITheme.DimWhite;
                txt.fontStyle = FontStyle.Bold;
                txt.alignment = TextAnchor.MiddleLeft;
                txt.text = "0";
                textGo.GetComponent<LayoutElement>().preferredWidth = 60;

                texts[i] = txt;
            }

            var so = new SerializedObject(view);
            so.FindProperty("_platinumGroup").objectReferenceValue = groups[0];
            so.FindProperty("_platinumText").objectReferenceValue = texts[0];
            so.FindProperty("_goldGroup").objectReferenceValue = groups[1];
            so.FindProperty("_goldText").objectReferenceValue = texts[1];
            so.FindProperty("_silverGroup").objectReferenceValue = groups[2];
            so.FindProperty("_silverText").objectReferenceValue = texts[2];
            so.FindProperty("_copperGroup").objectReferenceValue = groups[3];
            so.FindProperty("_copperText").objectReferenceValue = texts[3];
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return path;
        }

        // ─── Dialog ──────────────────────────────────────────────────────────

        private static void BuildDialogPrefab(string currencyPrefabPath)
        {
            string dialogFramePath = "Assets/_Game/Prefabs/UI/Legacy/LegacyDialogFrame.prefab";
            GameObject framePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dialogFramePath);
            if (framePrefab == null)
            {
                Debug.LogError("[QuartersDialogBuilder] LegacyDialogFrame.prefab not found — run Phase 2's theme builder first.");
                return;
            }

            GameObject dialog = (GameObject)PrefabUtility.InstantiatePrefab(framePrefab);
            dialog.name = "QuartersDialog";

            var dialogRt = dialog.GetComponent<RectTransform>();
            // Let ContentSizeFitter own the vertical size. Keeping a fixed height here can
            // preserve a stale design-time height and make the last action overlap when the
            // nested upgrade section changes its preferred height at runtime.
            dialogRt.sizeDelta = new Vector2(DialogSize.x, 0f);
            dialogRt.anchorMin = dialogRt.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRt.anchoredPosition = Vector2.zero;

            // Width is fixed (720), but height must match actual content exactly — a hardcoded
            // height caused the previous build's Close button to overlap the Upgrade button
            // whenever content ran taller than the guessed size. ContentSizeFitter recalculates
            // height from the VerticalLayoutGroup's real total every time content changes.
            var fitter = dialog.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = dialog.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var vlg = dialog.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = dialog.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(48, 48, 40, 40);
            vlg.spacing = 28;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Title
            Text titleTxt = AddText(dialog.transform, "Title", "Quarters", 44, LegacyUITheme.DimWhite, FontStyle.Bold, TextAnchor.MiddleCenter, 60);

            // Capacity — the primary stat, given real visual weight (large, high-contrast)
            Text descTxt = AddText(dialog.transform, "CapacityText", "Capacity: 0", 36, LegacyUITheme.BrassBorder, FontStyle.Bold, TextAnchor.MiddleCenter, 50);

            // Divider spacer so the capacity readout doesn't crowd into the upgrade section
            AddSpacer(dialog.transform, 4);

            // Upgrade section — single bordered card containing benefit + cost + button, so it
            // reads as one coherent action rather than loose floating rows.
            GameObject upgCard = new GameObject("UpgradeSection", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement), typeof(ContentSizeFitter));
            upgCard.transform.SetParent(dialog.transform, false);
            var upgImg = upgCard.GetComponent<Image>();
            upgImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/UI/Generated/object_border_dim_white.png");
            upgImg.type = Image.Type.Sliced;
            var upgVlg = upgCard.GetComponent<VerticalLayoutGroup>();
            upgVlg.padding = new RectOffset(28, 28, 24, 24);
            upgVlg.spacing = 18;
            upgVlg.childAlignment = TextAnchor.UpperCenter;
            upgVlg.childControlWidth = true;
            upgVlg.childControlHeight = true;
            upgVlg.childForceExpandWidth = true;
            upgVlg.childForceExpandHeight = false;
            upgCard.GetComponent<LayoutElement>().flexibleHeight = 0;
            // Nested layout groups don't automatically report their real height to the outer
            // VerticalLayoutGroup unless they also fit-to-content themselves — without this, the
            // outer group used a stale/incorrect height for this card, which pushed the Close
            // button up on top of the Upgrade button instead of below it.
            var upgFitter = upgCard.GetComponent<ContentSizeFitter>();
            upgFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            upgFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Text upgDescTxt = AddText(upgCard.transform, "UpgradeBenefit", "Increases the maximum number of adventurers you can recruit.", 26, LegacyUITheme.DimWhite, FontStyle.Normal, TextAnchor.MiddleCenter, 72);
            upgDescTxt.horizontalOverflow = HorizontalWrapMode.Wrap;

            GameObject instCurrency = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(currencyPrefabPath), upgCard.transform);
            instCurrency.AddComponent<LayoutElement>().preferredHeight = 48;

            // Upgrade button — primary action, brass-bordered to stand out
            GameObject upgBtn = InstantiateButton(upgCard.transform, "UpgradeButton", "Upgrade", out Text upgBtnText);
            upgBtn.GetComponent<LayoutElement>().preferredHeight = 84;

            AddSpacer(dialog.transform, 8);

            // Close — secondary action, visually smaller/lower emphasis than Upgrade
            GameObject closeBtn = InstantiateButton(dialog.transform, "CloseButton", "Close", out Text closeBtnText);
            closeBtn.GetComponent<LayoutElement>().preferredHeight = 68;
            closeBtn.GetComponent<LayoutElement>().preferredWidth = 280;

            var qDialog = dialog.AddComponent<QuartersDialog>();
            var qSo = new SerializedObject(qDialog);
            qSo.FindProperty("_descriptionText").objectReferenceValue = descTxt;
            qSo.FindProperty("_upgradeDescriptionText").objectReferenceValue = upgDescTxt;
            qSo.FindProperty("_costCurrencyView").objectReferenceValue = instCurrency.GetComponent<LegacyCurrencyView>();
            qSo.FindProperty("_upgradeButton").objectReferenceValue = upgBtn.GetComponent<Button>();
            qSo.FindProperty("_upgradeButtonText").objectReferenceValue = upgBtnText;
            qSo.FindProperty("_closeButton").objectReferenceValue = closeBtn.GetComponent<Button>();
            qSo.ApplyModifiedPropertiesWithoutUndo();

            string outPath = "Assets/_Game/Prefabs/UI/Headquarters/QuartersDialog.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(outPath));
            PrefabUtility.SaveAsPrefabAssetAndConnect(dialog, outPath, InteractionMode.AutomatedAction);
            Object.DestroyImmediate(dialog);
        }

        // ─── Shared helpers ──────────────────────────────────────────────────

        private static Text AddText(Transform parent, string name, string text, int fontSize, Color color, FontStyle style, TextAnchor alignment, float preferredHeight)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var txt = go.GetComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.color = color;
            txt.fontStyle = style;
            txt.alignment = alignment;
            txt.text = text;
            go.GetComponent<LayoutElement>().preferredHeight = preferredHeight;
            return txt;
        }

        private static void AddSpacer(Transform parent, float height)
        {
            GameObject go = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = height;
        }

        private static GameObject InstantiateButton(Transform parent, string name, string label, out Text labelText)
        {
            string buttonPrefabPath = "Assets/_Game/Prefabs/UI/Legacy/LegacyButtonFrame.prefab";
            GameObject btn = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(buttonPrefabPath), parent);
            btn.name = name;
            if (btn.GetComponent<LayoutElement>() == null) btn.AddComponent<LayoutElement>();

            // LegacyButtonFrame.prefab (Phase 2) is a bare Image — no Button component. Without
            // this, the instantiated "button" is just a static graphic with no click behavior at
            // all, which is the root cause of the reported Close/Upgrade-not-working bug.
            if (!btn.TryGetComponent<Button>(out var buttonComponent))
            {
                buttonComponent = btn.AddComponent<Button>();
            }
            if (btn.TryGetComponent<Image>(out var btnImage))
            {
                buttonComponent.targetGraphic = btnImage;
            }

            GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(btn.transform, false);
            var rt = (RectTransform)textGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            labelText = textGo.GetComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 32;
            labelText.color = LegacyUITheme.DimWhite;
            labelText.fontStyle = FontStyle.Bold;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.text = label;

            return btn;
        }
    }
}
