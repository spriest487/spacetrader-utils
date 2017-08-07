using System.Collections;
using UnityEngine;

namespace SpaceTrader.Util
{
    public class WaitOrTimeout : IEnumerator
    {
        public float StartedWaiting { get; private set; }
        public float Timeout { get; private set; }
        
        private readonly IEnumerator waitFor;

        public WaitOrTimeout(IEnumerator waitFor, float timeout)
        {
            StartedWaiting = Time.time;
            Timeout = timeout;

            this.waitFor = waitFor;
        }

        public object Current { get { return waitFor.Current; } }

        public bool MoveNext()
        {
            if (Time.time > StartedWaiting + Timeout)
            {
                return false;
            }
            
            return waitFor.MoveNext();
        }

        public void Reset()
        {
            waitFor.Reset();
        }
    }
}
