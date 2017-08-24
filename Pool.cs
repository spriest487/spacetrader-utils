using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Util
{
    public class Pool<T> where T : Component
    {
        private readonly List<T> pooled;
        private readonly List<T> active;

        private readonly T prefab;
        private readonly Transform parent;

        public Pool(T prefab, Transform parent)
        {
            Debug.Assert(!prefab.gameObject.activeSelf, "source prefab for Pool shouldn't be active", prefab);
        
            this.prefab = prefab;
            this.parent = parent;

            pooled = new List<T>();
            active = new List<T>();
        }
        
        public void Reserve(int minSize)
        {
            var totalSize = pooled.Count + active.Count;
            while (totalSize < minSize)
            {
                var newItem = Object.Instantiate(prefab, parent);
                Debug.Assert(!newItem.gameObject.activeInHierarchy, "items instantiated during Reserve shouldn't be active", newItem);
                pooled.Add(newItem);
                ++totalSize;
            }
        }
        
        public void Return(T item)
        {
            Debug.Assert(!pooled.Contains(item));
            Debug.Assert(active.Contains(item));

            item.gameObject.SetActive(false);
            pooled.Add(item);
            active.Remove(item);
        }

        public T Get()
        {
            T item;
            if (pooled.Count > 0)
            {
                item = pooled[0];
                pooled.RemoveAt(0);
            }
            else
            {
                item = Object.Instantiate(prefab, parent);
            }

            active.Add(item);
            item.gameObject.SetActive(true);

            return item;
        }
    }
}
