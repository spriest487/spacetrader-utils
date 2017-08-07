using System.Collections;
using UnityEngine;

namespace PhoenixQuest
{
    public class WaitOrTimeout : IEnumerator
    {
        public float StartedWaiting { get; }
        public float Timeout { get; }
        
        private readonly IEnumerator waitFor;

        public WaitOrTimeout(IEnumerator waitFor, float timeout)
        {
            StartedWaiting = Time.time;
            Timeout = timeout;

            this.waitFor = waitFor;
        }

        public object Current => waitFor.Current;

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
