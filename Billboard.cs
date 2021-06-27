using UnityEngine;

namespace SpaceTrader.Util {
    public class Billboard : MonoBehaviour {
        [field: SerializeField, Range(-360, 360)]
        public float Angle { get; set; }

        [field: SerializeField]
        public Camera Camera { get; set; }

        private void LateUpdate() {
            var camera = this.Camera ? this.Camera : Camera.main;
            if (!camera) {
                return;
            }

            var up = Vector3.up;
            if (!Mathf.Approximately(0, this.Angle)) {
                var angleRot = Quaternion.Euler(0, 0, -this.Angle);
                up = angleRot * up;
            }

            var camXform = camera.transform;
            var camRotation = camXform.rotation;

            this.transform.LookAt(this.transform.position + camRotation * Vector3.forward,
                camRotation * up);
        }

        [ContextMenu("Apply Rotation Now")]
        private void ApplyRotationNow() {
            this.LateUpdate();
        }
    }
}
