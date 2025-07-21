using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using NotNullAttribute = System.Diagnostics.CodeAnalysis.NotNullAttribute;

namespace Teragram.Squadron.BattleMode {
    [Serializable]
    public class GridDictionary<T> : IReadOnlyDictionary<int2, T>, ISerializationCallbackReceiver {
        public struct Enumerator : IEnumerator<KeyValuePair<int2, T>> {
            private readonly GridDictionary<T> dict;
            private int rowIndex;
            private int colIndex;

            public Enumerator(GridDictionary<T> dict)
                : this() {
                this.dict = dict;
                this.rowIndex = -1;
            }

            public KeyValuePair<int2, T> Current {
                get {
                    Debug.Assert(this.dict.rowKeys != null);
                    Debug.Assert(this.dict.rows != null);

                    var y = this.dict.rowKeys[this.rowIndex];
                    ref var row = ref this.dict.rows[this.rowIndex];

                    var x = row.keys[this.colIndex];
                    var value = row.values[this.colIndex];

                    var coord = new int2(x, y);

                    return new KeyValuePair<int2, T>(coord, value);
                }
            }

            void IDisposable.Dispose() {
            }

            public bool MoveNext() {
                var rowLen = this.rowIndex >= 0 ? this.dict.rows![this.rowIndex].keys.Length : 0;
                if (this.colIndex < rowLen - 1) {
                    this.colIndex += 1;
                    return true;
                }

                if (this.dict.rows == null || this.rowIndex >= this.dict.rows.Length - 1) {
                    return false;
                }

                this.rowIndex += 1;
                this.colIndex = 0;

                return true;
            }

            public void Reset() {
                this.rowIndex = -1;
                this.colIndex = 0;
            }

            object IEnumerator.Current => this.Current;
        }

        [Serializable]
        private struct RowEntry {
            public int[] keys;
            public T[] values;
        }

        [SerializeField, HideIf("@true")]
        [CanBeNull]
        private int[] rowKeys;

        [SerializeField, HideIf("@true")]
        [CanBeNull]
        private RowEntry[] rows;

        public int Count => this.rowKeys?.Length ?? 0;

        public T this[int2 key] {
            get => this.TryGetValue(key, out var value)
                ? value
                : throw new ArgumentException($"key not found: {key}", nameof(key));
            set => this.AddOrInsertEntry(key, value);
        }

        public IEnumerable<int2> Keys => this.Select(entry => entry.Key);
        public IEnumerable<T> Values => this.Select(entry => entry.Value);

        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator<KeyValuePair<int2, T>> IEnumerable<KeyValuePair<int2, T>>.GetEnumerator() {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public void Clear() {
            this.rows = null;
            this.rowKeys = null;
        }

        public void Add(int2 key, T value) {
            if (!this.TryAdd(key, value)) {
                throw new ArgumentException("key already exists: {key}", nameof(key));
            }
        }

        private int GetOrInsertRow(int y) {
            var rowIndex = FindKey(this.rowKeys, y);
            if (rowIndex < 0) {
                rowIndex = ~rowIndex;
                Insert(ref this.rowKeys, ref this.rows, rowIndex, y, new RowEntry());
            }

            return rowIndex;
        }

        private void AddOrInsertEntry(int2 key, T value) {
            var rowIndex = this.GetOrInsertRow(key.y);
            ref var row = ref this.rows![rowIndex];

            var colIndex = FindKey(row.keys, key.x);
            if (colIndex < 0) {
                colIndex = ~colIndex;
                Insert(ref row.keys, ref row.values, colIndex, key.x, value);
            }

            row.values[colIndex] = value;
        }

        public bool TryAdd(int2 key, T value) {
            var rowIndex = this.GetOrInsertRow(key.y);
            ref var row = ref this.rows![rowIndex];

            var colIndex = FindKey(row.keys, key.x);
            if (colIndex >= 0) {
                return false;
            }

            Insert(ref row.keys, ref row.values, ~colIndex, key.x, value);
            return true;
        }

        public bool ContainsKey(int2 key) {
            throw new NotImplementedException();
        }

        public bool TryGetValue(int2 key, [MaybeNullWhen(false)] out T value) {
            var rowIndex = FindKey(this.rowKeys, key.y);
            if (rowIndex < 0) {
                value = default;
                return false;
            }

            ref readonly var row = ref this.rows![rowIndex];

            var colIndex = FindKey(row.keys, key.x);
            if (colIndex < 0) {
                value = default;
                return false;
            }

            value = this.rows[rowIndex].values[colIndex];
            return true;
        }

        private static void Insert<TVal>(
            [NotNull] ref int[] keys,
            ref TVal[] values,
            int insertIndex,
            int key,
            TVal value) {
            var length = keys == null ? 1 : keys.Length + 1;

            Array.Resize(ref keys, length);
            Array.Resize(ref values, length);

            for (var i = length - 1; i > insertIndex; i -= 1) {
                keys[i] = keys[i - 1];
                values[i] = values[i - 1];
            }

            keys[insertIndex] = key;
            values[insertIndex] = value;
        }

        private static int FindKey([CanBeNull] int[] keys, int y) {
            if (keys == null) {
                return ~0;
            }

            return Array.BinarySearch(keys, 0, keys.Length, y);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            if (this.rowKeys != null) {
                Array.Resize(ref this.rows, this.rowKeys.Length);

                for (var i = 0; i < this.rows!.Length; i++) {
                    ref var row = ref this.rows![i];
                    if (row.keys != null) {
                        Array.Resize(ref row.values, row.keys.Length);
                    } else {
                        row.values = Array.Empty<T>();
                    }
                }
            } else {
                this.rows = Array.Empty<RowEntry>();
            }
        }
    }
}
