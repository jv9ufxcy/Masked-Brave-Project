/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour {
    
    private const string SAVE_SEPARATOR = "#SAVE-VALUE#";

    [SerializeField] private GameObject unitGameObject;
    //VideoSettings
    public int resolutionIndex;
    public bool isFullScreen;
    public bool isVSync;
    public bool visibleHealth;
    //AudioSettings
    public float[] masterVolume;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            Load();
        }
    }

    public void Save() {
        // Save
        Vector3 playerPosition = Vector3.one;
        int goldAmount = 0;

        string[] contents = new string[] { 
            ""+goldAmount,
            ""+playerPosition.x,
            ""+playerPosition.y
        };
        string saveString = string.Join(SAVE_SEPARATOR, contents);
        File.WriteAllText(Application.dataPath + "/settings.txt", saveString);

        Debug.Log("Saved!");
    }

    public void Load() {
        // Load
        if (File.Exists(Application.dataPath + "/settings.txt")) {
            string saveString = File.ReadAllText(Application.dataPath + "/settings.txt");
            Debug.Log("Loaded: " + saveString);

            string[] contents = saveString.Split(new[] { SAVE_SEPARATOR }, System.StringSplitOptions.None);

            int goldAmount = int.Parse(contents[0]);
            float playerPositionX = float.Parse(contents[1]);
            float playerPositionY = float.Parse(contents[2]);
            Vector3 playerPosition = new Vector3(playerPositionX, playerPositionY);

            //unit.SetPosition(playerPosition);
            //unit.SetGoldAmount(goldAmount);
        } else {
            Debug.Log("No save");
        }
    }

}
