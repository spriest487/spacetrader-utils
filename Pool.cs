using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Util {
    public class Pool<T> where T : Component {
        private readonly List<T> active;
        private readonly Transform parent;
        private readonly List<T> pooled;

        private readonly T prefab;

        public Pool(T prefab, Transform parent) {
            Debug.Assert(!prefab.gameObject.activeSelf, "source prefab for Pool shouldn't be active", prefab);

            this.prefab = prefab;
            this.parent = parent;

            this.pooled = new List<T>();
            this.active = new List<T>();
        }

        public void Reserve(int minSize) {
            var totalSize = this.pooled.Count + this.active.Count;
            while (totalSize < minSize) {
                var newItem = Object.Instantiate(this.prefab, this.parent);
                Debug.Assert(!newItem.gameObject.activeInHierarchy,
                    "items instantiated during Reserve shouldn't be active", newItem);
                this.pooled.Add(newItem);
                ++totalSize;
            }
        }

        public void Return(T item) {
            Debug.Assert(!this.pooled.Contains(item));
            Debug.Assert(this.active.Contains(item));

            item.gameObject.SetActive(false);
            this.pooled.Add(item);
            this.active.Remove(item);
        }

        public T Get() {
            T item;
            if (this.pooled.Count > 0) {
                item = this.pooled[0];
                this.pooled.RemoveAt(0);
            } else {
                item = Object.Instantiate(this.prefab, this.parent);
            }

            this.active.Add(item);
            item.gameObject.SetActive(true);

            return item;
        }
    }
}