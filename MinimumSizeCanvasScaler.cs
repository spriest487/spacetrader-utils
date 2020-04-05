using UnityEngine;
using UnityEngine.UI;

namespace SpaceTrader.Util {
    [ExecuteInEditMode, RequireComponent(typeof(CanvasScaler))]
    public class MinimumSizeCanvasScaler : MonoBehaviour {
        private CanvasScaler canvasScaler;

        [SerializeField]
        private int maxHeight = 1080;

        [SerializeField]
        private int maxWidth = 1920;

        [SerializeField]
        private int minHeight = 768;

        [SerializeField]
        private int minWidth = 1024;

        private void Start() {
            this.canvasScaler = this.GetComponent<CanvasScaler>();
        }

        private void OnEnable() {
            this.Start();
            this.Update();
        }

        private void Update() {
            float scale;
            if (Screen.height < this.minHeight || Screen.width < this.minWidth) {
                scale = Mathf.Min(Screen.height / (float)this.minHeight, Screen.width / (float)this.minWidth);
            } else if (Screen.height > this.maxHeight || Screen.width > this.maxWidth) {
                scale = Mathf.Max(Screen.height / (float)this.maxHeight, Screen.width / (float)this.maxWidth);
            } else {
                scale = 1;
            }

            this.canvasScaler.scaleFactor = scale;
        }

#if UNITY_EDITOR
        private void OnGUI() {
            this.Update();
        }
#endif
    }
}