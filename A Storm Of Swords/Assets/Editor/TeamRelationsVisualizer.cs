using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(VisualizeAttribute))]
public class TeamRelationsVisualizer : PropertyDrawer
{
    // Stores foldout state for each property
    private static readonly Dictionary<string, bool> foldoutStates = new();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        string key = property.propertyPath;
        bool expanded = foldoutStates.ContainsKey(key) && foldoutStates[key];

        if (!expanded)
            return EditorGUIUtility.singleLineHeight + 4;

        VisualizeAttribute att = attribute as VisualizeAttribute;
        float cubeSize = att.cubeSize;

        return EditorGUIUtility.singleLineHeight + att.height * (cubeSize + att.spacing) + 20;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        VisualizeAttribute att = attribute as VisualizeAttribute;
        string key = property.propertyPath;

        if (!foldoutStates.ContainsKey(key))
            foldoutStates[key] = false;

        // Foldout header
        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        foldoutStates[key] = EditorGUI.Foldout(foldoutRect, foldoutStates[key], label, true);

        if (!foldoutStates[key])
            return;

        float yOffset = position.y + EditorGUIUtility.singleLineHeight + 4;
        float xStart = position.x;

        // Determine cube size (auto-fit if enabled)
        float cubeSize = att.cubeSize;
        if (att.autoFit)
        {
            float totalSpacing = (att.width - 1) * att.spacing;
            cubeSize = (position.width - totalSpacing) / att.width;
        }

        // Draw the grid
        for (int row = 0; row < att.height; row++)
        {
            float xOffset = xStart;

            for (int col = 0; col < att.width; col++)
            {
                int index = row * att.width + col;

                // Safety check: skip if array is smaller than expected
                if (index >= property.arraySize)
                    continue;

                SerializedProperty element = property.GetArrayElementAtIndex(index);
                bool bitSet = element.boolValue;

                Rect r = new Rect(xOffset, yOffset, cubeSize, cubeSize);

                GUIStyle style = new GUIStyle(GUI.skin.button);
                if (bitSet)
                    style.normal.textColor = Color.green;

                if (GUI.Button(r, bitSet ? "1" : "0", style))
                {
                    element.boolValue = !bitSet;
                }

                xOffset += cubeSize + att.spacing;
            }

            yOffset += cubeSize + att.spacing;
        }
    }
}
