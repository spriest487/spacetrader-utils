using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Util {
    [ExecuteInEditMode]
    public class Billboard : MonoBehaviour {
        [Range(-360, 360)]
        [SerializeField]
        private float angle;
        public float Angle {
            get { return angle; }
            set { angle = value; }
        }

        public void Update() {
            var up = Vector3.up;
            if (!Mathf.Approximately(0, angle)) {
                var angleRot = Quaternion.Euler(0, 0, -angle);
                up = angleRot * up;
            }

            var camXform = Camera.main.transform;
            transform.LookAt(transform.position + camXform.rotation * Vector3.forward,
                camXform.rotation * up);
        }
    }
}