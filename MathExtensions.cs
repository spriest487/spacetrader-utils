namespace SpaceTrader.Util {
    public static class MathExtensions {
        public static int Mod(this int x, int m) {
            var r = x % m;
            return r < 0 ? r + m : r;
        }

        public static float Mod(this float x, float m) {
            var r = x % m;
            return r < 0 ? r + m : r;
        }

        public static double Mod(this double x, double m) {
            var r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}