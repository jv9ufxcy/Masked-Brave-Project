using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseCondition : MonoBehaviour
{
    public static LoseCondition instance;
    [SerializeField] BattleSystem bossBattle;
    [SerializeField] DialogueTrigger lossDialogue;
    public bool hasLost = false;
    private Scene scene;
    private string defaultSceneName = "IntroStage", nextSceneName = "SewerStage";
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);
    }
    private void OnEnable()
    {
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
        Debug.Log("Checking in " + SceneManager.GetActiveScene().name + " if the player has lost: " + hasLost);
        if (hasLost)
        {
            GameEngine.gameEngine.localSkillsList.Remove("Henshin");
            GameEngine.gameEngine.localSkillsList.Remove("Zoe/BraveHenshin");
            Debug.Log(GameEngine.gameEngine.localSkillsList);
        }
    }
    public void RestoreLostForms()
    {
        if(!GameEngine.gameEngine.localSkillsList.Contains("Henshin"))
            GameEngine.gameEngine.localSkillsList.Add("Henshin");
        if (!GameEngine.gameEngine.localSkillsList.Contains("Zoe/BraveHenshin"))
            GameEngine.gameEngine.localSkillsList.Add("Zoe/BraveHenshin");
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //if (!string.Equals(scene.path, this.scene.path)) return;
        
        if (string.Equals(scene.name,nextSceneName))//if sewer stage
        {
            CheckLossForTempoFormLoss();
            if (Mission.instance != null)
            {
                Mission.instance.SetSpecialCase(true);//change the default form to bomba
            }
            GameEngine.gameEngine.mainCharacter.healthManager.OnLastChance -= PlayerHealth_OnLastChance;
        }
        else if (string.Equals(scene.name, defaultSceneName))//if intro stage
        {
            RestoreLostForms();
        }
        else
        {
            RestoreLostForms();
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        RestoreLostForms();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEngine.gameEngine.mainCharacter.healthManager.OnLastChance -= PlayerHealth_OnLastChance;
    }
}
