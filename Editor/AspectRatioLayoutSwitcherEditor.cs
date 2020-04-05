using UnityEditor;
using UnityEngine;

namespace SpaceTrader.Util {
    public static class AspectRatioLayoutSwitcherEditor {
        [MenuItem("SpaceTrader/Refresh All Layout Switchers")]
        private static void RefreshAllSwitchersInScene() {
            var switchers = Object.FindObjectsOfType<AspectRatioLayoutSwitcher>();
            foreach (var switcher in switchers) {
                if (switcher.isActiveAndEnabled) {
                    switcher.SendMessage("OnRectTransformDimensionsChange");

                    Debug.Log($"refreshed layout of {switcher.name}", switcher);
                }
            }
        }
    }
}