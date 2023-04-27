using NUnit.Framework;
using UnityEngine;

namespace SpaceTrader.Util.Tests {
    public class SplineTests {
        public static object[] LengthTestCases = {
            new object[] {
                new[] {
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 4f),
                },
                new[] {
                    0f,
                    1f,
                },
                0.5f,
                0.25f,
                new Vector3(0f, 0f, 2f),
                new Vector3(0f, 0f, 3f),
            },

            new object[] {
                new[] {
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 3f),
                    new Vector3(0f, 0f, 6f),
                },
                new[] {
                    0f,
                    0.5f,
                    1f,
                },
                0.1f,
                0.5f,
                new Vector3(0f, 0f, 0.6f),
                new Vector3(0f, 0f, 3.6f),
            },
        };

        [TestCaseSource(nameof(LengthTestCases))]
        public void SliceTest(
            Vector3[] positions,
            float[] distances,
            float sliceStart,
            float sliceLength,
            Vector3 expectStart,
            Vector3 expectEnd
        ) {
            var spline = new Spline(positions, distances, positions.Length, distances[^1]);
            var sliced = spline.Slice(sliceStart, sliceLength);

            var actualStart = sliced.Positions[0];
            var actualEnd = sliced.Positions[^1];
            Assert.IsTrue(expectStart == actualStart, $"actualStart {actualStart} == expectStart {expectStart}");
            Assert.IsTrue(expectEnd == actualEnd, $"actualEnd {actualEnd} == expectEnd {expectEnd}");

            Assert.AreEqual(0f, sliced.Distances[0]);
            Assert.AreEqual(1f, sliced.Distances[^1]);
        }
    }
}
