using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpaceTrader.Util {
    public class SplineVisualizer : MonoBehaviour {
        [SerializeField]
        private float gizmoSize = 1.0f;

        [SerializeField]
        private Vector3 origin;

        [SerializeField]
        private Spline.Segment[] segments;

        [SerializeField]
        private int steps = Spline.DefaultSteps;

        public static SplineVisualizer Create(
            GameObject obj,
            Vector3 origin,
            ReadOnlySpan<Spline.Segment> segments,
            int steps = Spline.DefaultSteps,
            float gizmoSize = 1.0f
        ) {
            var visualizer = obj.AddComponent<SplineVisualizer>();
            visualizer.origin = origin;
            visualizer.segments = segments.ToArray();
            visualizer.steps = steps;
            visualizer.gizmoSize = gizmoSize;
            return visualizer;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            var localOrigin = this.transform.TransformPoint(this.origin);

            var localSegments = new Spline.Segment[this.segments.Length];

            for (var i = 0; i < this.segments.Length; i += 1) {
                var segment = this.segments[i];
                
                localSegments[i] = new Spline.Segment {
                    StartTangent = this.transform.TransformPoint(segment.StartTangent),
                    End = this.transform.TransformPoint(segment.End),
                    EndTangent = this.transform.TransformPoint(segment.EndTangent),
                };
            }

            foreach (var segment in localSegments) {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(localOrigin, Vector3.one * this.gizmoSize);
                Gizmos.DrawWireCube(segment.End, Vector3.one * this.gizmoSize);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(localOrigin, segment.StartTangent);
                Gizmos.DrawWireCube(segment.StartTangent, Vector3.one * 0.5f * this.gizmoSize);

                Gizmos.DrawLine(segment.End, segment.EndTangent);
                Gizmos.DrawWireCube(segment.EndTangent, Vector3.one * 0.5f * this.gizmoSize);
            }

            var spline = Spline.FromSegments(localOrigin, localSegments, this.steps);

            var index = 1;
            Gizmos.color = Color.cyan;

            if (spline.Positions.Count > 0) {
                Gizmos.DrawLine(localOrigin, spline.Positions[0]);

                foreach (var point in spline.Positions) {
                    if (index < spline.Positions.Count - 2) {
                        Gizmos.DrawLine(point, spline.Positions[index + 1]);
                    }

                    Gizmos.DrawWireSphere(point, 0.25f * this.gizmoSize);
                    Handles.Label(point, $"{index}/{spline.Positions.Count}");

                    ++index;
                }
            }
        }
#endif
    }
}
