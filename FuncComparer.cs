using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SpaceTrader.Util {
    public struct FuncComparer<T> : IComparer<T> {
        [NotNull]
        private readonly Func<T, T, int> comparison;

        public FuncComparer([NotNull] Func<T, T, int> comparison) {
            this.comparison = comparison;
        }

        public int Compare(T x, T y) {
            return this.comparison(x, y);
        }
    }
}
