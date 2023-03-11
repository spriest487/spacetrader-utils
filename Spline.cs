using System;
using System.Collections.Generic;
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

        public float Length { get; }

        public IReadOnlyList<Vector3> Positions => this.positions ?? (IReadOnlyList<Vector3>)Array.Empty<Vector3>();

        private Spline([NotNull] Vector3[] positions, [CanBeNull] float[] distances, float length) {
            this.positions = positions;
            this.distances = distances;
            this.Length = length;
        }

        public Vector3 Evaluate(float time) {
            if (this.positions == null || this.positions.Length == 0) {
                return Vector3.zero;
            }

            if (time <= 0f) {
                return this.positions[0];
            }

            if (time >= 1f) {
                return this.positions[^1];
            }

            var nearestIndex = (this.positions.Length - 1) * time;
            var index = Mathf.FloorToInt(nearestIndex);

            if (this.distances != null) {
                while (index > 1 && this.distances[index] >= time) {
                    index -= 1;
                }

                while (index < this.distances.Length - 1 && this.distances[index + 1] < time) {
                    index += 1;
                }
            }

            var startPos = this.positions[index];
            var endPos = this.positions[index + 1];

            float fraction;
            if (this.distances != null) {
                var startDist = this.distances[index];
                var endDist = this.distances[index + 1];

                fraction = Mathf.InverseLerp(startDist, endDist, time);
            } else {
                fraction = nearestIndex % 1.0f;
            }

            return Vector3.LerpUnclamped(startPos, endPos, fraction);
        }

        public static Spline FromSegments(
            Vector3 start,
            ReadOnlySpan<Segment> segments,
            int steps = DefaultSteps,
            bool calculateDistances = false
        ) {
            if (segments.Length == 0 || steps <= 0) {
                return new Spline(new[] { start }, distances: new[] { 0f }, 0f);
            }

            var capacity = 1 + segments.Length * steps;

            var positions = new Vector3[capacity];
            var distances = calculateDistances ? new float[capacity] : null;
            
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

            return new Spline(positions, distances, length);
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
