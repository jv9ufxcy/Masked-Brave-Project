using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class IndexedItemAttribute : PropertyAttribute
{
    public enum IndexedItemType { SCRIPTS, STATES }
    public IndexedItemType type;

    public IndexedItemAttribute(IndexedItemType type)
    {
        this.type = type;
    }

    
}
