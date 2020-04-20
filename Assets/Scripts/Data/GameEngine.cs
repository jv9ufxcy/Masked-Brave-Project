using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine : MonoBehaviour
{

    public CoreData coreDataObject;
    public static CoreData coreData;
    public static float hitStop;
    public static GameEngine gameEngine;
	// Use this for initialization
	void Start ()
    {
        coreData = coreDataObject;
        gameEngine = this;
	}
    public static void SetHitPause(float _pow)
    {
        if (_pow > hitStop)
        {
            hitStop = _pow;
        }
    }
	// Update is called once per frame
	void Update ()
    {
        if (hitStop>0)
        {
            hitStop--;
        }
	}
    public static void GlobalPrefab(int _index, GameObject _obj)
    {
        GameObject nextPrefab = Instantiate(coreData.globalPrefabs[_index], _obj.transform.position, Quaternion.identity, _obj.transform.root);
    }


    public InputBuffer playerInputBuffer;
    void OnGUI()
    {
        int xSpace = 20;
        int ySpace = 25;
        //GUI.Label(new Rect(10, 10, 100, 20), "Hello World!");
        for (int i = 0; i < playerInputBuffer.inputList.Count; i++)
        {
            GUI.Label(new Rect(xSpace, i * ySpace, 100, 20), coreData.rawInputs[playerInputBuffer.inputList[i].button] + ":");
            for (int j = 0; j < playerInputBuffer.inputList[i].buffer.Count; j++)
            {
                if (playerInputBuffer.inputList[i].buffer[i].used)
                { GUI.Label(new Rect(j * xSpace + 100, i * ySpace, 100, 20), playerInputBuffer.inputList[i].buffer[j].hold.ToString() + "*"); }
                else { GUI.Label(new Rect(j * xSpace + 100, i * ySpace, 100, 20), playerInputBuffer.inputList[i].buffer[j].hold.ToString()); }
            }
        }
    }
}
