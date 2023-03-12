using System.Collections;
using UnityEngine;

namespace SpaceTrader.Util {
    public static class CanvasGroupUtil {
        public static IEnumerator AnimateFade(
            this CanvasGroup canvasGroup,
            float startAlpha,
            float endAlpha,
            float duration
        ) {
            if (!canvasGroup) {
                yield break;
            }

            var start = Time.time;
            while (true) {
                var progress = (Time.time - start) / duration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.Min(1, progress));

                if (progress >= 1) {
                    break;
                }

                yield return null;
            }
        }

        public static bool IsInteractiveInCanvasGroups(this Behaviour uiBehaviour) {
            var interactible = true;
            var next = uiBehaviour.transform;
            do {
                if (next.TryGetComponent(out CanvasGroup canvasGroup)) {
                    if (canvasGroup.ignoreParentGroups) {
                        interactible = canvasGroup.interactable;
                        break;
                    }

                    interactible &= canvasGroup.interactable;
                }

                next = next.parent;
            } while (next);

            return interactible;
        }
    }
}
