using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace SpaceTrader.Util
{
    public class PooledList<TItem, TData> : IEnumerable<TItem>
        where TItem : MonoBehaviour
    {
        public delegate void UpdateItemCallback(TItem existing, TData source);
        public delegate void UpdateItemIndexedCallback(int index, TItem existing, TData source);

        private TItem itemPrefab;

        private Transform root;

        private List<TItem> currentItems;
        private List<TData> currentData;

        public int Count
        {
            get
            {
                if (currentItems == null)
                {
                    return 0;
                }

                return currentItems.Where(i => i.isActiveAndEnabled).Count();
            }
        }

        public int Capacity
        {
            get { return currentItems == null ? 0 : currentItems.Count; }
            set
            {
                if (currentItems == null)
                {
                    currentItems = new List<TItem>(value);
                }
                else
                {
                    currentItems.Capacity = value;
                }

                int diff = value - currentItems.Count;
                for (int i = 0; i < diff; ++i)
                {
                    var item = UnityEngine.Object.Instantiate(itemPrefab);
                    item.transform.SetParent(root);
                    item.gameObject.SetActive(false);
                }
            }
        }

        public TItem this[int index]
        {
            get { return currentItems == null ? null : currentItems[index]; }
        }

        public IEnumerable<TItem> Items
        {
            get { return currentItems != null ? currentItems : Enumerable.Empty<TItem>(); }
        }

        public IEnumerable<TData> Data
        {
            get { return currentData != null ? currentData : Enumerable.Empty<TData>(); }
        }

        public IEnumerable<KeyValuePair<TData, TItem>> Entries
        {
            get
            {
                for (int i = 0; i < currentData.Count; ++i)
                {
                    yield return new KeyValuePair<TData, TItem>(currentData[i], currentItems[i]);
                }
            }
        }

        public PooledList(Transform root, TItem itemPrefab)
        {
            Debug.Assert(root, "transform root of PooledList must exist");
            Debug.Assert(itemPrefab, "item prefab of PooledList must exist");

            currentItems = new List<TItem>(root.GetComponentsInChildren<TItem>());
            currentData = null;
            this.root = root;
            this.itemPrefab = itemPrefab;
        }

        public bool Clear()
        {
            currentItems.ForEach(item => item.gameObject.SetActive(false));

            if (currentData != null && currentData.Count > 0)
            {
                currentData.Clear();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ResizeList<T>(List<T> list, int size)
        {
            while (list.Count > size)
            {
                list.RemoveAt(list.Count - 1);
            }
            while (list.Count < size)
            {
                list.Add(default(T));
            }
        }

        public bool Refresh(IEnumerable<TData> dataItems, UpdateItemCallback onUpdateItem)
        {
            return Refresh(dataItems, (index, item, data) => onUpdateItem(item, data));
        }

        public bool Refresh(IEnumerable<TData> dataItems, UpdateItemIndexedCallback onUpdateItem)
        {
            var newData = dataItems.ToList();

            if (currentData != null && currentData.SequenceEqual(newData))
            {
                //already up to date
                return false;
            }

            currentData = newData;

            int existingItemsCount = currentItems.Count;
            int newCount = currentData.Count;

            ResizeList(currentItems, newCount);

            int itemIndex;
            for (itemIndex = 0; itemIndex < newCount; ++itemIndex)
            {
                var dataValue = currentData[itemIndex];

                TItem item;
                if (itemIndex >= existingItemsCount)
                {
                    item = currentItems[itemIndex] = UnityEngine.Object.Instantiate(itemPrefab, root, false);
                }
                else
                {
                    item = currentItems[itemIndex];
                }

                item.gameObject.SetActive(true);

                if (onUpdateItem != null)
                {
                    onUpdateItem(itemIndex, item, dataValue);
                }
            }

            //if there are less items than previously, deactivate any extras
            while (itemIndex < existingItemsCount)
            {
                currentItems[itemIndex].gameObject.SetActive(false);
                ++itemIndex;
            }

            return true;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            if (currentItems != null)
            {
                return currentItems.GetEnumerator();
            }
            else
            {
                return Enumerable.Empty<TItem>().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
