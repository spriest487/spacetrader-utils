using System.Collections.Generic;
using UnityEngine;

namespace PhoenixQuest
{
    public class Pool<T> where T : Component
    {
        private readonly List<T> pooled;
        private readonly List<T> active;

        private readonly T prefab;
        private readonly Transform parent;

        public Pool(T prefab, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;

            pooled = new List<T>();
            active = new List<T>();
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
