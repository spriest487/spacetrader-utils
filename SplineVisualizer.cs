using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceTrader.Util
{
    public class SplineVisualizer : MonoBehaviour
    {
        public static SplineVisualizer Create(GameObject obj, 
            Vector3 origin,
            IEnumerable<Spline.Segment> segments,
            int steps = Spline.DefaultSteps,
            float gizmoSize = 1.0f)
        {
            var visualizer = obj.AddComponent<SplineVisualizer>();
            visualizer.origin = origin;
            visualizer.segments = segments.ToArray();
            visualizer.steps = steps;
            visualizer.gizmoSize = gizmoSize;
            return visualizer;
        }

        [SerializeField]
        private Vector3 origin;

        [SerializeField]
        private Spline.Segment[] segments;

        [SerializeField]
        private int steps = Spline.DefaultSteps;

        [SerializeField]
        private float gizmoSize = 1.0f;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var localOrigin = transform.TransformPoint(origin);

            var localSegments = segments.Select(s => new Spline.Segment
                {
                    StartTangent = transform.TransformPoint(s.StartTangent),
                    End = transform.TransformPoint(s.End),
                    EndTangent = transform.TransformPoint(s.EndTangent),
                })
                .ToList();
            
            foreach (var segment in localSegments)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(localOrigin, Vector3.one * gizmoSize);
                Gizmos.DrawWireCube(segment.End, Vector3.one * gizmoSize);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(localOrigin, segment.StartTangent);
                Gizmos.DrawWireCube(segment.StartTangent, Vector3.one * 0.5f * gizmoSize);

                Gizmos.DrawLine(segment.End, segment.EndTangent);
                Gizmos.DrawWireCube(segment.EndTangent, Vector3.one * 0.5f * gizmoSize);
            }

            var spline = Spline.FromSegments(localOrigin, localSegments, steps);

            int index = 1;
            Gizmos.color = Color.cyan;

            if (spline.Positions.Count > 0)
            {
                Gizmos.DrawLine(localOrigin, spline.Positions[0]);

                foreach (var point in spline.Positions)
                {
                    if (index < spline.Positions.Count - 2)
                    {
                        Gizmos.DrawLine(point, spline.Positions[index + 1]);
                    }

                    Gizmos.DrawWireSphere(point, 0.25f * gizmoSize);
                    UnityEditor.Handles.Label(point, $"{index}/{spline.Positions.Count}");

                    ++index;
                }
            }
        }
#endif
    }
}