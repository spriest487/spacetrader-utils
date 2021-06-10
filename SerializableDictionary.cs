using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace SpaceTrader.Util {
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
        [SerializeField]
        private List<TKey> keys;

        [SerializeField]
        private List<TValue> values;

        public SerializableDictionary() {
            this.keys = new List<TKey>();
            this.values = new List<TValue>();
        }

        public SerializableDictionary(IDictionary<TKey, TValue> input) : base(input) {
            this.keys = new List<TKey>();
            this.values = new List<TValue>();
        }

        public void OnBeforeSerialize() {
            this.keys.Clear();
            this.values.Clear();

            foreach (var (key, value) in this) {
                this.keys.Add(key);
                this.values.Add(value);
            }
        }

        public void OnAfterDeserialize() {
            this.Clear();

            if (this.keys.Count != this.values.Count) {
                throw new SerializationException(
                    $"there are {this.keys.Count} keys and {this.values.Count} "
                    + "values after deserialization. Make sure that both key and value types are serializable."
                );
            }

            for (var i = 0; i < this.keys.Count; i++) {
                this.Add(this.keys[i], this.values[i]);
            }
        }
    }
}
