using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TappGo.Widgets.Editor
{
    /// <summary>
    /// Draws <see cref="TappWidgetsSettings.iosWidgetSizes"/> as a flags field labelled "Widget sizes (iOS)".
    /// </summary>
    /// <remarks>
    /// Core's settings page draws every field of a product's asset as a stock property field, which would label
    /// this one "Ios Widget Sizes". A drawer for the type is how this package names it without a change to core;
    /// the tooltip is the field's own, so it is written once.
    /// </remarks>
    [CustomPropertyDrawer(typeof(TappWidgetSizes))]
    internal sealed class TappWidgetSizesDrawer : PropertyDrawer
    {
        internal const string Label = "Widget sizes (iOS)";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = new EnumFlagsField(Label, (TappWidgetSizes)property.intValue)
            {
                bindingPath = property.propertyPath,
                tooltip = property.tooltip,
            };
            field.AddToClassList(BaseField<System.Enum>.alignedFieldUssClassName);
            return field;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                EditorGUI.BeginChangeCheck();
                var chosen = EditorGUI.EnumFlagsField(position, new GUIContent(Label, property.tooltip),
                    (TappWidgetSizes)property.intValue);
                if (EditorGUI.EndChangeCheck())
                {
                    property.intValue = (int)(TappWidgetSizes)chosen;
                }
            }
        }
    }
}
