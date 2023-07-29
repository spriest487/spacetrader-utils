using UnityEditor;
using UnityEngine;

namespace SpaceTrader.Util.EditorUtil {
    public static class AspectRatioLayoutSwitcherEditor {
        [MenuItem("Tools/SpaceTrader/Refresh All Layout Switchers")]
        private static void RefreshAllSwitchersInScene() {
            var switchers = Object.FindObjectsByType<AspectRatioLayoutSwitcher>(FindObjectsSortMode.None);
            foreach (var switcher in switchers) {
                if (switcher.isActiveAndEnabled) {
                    switcher.SendMessage("OnRectTransformDimensionsChange");

                    Debug.Log($"refreshed layout of {switcher.name}", switcher);
                }
            }
        }
    }
}
