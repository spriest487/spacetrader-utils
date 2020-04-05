using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Util {
    public class MovementTowards : IEnumerator<Vector3> {
        private readonly float startTime;

        public MovementTowards(Vector3 start, Vector3 end, float duration, bool smooth = true) {
            this.Start = start;
            this.End = end;
            this.Duration = duration;

            this.startTime = Time.time;

            this.Smooth = smooth;
        }

        public Vector3 Start { get; }
        public Vector3 End { get; }
        public float Duration { get; }

        public float Progress { get; private set; }

        public bool Smooth { get; }

        public Vector3 Current { get; private set; }
        object IEnumerator.Current => this.Current;

        public bool MoveNext() {
            this.Progress = Mathf.Clamp01((Time.time - this.startTime) / this.Duration);
            if (this.Smooth) {
                this.Progress = Mathf.SmoothStep(0, 1, this.Progress);
            }

            if (this.Progress >= 1) {
                this.Current = this.End;
                return false;
            }

            this.Current = Vector3.LerpUnclamped(this.Start, this.End, this.Progress);
            return true;
        }

        public void Reset() {
        }

        public void Dispose() {
        }
    }
}