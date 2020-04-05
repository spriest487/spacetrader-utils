using UnityEngine;

namespace SpaceTrader.Util {
    [ExecuteInEditMode]
    public class Billboard : MonoBehaviour {
        [Range(-360, 360), SerializeField]
        private float angle;

        public float Angle {
            get => this.angle;
            set => this.angle = value;
        }

        public void Update() {
            var up = Vector3.up;
            if (!Mathf.Approximately(0, this.angle)) {
                var angleRot = Quaternion.Euler(0, 0, -this.angle);
                up = angleRot * up;
            }

            var camXform = Camera.main.transform;
            this.transform.LookAt(this.transform.position + camXform.rotation * Vector3.forward,
                camXform.rotation * up);
        }
    }
}