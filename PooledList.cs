#pragma warning disable 0649

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpaceTrader.Util {
    [Serializable]
    public abstract class PooledList<TData, TComponent, TSender> :
        IReadOnlyList<TComponent>
        where TComponent : Component {
        [SerializeField]
        private TComponent prefab;

        public TComponent Prefab => prefab;

        [SerializeField]
        private Transform root;

        public Transform Root => root;

        [SerializeField, HideInInspector]
        private List<TComponent> pool;

        protected abstract void Initialize(
            TData item,
            TComponent component,
            int index,
            TSender sender
        );

        protected virtual TComponent Instantiate(TSender sender, Transform root) {
            return Object.Instantiate(this.prefab, root);
        }

        public void CleanupRoot() {
            if (!Application.isPlaying) {
                Debug.LogAssertion("only call CleanupRoot in play mode");
                return;
            }

            var instances = root.GetComponentsInChildren<TComponent>();
            foreach (var instance in instances) {
                if (!pool.Contains(instance)) {
                    Object.Destroy(instance.gameObject);
                }
            }
        }

        public void Populate(
            IEnumerable<TData> data,
            TSender sender
        ) {
            var index = 0;
            foreach (var item in data) {
                TComponent component;
                if (index < pool.Count) {
                    component = pool[index];
                } else {
                    component = InstantiateElement(sender);
                    pool.Add(component);
                }

                component.gameObject.SetActive(true);
                Initialize(item, component, index, sender);

                ++index;
            }

            while (index < pool.Count) {
                pool[index].gameObject.SetActive(false);
                ++index;
            }
        }

        /// <summary>
        /// Return all component instances to the pool
        /// </summary>
        public void Empty() {
            foreach (var item in pool) {
                item.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Empty the pool storage and destroy all component instances
        /// </summary>
        public void Clear() {
            foreach (var item in pool) {
                if (item) {
                    if (Application.isPlaying) {
                        Object.Destroy(item.gameObject);
                    } else {
                        Object.DestroyImmediate(item.gameObject);
                    }
                }
            }

            pool.Clear();
        }

        public TComponent Add(TData data, TSender sender) {
            var index = pool.FindIndex(it => !it.gameObject.activeSelf);
            TComponent component;
            if (index < 0) {
                component = InstantiateElement(sender);

                pool.Add(component);
                index = pool.Count - 1;
            } else {
                component = pool[index];
            }

            component.gameObject.SetActive(true);
            Initialize(data, component, index, sender);

            return component;
        }

        private TComponent InstantiateElement(TSender sender) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                var obj = (TComponent)UnityEditor.PrefabUtility.InstantiatePrefab(
                    prefab
                );
                obj.transform.SetParent(root);
                return obj;
            }
#endif

            return Instantiate(sender, root);
        }

        public void Remove(TComponent item) {
            pool.Single(it => it == item).gameObject.SetActive(false);
        }

        public void RemoveAll(Func<TComponent, bool> predicate) {
            foreach (var removed in pool.Where(predicate)) {
                removed.gameObject.SetActive(false);
            }
        }

        public int IndexOf(TComponent component) => pool.IndexOf(component);

        public IEnumerator<TComponent> GetEnumerator() => pool.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TComponent this[int index] => pool[index];

        public int Count => pool.Count;
    }
}