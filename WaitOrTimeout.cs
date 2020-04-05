using System.Collections;
using UnityEngine;

namespace SpaceTrader.Util {
    public class WaitOrTimeout : IEnumerator {
        private readonly IEnumerator waitFor;

        public WaitOrTimeout(IEnumerator waitFor, float timeout) {
            this.StartedWaiting = Time.time;
            this.Timeout = timeout;

            this.waitFor = waitFor;
        }

        public float StartedWaiting { get; }
        public float Timeout { get; }

        public object Current => this.waitFor.Current;

        public bool MoveNext() {
            if (Time.time > this.StartedWaiting + this.Timeout) {
                return false;
            }

            return this.waitFor.MoveNext();
        }

        public void Reset() {
            this.waitFor.Reset();
        }
    }
}