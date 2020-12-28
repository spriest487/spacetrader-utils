#pragma warning disable 0649

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpaceTrader.Util {
    [Serializable]
    public abstract class PooledList<TData, TComponent, TSender> :
        IReadOnlyList<TComponent>
        where TComponent : Component {
        [SerializeField, HideInInspector]
        private List<TComponent> pool;

        [SerializeField]
        private TComponent prefab;

        [SerializeField]
        private Transform root;

        public TComponent Prefab => this.prefab;

        public Transform Root => this.root;

        public IEnumerator<TComponent> GetEnumerator() {
            return this.pool.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public TComponent this[int index] => this.pool[index];

        public int Count => this.pool.Count;

        public int ActiveCount {
            get {
                var count = 0;
                foreach (var item in this.pool) {
                    if (item.gameObject.activeSelf) {
                        count += 1;
                    }
                }

                return count;
            }
        }

        protected abstract void Initialize(
            TData item,
            TComponent component,
            int index,
            TSender sender
        );

        protected virtual TComponent Instantiate(TSender sender, Transform root) {
            return Object.Instantiate(this.prefab, root);
        }

        protected virtual bool Include(TData item, TSender sender) {
            return true;
        }

        /// <summary>
        /// Destroy all TComponent instances under the Root's hierarchy that are not already in the pool
        /// </summary>
        public void CleanupRoot() {
            if (!Application.isPlaying) {
                Debug.LogAssertion("only call CleanupRoot in play mode");
                return;
            }

            var instances = this.root.GetComponentsInChildren<TComponent>();
            foreach (var instance in instances) {
                if (!this.pool.Contains(instance)) {
                    Object.Destroy(instance.gameObject);
                }
            }
        }

        /// <summary>
        /// Add all children that exist in the hierarchy under the Root and that are not already members of the
        /// pool to the pool as disabled instances
        /// </summary>
        public void AddExistingChildren() {
            foreach (var instance in this.root.GetComponentsInChildren<TComponent>(true)) {
                if (this.pool.Contains(instance)) {
                    continue;
                }

                this.pool.Add(instance);
                instance.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Populate the root with enough instances to represent each item in the source. Initializes each instance
        /// with a corresponding item from the source. Disables all instances which are not needed to display the
        /// current number of items in the source.
        /// </summary>
        public void Populate<TSrc>(TSrc source, TSender sender)
            where TSrc : IEnumerable<TData> {
            var index = 0;

            foreach (var item in source) {
                if (!this.Include(item, sender)) {
                    continue;
                }

                TComponent component;
                if (index < this.pool.Count) {
                    component = this.pool[index];
                } else {
                    component = this.InstantiateElement(sender);
                    this.pool.Add(component);
                }

                component.gameObject.SetActive(true);
                this.Initialize(item, component, index, sender);

                ++index;
            }

            while (index < this.pool.Count) {
                this.pool[index].gameObject.SetActive(false);
                ++index;
            }
        }

        /// <summary>
        /// Return all active component instances to the pool
        /// </summary>
        public void Empty() {
            foreach (var item in this.pool) {
                item.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Empty the pool storage and destroy all component instances
        /// </summary>
        public void Clear() {
            foreach (var item in this.pool) {
                if (item) {
                    if (Application.isPlaying) {
                        Object.Destroy(item.gameObject);
                    } else {
                        Object.DestroyImmediate(item.gameObject);
                    }
                }
            }

            this.pool.Clear();
        }

        /// <summary>
        /// Add a single item item to the pool, either instantiating a new instance or initializing an existing one.
        /// </summary>
        public TComponent Add(TData data, TSender sender) {
            var index = this.pool.FindIndex(it => !it.gameObject.activeSelf);
            TComponent component;
            if (index < 0) {
                component = this.InstantiateElement(sender);

                this.pool.Add(component);
                index = this.pool.Count - 1;
            } else {
                component = this.pool[index];
            }

            component.gameObject.SetActive(true);
            this.Initialize(data, component, index, sender);

            return component;
        }

        private TComponent InstantiateElement(TSender sender) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                var obj = (TComponent)PrefabUtility.InstantiatePrefab(
                    this.prefab
                );
                obj.transform.SetParent(this.root);
                return obj;
            }
#endif

            return this.Instantiate(sender, this.root);
        }
    }
}
