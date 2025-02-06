using QuizSystem.Custom;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConditionalPropertyAttribute))]
public class ConditionalPropertyDrawer : PropertyDrawer
{

    // Determine whether this field should be visible.
    // (We could probably do some caching here...)
    bool ShouldShow(SerializedProperty property)
    {
        var conditionAttribute = (ConditionalPropertyAttribute)attribute;
        string conditionPath = conditionAttribute.conditionPath;

        // If this property is defined inside a nested type 
        // (like a struct inside a MonoBehaviour), look for
        // our condition field inside the same nested instance.
        string thisPropertyPath = property.propertyPath;
        int last = thisPropertyPath.LastIndexOf('.');
        if (last > 0)
        {
            string containerPath = thisPropertyPath.Substring(0, last + 1);
            conditionPath = containerPath + conditionPath;
        }

        // Get the SerializedProperty representing the field that is our criterion.
        var conditionProperty = property.serializedObject.FindProperty(conditionPath);

        // For now, we'll only support bool criteria, and default to visible if there's a problem.
        if (conditionProperty == null)
        {
            return true;
        }

        // Use the condition property's boolean or enum value to drive visibility.
        if (conditionProperty.propertyType == SerializedPropertyType.Boolean)
        {
            return conditionProperty.boolValue;
        }

        // Use the condition property's enum value to drive visibility.
        else if (conditionProperty.propertyType == SerializedPropertyType.Enum)
        {
            // Get the enum value index for the specified enum value in the attribute.
            int enumValueIndex = conditionAttribute.GetConditionEnumIndex();
            
            // Check if the enum value index matches the current enum value index of the condition property.
            return conditionProperty.enumValueIndex == enumValueIndex;
        }


        return true;
        // Use the condition property's boolean value to drive visibility.
        //return conditionProperty.boolValue;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (ShouldShow(property))
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (ShouldShow(property))
        {
            // Provision the normal vertical spacing for this control.
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        else
        {
            // Collapse the unseen derived property.
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }
}