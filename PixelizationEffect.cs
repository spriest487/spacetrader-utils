using System;
using UnityEngine;

namespace SpaceTrader.Util {
    [RequireComponent(typeof(Camera))]
    public class PixelizationEffect : MonoBehaviour {
        public enum MatchDimensionOption {
            Width,
            Height,
        }

        private const string ShaderName = "SpaceTrader/PixelizationEffect";

        private static readonly int ScaleXShaderProperty = Shader.PropertyToID("_ScaleX");
        private static readonly int ScaleYShaderProperty = Shader.PropertyToID("_ScaleY");

        [field: SerializeField]
        public int Width { get; private set; } = 640;

        [field: SerializeField]
        public int Height { get; private set; } = 360;

        [field: SerializeField]
        public int Depth { get; private set; } = 24;

        [field: SerializeField]
        public bool Multisample { get; private set; } = true;

        [field: SerializeField]
        public MatchDimensionOption MatchDimension { get; private set; } = MatchDimensionOption.Height;

        [SerializeField]
        private Shader shader;

        private Material material;

        private void Start() {
            this.material = new Material(this.shader);
            if (this.Multisample) {
                this.material.EnableKeyword("MULTISAMPLE_ON");
            } else {
                this.material.DisableKeyword("MULTISAMPLE_ON");
            }
        }

        private void OnValidate() {
            if (!this.shader) {
                this.shader = Shader.Find(ShaderName);
            }

            if (this.material) {
                if (this.Multisample) {
                    this.material.EnableKeyword("MULTISAMPLE_ON");
                } else {
                    this.material.DisableKeyword("MULTISAMPLE_ON");
                }
            }
        }

        private void OnDestroy() {
            Destroy(this.material);
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest) {
            var destWidth = dest ? dest.width : Screen.width;
            var destHeight = dest ? dest.height : Screen.height;

            var scale = this.MatchDimension switch {
                MatchDimensionOption.Width => (float)destWidth / this.Width,
                _ => (float)destHeight / this.Height,
            };

            this.material.SetFloat(ScaleXShaderProperty, scale);
            this.material.SetFloat(ScaleYShaderProperty, scale);

            Graphics.Blit(src, dest, this.material);
        }
    }
}
