using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(IndexedItemAttribute))]
public class IndexedItemDrawer : PropertyDrawer
{
    CoreData coreData;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //get the attricbut since it contains the range of the slider
        IndexedItemAttribute indexedItem = attribute as IndexedItemAttribute;
        if (coreData==null)
        {
            foreach (string guid in AssetDatabase.FindAssets("t: CoreData"))//looks at whole project for assets tagged CoreData
            {
                coreData = AssetDatabase.LoadAssetAtPath<CoreData>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        switch (indexedItem.type)
        {
            case IndexedItemAttribute.IndexedItemType.SCRIPTS:
                property.intValue = EditorGUI.IntPopup(position, property.intValue, coreData.GetScriptNames(), null);
                break;
            case IndexedItemAttribute.IndexedItemType.STATES:
                property.intValue = EditorGUI.IntPopup(position, property.intValue, coreData.GetStateNames(), null);
                break;
            default:
                break;
        }
        //base.OnGUI(position, property, label);
    }
}
