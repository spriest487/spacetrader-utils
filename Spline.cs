using System;
using JetBrains.Annotations;
using UnityEngine;

namespace SpaceTrader.Util {
    public readonly struct Spline {
        [Serializable]
        public struct Segment {
            public Vector3 StartTangent;
            public Vector3 End;
            public Vector3 EndTangent;
        }

        public const int DefaultSteps = 5;

        private readonly Vector3[] positions;
        private readonly float[] distances;

        private readonly int count;

        public float Length { get; }

        public ReadOnlySpan<Vector3> Positions {
            get {
                if (this.positions == null) {
                    return default;
                }

                return this.positions.AsSpan(0, this.count);
            }
        }

        private Spline([NotNull] Vector3[] positions, [CanBeNull] float[] distances, int count, float length) {
            this.positions = positions;
            this.distances = distances;
            this.count = count;
            this.Length = length;
        }

        public int EvaluateIndex(float time, out float fractionalPart) {
            fractionalPart = 0f;

            if (this.positions == null || this.count == 0) {
                return -1;
            }

            if (time <= 0f) {
                return 0;
            }

            if (time >= 1f) {
                return this.count - 1;
            }
            
            var nearestIndex = (this.count - 1) * time;
            var index = Mathf.FloorToInt(nearestIndex);

            if (this.distances != null) {
                while (index > 1 && this.distances[index] >= time) {
                    index -= 1;
                }

                while (index < this.distances.Length - 1 && this.distances[index + 1] < time) {
                    index += 1;
                }
            }

            if (this.distances != null) {
                var startDist = this.distances[index];
                var endDist = this.distances[index + 1];

                fractionalPart = Mathf.InverseLerp(startDist, endDist, time);
            } else {
                fractionalPart = nearestIndex % 1.0f;
            }

            return index;
        }

        public Vector3 Evaluate(float time) {
            var index = this.EvaluateIndex(time, out var fraction);

            if (fraction == 0f || index == this.count - 1) {
                return this.positions[index];
            }

            var startPos = this.positions[index];
            var endPos = this.positions[index + 1];

            return Vector3.LerpUnclamped(startPos, endPos, fraction);
        }

        public static Spline FromSegments(
            Vector3 start,
            ReadOnlySpan<Segment> segments,
            int steps = DefaultSteps,
            bool calculateDistances = false
        ) {
            var spline = new Spline();
            CreateFromSegments(ref spline, start, segments, steps, calculateDistances);
            return spline;
        }

        public static void CreateFromSegments(
            ref Spline spline,
            Vector3 start,
            ReadOnlySpan<Segment> segments,
            int steps = DefaultSteps,
            bool calculateDistances = false
        ) {
            var capacity = 1 + segments.Length * steps;
            
            var positions = spline.positions;
            if (positions == null || positions.Length < capacity) {
                positions = new Vector3[capacity];
            }
            
            var distances = spline.distances;
            if (calculateDistances) {
                if (distances == null || distances.Length < capacity) {
                    distances = new float[capacity];
                }
            } else {
                distances = null;
            }
            
            // a spline with no segments is just its origin point
            if (segments.Length == 0 || steps <= 0) {
                spline.positions[0] = start;
                if (calculateDistances) {
                    spline.distances[0] = 0f;
                }

                spline = new Spline(positions, distances, capacity, 0f);
                return;
            }

            positions[0] = start;

            var length = 0f;

            var posIndex = 1;

            for (var s = 0; s < segments.Length; s += 1) {
                var segment = segments[s];
                for (var step = 1; step <= steps; step += 1) {
                    var time = step / (float)steps;
                    var pos = CubicBezier(time, start, segment.StartTangent, segment.EndTangent, segment.End);
                    positions[posIndex] = pos;

                    if (distances != null) {
                        var distanceBetween = Vector3.Distance(pos, positions[posIndex - 1]);
                        length += distanceBetween;

                        // the stored distance used for interpolation is cumulative
                        distances[posIndex] = distances[posIndex - 1] + distanceBetween;
                    }

                    posIndex += 1;
                }

                start = segment.End;
            }

            // distances are stored normalized
            if (distances != null && length > 0f) {
                // make sure these values are exact:
                distances[0] = 0f;
                if (distances.Length > 1) {
                    distances[^1] = 1f;
                }

                for (var i = 1; i < distances.Length - 1; i += 1) {
                    distances[i] /= length;
                }
            } else {
                length = Vector3.Distance(positions[0], positions[^1]);
            }

            spline = new Spline(positions, distances, capacity, length);
        }

        public static Vector3 CubicBezier(
            float time,
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3 p3
        ) {
            var u = 1 - time;
            var tt = time * time;
            var uu = u * u;
            var uuu = uu * u;
            var ttt = tt * time;

            var p = uuu * p0;
            p += 3 * uu * time * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }
    }
}
