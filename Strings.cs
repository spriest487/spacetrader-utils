using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SpaceTrader.Util {
    public class Strings : ScriptableObject, IReadOnlyDictionary<string, string> {
        [Serializable]
        private struct Entry {
            public string key;
            public string value;
        }

        [SerializeField]
        private Entry[] entries = Array.Empty<Entry>();

        [field: SerializeField]
        public string MissingKeyFormat { get; set; } = "";

        private Dictionary<string, string> map;

        public string this[string key, params object[] args] {
            get {
                if (this.TryGetValue(key, out var formatString)) {
                    return string.Format(formatString, args);
                }

                return key;
            }
        }

        public string this[string key] {
            get {
                if (this.TryGetValue(key, out var val)) {
                    return val;
                }

                var lastGroupIndex = key.LastIndexOf('.') + 1;
                var lastGroup = key[lastGroupIndex..];
                return string.Format(this.MissingKeyFormat, lastGroup);
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
            return this.map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public bool ContainsKey(string key) {
            return this.map.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string value) {
            if (string.IsNullOrEmpty(key)) {
                value = "";
                return true;
            }

            if (this.map != null) {
                return this.map.TryGetValue(key, out value);
            }

            var index = this.entries.FindIndex(e => e.key == key);
            value = index >= 0 ? this.entries[index].value : "";
            return index >= 0;
        }

        public int Count => this.map.Count;
        public IEnumerable<string> Keys => this.map.Keys;
        public IEnumerable<string> Values => this.map.Values;

        public static Strings Create(IReadOnlyDictionary<string, string> map) {
            var instance = CreateInstance<Strings>();
            var entries = map.Select(entry => new Entry {
                    key = entry.Key,
                    value = entry.Value
                })
                .ToArray();

            instance.entries = entries;
            instance.OnEnable();
            return instance;
        }

        private void OnEnable() {
            this.map = new Dictionary<string, string>();

            foreach (var entry in this.entries) {
                this.map.Add(entry.key, entry.value);
            }
        }

        public void Format(StringBuilder text, string key, params object[] args) {
            if (this.TryGetValue(key, out var formatString)) {
                text.AppendFormat(formatString, args);
            } else {
                text.Append(key);
            }
        }

        private void OnValidate() {
            this.OnEnable();
        }
    }
}
