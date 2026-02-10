using System;
using System.Linq;
using System.Runtime.CompilerServices;
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
        
        [CanBeNull]
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

        internal ReadOnlySpan<float> Distances {
            get {
                if (this.distances == null) {
                    return default;
                }

                return this.distances.AsSpan(0, this.count);
            }
        }

        internal Spline([NotNull] Vector3[] positions, [CanBeNull] float[] distances, int count, float length) {
            this.positions = positions;
            this.distances = distances;
            this.count = count;
            this.Length = length;
        }

        public Spline Slice(float startTime, float length) {
            if (length == 0) {
                return new Spline();
            }

            var startIndex = this.EvaluateIndex(startTime, out var startFraction);
            var endIndex = this.EvaluateIndex(startTime + length, out var endFraction);

            var positionCount = endIndex - startIndex;

            var srcIndex = startIndex;
            var copyDstIndex = 0;
            var copyCount = positionCount;

            if (startFraction > 0f) {
                positionCount += 1;
                
                srcIndex += 1;
                copyDstIndex = 1;
            }

            if (endFraction > 0f) {
                positionCount += 1;
            }
            
            if (positionCount == 0) {
                return new Spline();
            }

            var positions = new Vector3[positionCount];
            var distances = this.distances != null ? new float[positionCount] : null;
            
            var distOffset = 0f;
            if (distances != null) {
                distances[0] = 0f;
                distOffset = Mathf.LerpUnclamped(
                    this.distances[startIndex],
                    this.distances[startIndex + 1],
                    startFraction
                );
            }

            for (var i = 0; i < copyCount; i += 1) {
                positions[copyDstIndex + i] = this.Positions[srcIndex + i];

                if (distances != null) {
                    distances[copyDstIndex + i] = this.distances[srcIndex + i] - distOffset;
                }
            }

            if (startFraction > 0f) {
                positions[0] = Vector3.Lerp(this.Positions[startIndex], this.Positions[startIndex + 1], startFraction);
                if (distances != null) {
                    distances[1] = Mathf.Lerp(this.distances[startIndex], this.distances[startIndex + 1],startFraction) - distOffset;
                }
            }

            if (endFraction > 0f) {
                positions[^1] = Vector3.Lerp(this.Positions[endIndex], this.Positions[endIndex + 1], endFraction);
                if (distances != null) {
                    distances[^1] = Mathf.Lerp(this.distances[endIndex], this.distances[endIndex + 1],endFraction) - distOffset;
                }
            }

            // renormalize distances for the new length
            if (distances != null) {
                for (var i = 1; i < positionCount; i += 1) {
                    distances[i] /= length;
                }
            }

            return new Spline(positions, distances, positionCount, length);
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
                while (index > 0 && this.distances[index] >= time) {
                    index -= 1;
                }

                while (index + 1 < this.count - 1 && this.distances[index + 1] < time) {
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

        public float ClosestTime(Vector3 position) {
            if (this.positions.Length < 2) {
                return 0f;
            }

            var minIndex = 0;

            var minDistSqr = float.PositiveInfinity;
            var minPrevDistSqr = float.PositiveInfinity;

            var prevDistSqr = Vector3.SqrMagnitude(position - this.positions[0]);

            for (var i = 1; i < this.positions.Length; i++) {
                var splinePos = this.positions[i];

                var distSqr = Vector3.SqrMagnitude(position - splinePos);
                
                if (distSqr <= minDistSqr && prevDistSqr <= minPrevDistSqr) {
                    minIndex = i;
                    minDistSqr = distSqr;
                    minPrevDistSqr = prevDistSqr;
                }

                prevDistSqr = distSqr;
            }

            if (minIndex == 0) {
                return 0f;
            }

            var splinePosA = this.positions[minIndex];
            var splinePosB = this.positions[minIndex - 1];

            if (splinePosA == splinePosB) {
                return minIndex / (float)(this.positions.Length - 1);
            }

            var between = splinePosB - splinePosA;
            var betweenLen = between.magnitude;
            var betweenDir = between / betweenLen;

            var relativePos = position - splinePosA;

            var projected = Vector3.Project(relativePos, betweenDir);
            var betweenTime = Mathf.Clamp01(projected.magnitude / betweenLen);

            return (minIndex + betweenTime) / (this.positions.Length - 1);
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
                if (distances != null) {
                    distances[0] = 0f;
                }

                spline = new Spline(positions, distances, 1, 0f);
                return;
            }

            positions[0] = start;

            var length = 0f;

            var posIndex = 1;

            foreach (var segment in segments) {
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
                if (capacity > 1) {
                    distances[capacity - 1] = 1f;
                }

                for (var i = 1; i < capacity - 1; i += 1) {
                    distances[i] /= length;
                }
            } else {
                length = Vector3.Distance(positions[0], positions[capacity - 1]);
            }

            spline = new Spline(positions, distances, capacity, length);
        }

        public void CopyTo(ref Spline other) {
            var positions = other.positions;
            
            if (positions.Length < this.count) {
                positions = new Vector3[this.count];
            }
            Array.Copy(this.positions, 0, positions, 0, this.count);

            float[] distances;
            if (this.distances != null) {
                distances = other.distances;
                if (distances == null || distances.Length < this.count) {
                    distances = new float[this.count];
                }
                
                Array.Copy(this.distances, 0, distances, 0, this.count);
            } else {
                distances = null;
            }

            other = new Spline(positions, distances, this.count, this.Length);
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
