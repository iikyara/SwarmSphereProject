using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Range2Attribute))]
public class Range2Drawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    Range2Attribute range2Attribute = (Range2Attribute)attribute;

    if (property.propertyType == SerializedPropertyType.Float)
    {
      EditorGUI.Slider(position, property, range2Attribute.min, range2Attribute.max, label);
    }
  }
}
