using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace SpaceTrader.Util {
    public class Pool {
        public struct PoolCollection : IReadOnlyCollection<Component> {
            private readonly LinkedList<Component> list;

            public PoolCollection(LinkedList<Component> list) : this() {
                this.list = list;
            }

            readonly IEnumerator<Component> IEnumerable<Component>.GetEnumerator() {
                return this.GetEnumerator();
            }

            readonly IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }

            public readonly LinkedList<Component>.Enumerator GetEnumerator() {
                return this.list.GetEnumerator();
            }

            public readonly int Count => this.list.Count;
        }

        private readonly LinkedList<Component> active;
        private readonly LinkedList<Component> pooled;

        private readonly Transform transform;

        private readonly Component prefab;

        public PoolCollection Active => new PoolCollection(this.active);
        public PoolCollection Pooled => new PoolCollection(this.pooled);

        public Pool(Component prefab, [CanBeNull] Transform parent, string name = "Pool") {
            this.prefab = prefab;

            this.transform = new GameObject(name).transform;
            this.transform.SetParent(parent, false);
            this.transform.gameObject.SetActive(false);

            this.pooled = new LinkedList<Component>();
            this.active = new LinkedList<Component>();
        }

        public void Reserve(int minSize) {
            var totalSize = this.pooled.Count + this.active.Count;
            while (totalSize < minSize) {
                var newItem = Object.Instantiate(this.prefab, this.transform);
                Debug.Assert(!newItem.gameObject.activeInHierarchy,
                    "items instantiated during Reserve shouldn't be active", newItem);
                this.pooled.AddLast(newItem);
                ++totalSize;
            }
        }

        public void Return(Component item) {
            Debug.Assert(!this.pooled.Contains(item));
            Debug.Assert(this.active.Contains(item));

            item.transform.SetParent(this.transform);

            this.pooled.AddLast(item);
            this.active.Remove(item);
        }

        public Component Get(Transform parent) {
            if (this.pooled.Count == 0) {
                this.Reserve(Mathf.Max(1, this.active.Count * 2));
            }

            var item = this.pooled.First.Value;
            this.pooled.RemoveFirst();
            this.active.AddLast(item);

            item.transform.SetParent(parent);

            return item;
        }

        public void ReturnAll() {
            foreach (var item in this.active) {
                this.Return(item);
            }
        }
    }

    public class Pool<T> : Pool where T : Component {
        public new struct PoolCollection : IReadOnlyCollection<T> {
            public struct Enumerator : IEnumerator<T> {
                private LinkedList<Component>.Enumerator listEnumerator;

                public Enumerator(LinkedList<Component>.Enumerator listEnumerator) {
                    this.listEnumerator = listEnumerator;
                }

                public bool MoveNext() {
                    return this.listEnumerator.MoveNext();
                }

                void IEnumerator.Reset() {
                }

                public T Current => (T)this.listEnumerator.Current;

                object IEnumerator.Current => this.Current;

                public void Dispose() {
                    this.listEnumerator.Dispose();
                }
            }

            private readonly Pool.PoolCollection collection;

            public PoolCollection(Pool.PoolCollection collection) : this() {
                this.collection = collection;
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator() {
                return this.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }

            public readonly Enumerator GetEnumerator() {
                return new Enumerator(this.collection.GetEnumerator());
            }

            public readonly int Count => this.collection.Count;
        }

        public new PoolCollection Active => new PoolCollection(base.Active);
        public new PoolCollection Pooled => new PoolCollection(base.Pooled);

        public Pool(T prefab, [CanBeNull] Transform parent, string name = "Pool") : base(prefab, parent, name) {
        }

        public new T Get(Transform parent) {
            return (T)base.Get(parent);
        }
    }
}
