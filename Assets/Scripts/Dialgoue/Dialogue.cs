using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "Dialogue/DialogueObject")]
[System.Serializable]
public class Dialogue
{
    //public List<Dialogue> line= new List<Dialogue>();
    [SerializeField]
    private Sprite _profile,_cutscene;
    [SerializeField]
    private string _name;

    [SerializeField]
    [TextArea(3,10)]
    private string _sentences;
    public string Name => _name;
    public Sprite Profile => _profile;
    public Sprite Cutscene=>_cutscene;
    public string Sentences => _sentences;
}
