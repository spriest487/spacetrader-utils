#pragma warning disable 0649

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceTrader.Util {
    [CreateAssetMenu(menuName = "SpaceTrader/Sprite Map")]
    public class SpriteMap : ScriptableObject, IReadOnlyDictionary<string, Sprite> {
        [Serializable]
        private struct Entry {
            public string tag;
            public Sprite sprite;
        }

        public const string EntriesProperty = nameof(entries);
        public const string EntrySpriteProperty = nameof(Entry.sprite);

        [SerializeField]
        private Sprite defaultSprite;

        [SerializeField]
        private Entry[] entries;

        private IReadOnlyDictionary<string, Sprite> spritesByTag;
        public Sprite DefaultSprite => this.defaultSprite;

        public IEnumerable<string> Keys => this.spritesByTag.Keys;
        public IEnumerable<Sprite> Values => this.spritesByTag.Values;
        public int Count => this.spritesByTag.Count;

        public Sprite this[string key] {
            get {
                if (this.TryGetValue(key, out var sprite)) {
                    return sprite;
                }

                return this.defaultSprite;
            }
        }

        public bool ContainsKey(string key) {
            return this.spritesByTag.ContainsKey(key);
        }

        public bool TryGetValue(string key, out Sprite value) {
            if (string.IsNullOrEmpty(key)) {
                value = this.defaultSprite;
                return true;
            }

            return this.spritesByTag.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, Sprite>> GetEnumerator() {
            return this.spritesByTag.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        private void OnEnable() {
            if (this.entries != null) {
                this.spritesByTag = this.entries.ToDictionary(e => e.tag, e => e.sprite);
            } else {
                this.spritesByTag = new Dictionary<string, Sprite>();
            }
        }
    }
}