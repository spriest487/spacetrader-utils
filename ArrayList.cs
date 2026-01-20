#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;

namespace SpaceTrader.Util {
    public class ArrayList<T> : IReadOnlyList<T> {
        private const int InitialCapacity = 4;
        private const float CapacityGrowthFactor = 1.5f;

        private T[] array;

        public int Count { get; private set; }

        public int Capacity => this.array.Length;

        public ref readonly T this[int index] {
            get {
                if (index < 0 || index >= this.Count) {
                    throw new IndexOutOfRangeException($"accessing index {index} of list with size {this.Count}");
                }

                return ref this.array[index];
            }
        }

        public static implicit operator Span<T>(ArrayList<T> list) {
            return list.array.AsSpan(0, list.Count);
        }
        
        public static implicit operator ReadOnlySpan<T>(ArrayList<T> list) {
            return list.array.AsSpan(0, list.Count);
        }

        public ArrayList() {
            this.array = Array.Empty<T>();
            this.Count = 0;
        }

        T IReadOnlyList<T>.this[int index] => this[index];

        public void Add(T item) {
            this.EnsureCapacity(this.Count + 1);
            this.array[this.Count] = item;
            
            this.Count += 1;
        }

        public void RemoveAt(int index) {
            if (index < 0 || index >= this.Count) {
                throw new IndexOutOfRangeException($"removing index {index} of list with size {this.Count}");
            }
            
            for (var i = index; i < Count - 1; i += 1) {
                this.array[i] = this.array[i + 1];
            }

            this.array[this.Count - 1] = default!;
            
            this.Count -= 1;
        }

        public bool Remove(T item) {
            var index = this.IndexOf(item);
            if (index == -1) {
                return false;
            }
            
            this.RemoveAt(index);
            return true;
        }

        public int IndexOf(T item) {
            return Array.IndexOf(this.array, item, 0, this.Count);
        }

        private void EnsureCapacity(int capacity) {
            var currentCapacity = this.Capacity;
            if (capacity <= this.Capacity) {
                return;
            }

            var newCapacity = currentCapacity == 0 ? InitialCapacity : (int)Math.Ceiling(currentCapacity * CapacityGrowthFactor);
            Array.Resize(ref this.array, newCapacity);
        }

        public IEnumerator<T> GetEnumerator() {
            return new ArrayListEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public Span<T> AsSpan() {
            return this.array.AsSpan(this.Count);
        }
    }

    public struct ArrayListEnumerator<T> : IEnumerator<T> {
        private readonly ArrayList<T> list;
        private int index;

        public readonly T Current => this.list[this.index];
        readonly object? IEnumerator.Current => this.Current;

        public ArrayListEnumerator(ArrayList<T> list) {
            this.list = list;
            this.index = -1;
        }

        public bool MoveNext() {
            if (this.index + 1 >= this.list.Count) {
                return false;
            }

            this.index += 1;
            return true;
        }

        public void Reset() {
            this.index = -1;
        }

        readonly void IDisposable.Dispose() {
        }
    }
}
