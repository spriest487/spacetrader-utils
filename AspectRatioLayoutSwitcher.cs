using UnityEngine;

namespace SpaceTrader.Util
{
    public class AspectRatioLayoutSwitcher : MonoBehaviour
    {
        [SerializeField]
        private Transform wideRoot;

        [SerializeField]
        private Transform tallRoot;
        
        [ContextMenu("Refresh")]
        private void OnRectTransformDimensionsChange()
        {
            /* find out which layout we should be in */
            var rect = ((RectTransform)transform).rect;
            var isWide = rect.width > rect.height;

            /* we have one root for the "tall" layout and one for the "wide"
             layout - only one of them should ever have children!*/
            Debug.Assert(wideRoot.childCount == 0 || tallRoot.childCount == 0,
                $"at least one of the layout roots in {nameof(AspectRatioLayoutSwitcher)} must be empty",
                this);

            if (wideRoot.childCount == 0 && isWide)
            {
                SwitchChildren(tallRoot, wideRoot);
            }
            else if (tallRoot.childCount == 0 && !isWide)
            {
                SwitchChildren(wideRoot, tallRoot);
            }
        }

        private void SwitchChildren(Transform from, Transform to)
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObjects(new[] { from, to }, $"Refresh {nameof(AspectRatioLayoutSwitcher)}");
#endif
            for (int child = from.childCount; child > 0; --child)
            {
                var childXform = from.GetChild(child - 1);
                childXform.SetParent(to);
                childXform.SetAsFirstSibling();
            }
        }
    }
}
