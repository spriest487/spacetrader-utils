#if UNITY_EDITOR

using Teragram.Squadron.BattleMode;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SpaceTrader.Util {
    [CustomPropertyDrawer(typeof(GridDictionary<>))]
    public class GridDictionaryDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var foldout = new Foldout { text = property.displayName };

            using var rowKeysProp = property.FindPropertyRelative("rowKeys");
            using var rowsProp = property.FindPropertyRelative("rows");

            var rowCount = rowKeysProp.arraySize;
            for (var rowIndex = 0; rowIndex < rowCount; rowIndex += 1) {
                var y = rowKeysProp.GetArrayElementAtIndex(rowIndex).intValue;

                var row = rowsProp.GetArrayElementAtIndex(rowIndex);

                using var keysProp = row.FindPropertyRelative("keys");
                using var valuesProp = row.FindPropertyRelative("values");

                for (var colIndex = 0; colIndex < keysProp.arraySize; colIndex += 1) {
                    var x = keysProp.GetArrayElementAtIndex(colIndex).intValue;

                    using var valueProp = valuesProp.GetArrayElementAtIndex(colIndex);

                    var propertyElement = new PropertyField(valueProp) {
                        label = $"{x}, {y}",
                    };
                    
                    foldout.Add(propertyElement);
                }
            }

            return foldout;
        }
    }
}

#endif
