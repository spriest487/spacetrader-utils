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

        [SerializeField]
        private Sprite defaultSprite;
        public Sprite DefaultSprite => defaultSprite;

        [SerializeField]
        private Entry[] entries;
        public const string EntriesProperty = nameof(entries);
        public const string EntrySpriteProperty = nameof(Entry.sprite);

        private IReadOnlyDictionary<string, Sprite> spritesByTag;

        public IEnumerable<string> Keys => spritesByTag.Keys;
        public IEnumerable<Sprite> Values => spritesByTag.Values;
        public int Count => spritesByTag.Count;

        public Sprite this[string key] {
            get {
                if (TryGetValue(key, out var sprite)) {
                    return sprite;
                } else {
                    return defaultSprite;
                }
            }
        }

        private void OnEnable() {
            if (entries != null) {
                spritesByTag = entries.ToDictionary(e => e.tag, e => e.sprite);
            } else {
                spritesByTag = new Dictionary<string, Sprite>();
            }
        }

        public bool ContainsKey(string key) {
            return spritesByTag.ContainsKey(key);
        }

        public bool TryGetValue(string key, out Sprite value) {
            if (string.IsNullOrEmpty(key)) {
                value = defaultSprite;
                return true;
            }

            return spritesByTag.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, Sprite>> GetEnumerator() {
            return spritesByTag.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}