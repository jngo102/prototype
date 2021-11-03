using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TypePopupAttribute))]
public sealed class TypePopupDrawer : PropertyDrawer
{
    private bool _expanded;
    private List<Type> _typesCache;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference)
            return;

        var type = GetManagedType(property);
        var types = GetAssignableTypes(type);

        string[] labels = types.Select(GetTypeLabel).ToArray();
        string[] typenames = types.Select(GetTypeName).ToArray();

        // Get the type of serialized object 

        var currentTypeIndex = Array.IndexOf(typenames, property.managedReferenceFullTypename);
        var currentType = types[currentTypeIndex];

        int selectedTypeIndex = EditorGUI.Popup(GetPopupPosition(position), currentTypeIndex, labels);
        if (selectedTypeIndex >= 0 && selectedTypeIndex < types.Count)
        {
            if (currentType != types[selectedTypeIndex])
            {
                if (types[selectedTypeIndex] == null)
                    property.managedReferenceValue = null;
                else
                    property.managedReferenceValue = Activator.CreateInstance(types[selectedTypeIndex]);

                currentType = types[selectedTypeIndex];
            }
        }

        if (_expanded == false)
        {
            _expanded = true;
            property.isExpanded = false;
        }

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }

    private Rect GetPopupPosition(Rect pos)
    {
        Rect popupPos = new Rect(pos);
        popupPos.x += EditorGUIUtility.labelWidth;
        popupPos.width -= EditorGUIUtility.labelWidth;
        popupPos.height = EditorGUIUtility.singleLineHeight;
        return popupPos;
    }

    private string GetTypeLabel(Type type)
    {
        if (type == null)
            return "(None)";
        
        var label = (TypePopupLabelAttribute)Attribute.GetCustomAttribute(type, typeof(TypePopupLabelAttribute));
        if (label != null)
            return label.Label;
        
        return type.ToString();
    }

    // See https://docs.unity3d.com/ScriptReference/SerializedProperty-managedReferenceFullTypename.html
    private string GetTypeName(Type type)
    {
        if (type == null)
            return string.Empty;

        var assembly = type.Assembly.ToString().Split(',')[0];

        return string.Format("{0} {1}", assembly, type.FullName);
    }

    private Type GetManagedType(SerializedProperty property)
    {
        var fieldTypeNameSplit = property.managedReferenceFieldTypename.Split(' ');
        
        var fieldAssemblyName = fieldTypeNameSplit[0];
        var fieldTypeName = fieldTypeNameSplit[1];

        var fieldAssembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.ToString().Split(',')[0] == fieldAssemblyName);
        var fieldType = fieldAssembly.GetType(fieldTypeName, true);

        return fieldType;
    }

    private List<Type> GetAssignableTypes(Type baseType)
    {
        if (_typesCache != null)
            return _typesCache;

        _typesCache = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => baseType.IsAssignableFrom(p) && p.IsClass)
            .ToList();

        _typesCache.Insert(0, null);

        return _typesCache;
    }
}