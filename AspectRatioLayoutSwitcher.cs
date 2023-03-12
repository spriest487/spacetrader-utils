using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SpaceTrader.Util {
    public class AspectRatioLayoutSwitcher : MonoBehaviour {
        [SerializeField]
        private Transform tallRoot;

        [SerializeField]
        private Transform wideRoot;

        [ContextMenu("Refresh")]
        private void OnRectTransformDimensionsChange() {
            /* find out which layout we should be in */
            var rect = ((RectTransform)this.transform).rect;
            var isWide = rect.width > rect.height;

            /* we have one root for the "tall" layout and one for the "wide"
             layout - only one of them should ever have children!*/
            Debug.Assert(this.wideRoot.childCount == 0 || this.tallRoot.childCount == 0,
                $"at least one of the layout roots in {nameof(AspectRatioLayoutSwitcher)} must be empty",
                this);

            if (this.wideRoot.childCount == 0 && isWide) {
                this.SwitchChildren(this.tallRoot, this.wideRoot);
            } else if (this.tallRoot.childCount == 0 && !isWide) {
                this.SwitchChildren(this.wideRoot, this.tallRoot);
            }
        }

        private void SwitchChildren(Transform from, Transform to) {
#if UNITY_EDITOR
            Undo.RecordObjects(new Object[] { from, to }, $"Refresh {nameof(AspectRatioLayoutSwitcher)}");
#endif
            for (var child = from.childCount; child > 0; --child) {
                var childXform = from.GetChild(child - 1);
                childXform.SetParent(to);
                childXform.SetAsFirstSibling();
            }
        }
    }
}
