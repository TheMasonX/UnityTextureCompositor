using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

// Tell the RangeDrawer that it is a drawer for properties with the RangeAttribute.

[CustomPropertyDrawer(typeof(ReadWriteEnabledAttribute))]
public class ReadWriteEnabledDrawer : PropertyDrawer
{
    private bool initialized;

    // Draw the property inside the given rect
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        ReadWriteEnabledAttribute update = attribute as ReadWriteEnabledAttribute;

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(position, property, label, true);
        if (EditorGUI.EndChangeCheck())
        {
            if (property.objectReferenceValue != null && property.propertyType == SerializedPropertyType.ObjectReference)
            {
                var type = property.objectReferenceValue.GetType();
                if (type == typeof(Texture2D) || type.IsSubclassOf(typeof(Texture2D)))
                {
                    Texture2D tex = (Texture2D)property.objectReferenceValue;
                    string path = AssetDatabase.GetAssetPath(tex);
                    TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
                    if (importer.isReadable && importer.textureFormat == TextureImporterFormat.ARGB32)
                        return;

                    importer.isReadable = true;
                    importer.textureFormat = TextureImporterFormat.ARGB32;
                    Debug.Log(string.Format("Importer Modified For Texture Property {0}", property.displayName));
                    importer.SaveAndReimport();
                }
            }
        }
    }
}