using UnityEngine;
using UnityEngine.UI;

namespace SpaceTrader.Util
{
    public class FitParentDimension : MonoBehaviour, ILayoutElement
    {
        public enum MatchDimension
        {
            None,
            ParentWidth,
            ParentHeight,
        }

        [SerializeField]
        private MatchDimension matchWidthWith = MatchDimension.None;

        [SerializeField]
        private MatchDimension matchHeightWith = MatchDimension.None;

        [SerializeField]
        private int layoutPriority;
        
        private float preferredWidth;
        private float preferredHeight;

        float ILayoutElement.preferredWidth => preferredWidth;
        float ILayoutElement.preferredHeight => preferredHeight;
        int ILayoutElement.layoutPriority => layoutPriority;
        
        float ILayoutElement.minWidth => 0;
        float ILayoutElement.flexibleWidth => 0;
        float ILayoutElement.minHeight => 0;
        float ILayoutElement.flexibleHeight => 0;

        private float GetDimension(MatchDimension dimension)
        {
            var rectXform = transform.parent.GetComponentInParent<RectTransform>();
            if (!rectXform)
            {
                return 0;
            }

            switch (dimension)
            {
                case MatchDimension.ParentWidth:
                    return rectXform.rect.width;

                case MatchDimension.ParentHeight:
                    return rectXform.rect.height;

                default: return 0;
            }
        }
        
        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
            preferredWidth = GetDimension(matchWidthWith);
        }

        void ILayoutElement.CalculateLayoutInputVertical()
        {
            preferredHeight = GetDimension(matchHeightWith);
        }
    }
}
