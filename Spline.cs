using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Util {
    [Serializable]
    public struct Spline {
        [Serializable]
        public struct Segment {
            public Vector3 StartTangent;
            public Vector3 End;
            public Vector3 EndTangent;
        }

        public const int DefaultSteps = 5;

        private static Vector3[] EmptyPositions = new Vector3[0];

        [HideInInspector]
        [SerializeField]
        private List<Vector3> positions;
        public IReadOnlyList<Vector3> Positions =>
            positions ?? (IReadOnlyList<Vector3>)EmptyPositions;

        public Vector3 Evaluate(float time) {
            if (positions == null) {
                return Vector3.zero;
            }

            var findex = (positions.Count - 1) * time;

            var index = Mathf.Clamp(Mathf.FloorToInt(findex), 0, positions.Count - 1);
            var nextIndex = Mathf.Min(index + 1, positions.Count - 1);

            var fraction = findex % 1.0f;

            return Vector3.Lerp(positions[index], positions[nextIndex], fraction);
        }

        public static Spline FromSegments(Vector3 start, IReadOnlyCollection<Segment> segments,
                int steps = DefaultSteps) {
            var positions = new List<Vector3>(1 + segments.Count * steps);
            positions.Add(start);

            foreach (var segment in segments) {
                for (int step = 1; step <= steps; ++step) {
                    var time = step / (float)steps;
                    var pos = CubicBezier(time, start, segment.StartTangent, segment.EndTangent, segment.End);
                    positions.Add(pos);
                }

                start = segment.End;
            }

            return new Spline { positions = positions };
        }

        public static Vector3 CubicBezier(float time,
                Vector3 p0,
                Vector3 p1,
                Vector3 p2,
                Vector3 p3) {
            float u = 1 - time;
            float tt = time * time;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * time;

            Vector3 p = uuu * p0;
            p += 3 * uu * time * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }
    }
}