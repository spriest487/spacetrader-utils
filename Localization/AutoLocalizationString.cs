using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Teragram.Util {
    public class AutoLocalizationString : MonoBehaviour {
        private static readonly List<AutoLocalizationString> instances = new List<AutoLocalizationString>();

        private static List<ValueDropdownItem<string>> valueDropdownItems = new List<ValueDropdownItem<string>>();

        [field: SerializeField]
        [field: ValueDropdown("valueDropdownItems")]
        public string Key { get; private set; }
        
        [field: SerializeField]
        public UnityEvent<string> OnLocalized { get; private set; }

        private void OnEnable() {
            instances.Add(this);
        }

        private void OnDisable() {
            instances.Remove(this);
        }

        public static void Apply(LocalizationStrings localizationStrings) {
            var language = localizationStrings.CurrentLanguage;
            
            foreach (var instance in instances) {
                if (!string.IsNullOrWhiteSpace(instance.Key)) {
                    var translation = language[instance.Key];
                    instance.OnLocalized.Invoke(translation);
                }
            }
        }

#if UNITY_EDITOR
        public static void SetValueDropdownItems(IEnumerable<ValueDropdownItem<string>> items) {
            valueDropdownItems.Clear();
            valueDropdownItems.AddRange(items);
        }
        
        [InitializeOnEnterPlayMode]
        private static void InitializeOnPlay() {
            instances.Clear();
        }
#endif
    }
}
