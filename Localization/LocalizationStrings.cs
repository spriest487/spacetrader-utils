using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using SpaceTrader.Util;
using UnityEditor;
using UnityEngine;

namespace Teragram.Util {
    [CreateAssetMenu(menuName = "SpaceTrader/Localization Strings")]
    public class LocalizationStrings : ScriptableObject {
        [field: SerializeField]
        public Strings DefaultLanguage { get; private set; }

        [field: SerializeField]
        internal string scriptPath = "Assets/StringKeys.cs";
        
        [field: SerializeField]
        internal string scriptNamespace = "";

        public virtual Strings CurrentLanguage => this.DefaultLanguage;

#if UNITY_EDITOR
        protected void Reset() {
            this.scriptNamespace = EditorSettings.projectGenerationRootNamespace;
        }

        public static IEnumerable<ValueDropdownItem<string>> DefaultLanguageDropdown(
            [CanBeNull] Strings defaultLanguage
        ) {
            yield return new ValueDropdownItem<string>("", "");

            if (defaultLanguage == null) {
                yield break;
            }

            var keyPathBuilder = new StringBuilder();
            
            foreach (var (key, _) in defaultLanguage) {
                keyPathBuilder.Clear();
                keyPathBuilder.Append(key);
                keyPathBuilder.Replace('.', '/');

                yield return new ValueDropdownItem<string>(keyPathBuilder.ToString(), key);
            }
        }
#endif
    }
}
