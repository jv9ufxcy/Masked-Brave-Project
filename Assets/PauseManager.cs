using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    GameManager GM;
    public static PauseManager pauseManager;
    private Player player;
    public static bool IsGamePaused = false;
    [SerializeField] string sceneToLoad;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject[] skillList;
    private void Start()
    {
        pauseManager = this;
        GM = GameManager.instance;
    }
    public void PauseButtonPressed()
    {
        if (IsGamePaused)
            Resume();
        else
            Pause();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsGamePaused = false;
    }
    private void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale=0f;
        IsGamePaused = true;
    }
    public void LoadMenu()
    {
        IsGamePaused = false;
        Time.timeScale = 1f;
        GM.RestoreCheckpointStart();
        SceneManager.LoadScene(sceneToLoad);
    }
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SkillListChange()
    {
        for (int i = 0; i < skillList.Length; i++)
        {
            if (i == GameEngine.gameEngine.globalMovelistIndex)
            {
                skillList[i].SetActive(true);
            }
            else
            {
                skillList[i].SetActive(false);
            }
        }
        switch (GameEngine.gameEngine.globalMovelistIndex)
        {
            case 0://brave
                
                break;
            case 1://bomb
                
                break;
            case 2://pursuer
                
                break;
            case 3://windgod
                
                break;
            case 4://majin

                break;
            default:
                break;
        }
    }
}
