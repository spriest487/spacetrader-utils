using System;
using UnityEngine;

namespace SpaceTrader.Util {
    [Serializable]
    public struct LineSegment2D : IEquatable<LineSegment2D> {
        [field: SerializeField]
        public Vector2 Origin { get; private set; }

        [field: SerializeField]
        public Vector2 Direction { get; private set; }

        public Vector2 End => this.Origin + this.Direction;

        public float Length => this.Direction.magnitude;
        public float SqrLength => this.Direction.sqrMagnitude;

        public Vector2 Right => this.Direction.Rotate90CW();
        public Vector2 Left => -this.Right;

        public LineSegment2D(Vector2 origin, Vector2 direction) {
            this.Origin = origin;
            this.Direction = direction;
        }

        public bool Equals(LineSegment2D other) {
            return this.Origin.Equals(other.Origin)
                && this.Direction.Equals(other.Direction);
        }

        public override bool Equals(object obj) {
            return obj is LineSegment2D other && this.Equals(other);
        }

        public override int GetHashCode() {
            return (Start: this.Origin, this.Direction).GetHashCode();
        }

        public static bool operator==(LineSegment2D left, LineSegment2D right) {
            return left.Equals(right);
        }

        public static bool operator!=(LineSegment2D left, LineSegment2D right) {
            return !left.Equals(right);
        }

        public LineSegment2D Offset(Vector2 distance) {
            return new LineSegment2D(this.Origin + distance, this.Direction);
        }

        public static LineSegment2D operator*(LineSegment2D line, Vector2 scale) {
            var origin = line.Origin * scale;
            var direction = line.Direction * scale;
            return new LineSegment2D(origin, direction);
        }
    }
}
