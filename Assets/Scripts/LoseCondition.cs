using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseCondition : MonoBehaviour
{
    public static LoseCondition instance;
    [SerializeField] BattleSystem bossBattle;
    [SerializeField] DialogueTrigger lossDialogue;
    private bool hasLost = false;
    private Scene scene;

    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);

        // This makes sure it is added only once
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Add the listener to be called when a scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void Start()
    {
        //bossBattle = GetComponent<BattleSystem>();
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
        hasLost = true;
        if (hasLost)
        {
            DontDestroyOnLoad(instance);
        }
    }

    private void CheckLossForTempoFormLoss()
    {
        if (hasLost)
        {
            GameEngine.gameEngine.localSkillsList.Remove("Henshin");
            GameEngine.gameEngine.localSkillsList.Remove("Zoe/BraveHenshin");
            Debug.Log(GameEngine.gameEngine.localSkillsList);
        }
    }
    public void RestoreLostForms()
    {
        GameEngine.gameEngine.localSkillsList.Add("Henshin");
        GameEngine.gameEngine.localSkillsList.Add("Zoe/BraveHenshin");
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!string.Equals(scene.path, this.scene.path)) return;
        CheckLossForTempoFormLoss();
        GameEngine.gameEngine.mainCharacter.healthManager.OnLastChance -= PlayerHealth_OnLastChance;
    }
    private void OnDestroy()
    {
        RestoreLostForms();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEngine.gameEngine.mainCharacter.healthManager.OnLastChance -= PlayerHealth_OnLastChance;
    }
}
