using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace SpaceTrader.Util {
    public class MovementTowards : IEnumerator<Vector3> {
        public Vector3 Start { get; }
        public Vector3 End { get; }
        public float Duration { get; }

        public float Progress { get; private set; }

        private readonly float startTime;

        public Vector3 Current { get; private set; }
        object IEnumerator.Current => Current;

        public bool Smooth { get; }

        public MovementTowards(Vector3 start, Vector3 end, float duration, bool smooth = true) {
            Start = start;
            End = end;
            Duration = duration;

            startTime = Time.time;

            Smooth = smooth;
        }

        public bool MoveNext() {
            Progress = Mathf.Clamp01((Time.time - startTime) / Duration);
            if (Smooth) {
                Progress = Mathf.SmoothStep(0, 1, Progress);
            }

            if (Progress >= 1) {
                Current = End;
                return false;
            }

            Current = Vector3.LerpUnclamped(Start, End, Progress);
            return true;
        }

        public void Reset() {
        }

        public void Dispose() {
        }
    }
}