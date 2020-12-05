using UnityEngine;
using Random = System.Random;

namespace SpaceTraderUtils {
    public static class RandomExtensions {
        public static float NextFloat(this Random random) {
            return (float)random.NextDouble();
        }

        public static float NextFloat(this Random random, float min, float max) {
            return min + (max - min) * random.NextFloat();
        }

        public static Vector3 PointOnSphere(this Random random, float radius = 1f) {
            var x = random.NextFloat();
            var y = random.NextFloat();
            var z = random.NextFloat();

            if (x == 0 && y == 0 && z == 0) {
                return Vector3.forward * radius;
            }

            var lenInv = 1f / Mathf.Sqrt(x * x + y * y + z * z);

            return new Vector3(x * lenInv, y * lenInv, z * lenInv) * radius;
        }
    }
}
