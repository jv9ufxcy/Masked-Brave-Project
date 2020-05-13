using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterStateEditorWindow : EditorWindow
{
    [MenuItem("Window/Character State Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(CharacterStateEditorWindow), false, "Character State Editor");
    }
    CoreData coreData;
    CharacterState currentCharacterState;
    int currentStateIndex;
    private void OnGUI()
    {
        if (coreData == null)
        {
            foreach (string guid in AssetDatabase.FindAssets("t: CoreData"))
            {
                coreData = AssetDatabase.LoadAssetAtPath<CoreData>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(currentStateIndex.ToString() + " | " + currentCharacterState.stateName);
        currentStateIndex = EditorGUILayout.Popup(currentStateIndex, coreData.GetStateNames());
        currentCharacterState = coreData.characterStates[currentStateIndex];

        EditorGUILayout.EndHorizontal();

        EditorUtility.SetDirty(coreData);
    }
}
