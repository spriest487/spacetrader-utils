using UnityEngine;
using UnityEngine.UI;

namespace SpaceTrader.Util {
    public class FitParentDimension : MonoBehaviour, ILayoutElement {
        public enum MatchDimension {
            None,
            ParentWidth,
            ParentHeight
        }

        [SerializeField]
        private int layoutPriority;

        [SerializeField]
        private MatchDimension matchHeightWith = MatchDimension.None;

        [SerializeField]
        private MatchDimension matchWidthWith = MatchDimension.None;

        private float preferredHeight;

        private float preferredWidth;

        float ILayoutElement.preferredWidth => this.preferredWidth;
        float ILayoutElement.preferredHeight => this.preferredHeight;
        int ILayoutElement.layoutPriority => this.layoutPriority;

        float ILayoutElement.minWidth => 0;
        float ILayoutElement.flexibleWidth => 0;
        float ILayoutElement.minHeight => 0;
        float ILayoutElement.flexibleHeight => 0;

        void ILayoutElement.CalculateLayoutInputHorizontal() {
            this.preferredWidth = this.GetDimension(this.matchWidthWith);
        }

        void ILayoutElement.CalculateLayoutInputVertical() {
            this.preferredHeight = this.GetDimension(this.matchHeightWith);
        }

        private float GetDimension(MatchDimension dimension) {
            var rectXform = this.transform.parent.GetComponentInParent<RectTransform>();
            if (!rectXform) {
                return 0;
            }

            switch (dimension) {
                case MatchDimension.ParentWidth:
                    return rectXform.rect.width;

                case MatchDimension.ParentHeight:
                    return rectXform.rect.height;

                default: return 0;
            }
        }
    }
}