using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseCondition : MonoBehaviour
{
    BattleSystem bossBattle;
    [SerializeField] DialogueTrigger lossDialogue;
    // Start is called before the first frame update
    void Start()
    {
        bossBattle = GetComponent<BattleSystem>();
    }

    public void CheckDefeatEvent()
    {
        HealthManager playerHealth = GameEngine.gameEngine.mainCharacter.healthManager;
        playerHealth.OnLastChance += PlayerHealth_OnLastChance;
    }

    private void PlayerHealth_OnLastChance(object sender, System.EventArgs e)
    {
        Debug.Log("Instant loss");
        bossBattle.EndBattle();
        lossDialogue.BeginDialogue();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
