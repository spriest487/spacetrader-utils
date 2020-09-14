using System;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace SpaceTrader.Util {
    public readonly struct ProfilerSegment : IDisposable {
        public ProfilerSegment(string name) {
            Profiler.BeginSample(name);
        }

        public ProfilerSegment(string name, Object context) {
            Profiler.BeginSample(name, context);
        }

        public void Dispose() {
            Profiler.EndSample();
        }
    }
}
