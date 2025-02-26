﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseManager : MonoBehaviour
{
    GameManager GM;
    public static PauseManager pauseManager;
    public static bool IsGamePaused = false;
    [SerializeField] string sceneToLoad;
    [SerializeField] private GameObject pauseMenuUI, resultsMenuUI;
    [SerializeField] private string[] missionResults;
    public TextMeshProUGUI[] pointsText;

    [SerializeField] private GameObject[] skillList;
    [SerializeField] private Sprite[] spriteList;
    [SerializeField] private Image eyeCon;
    public TextMeshProUGUI menuTimer, battleUI;
    private AudioManager audioManager;
    [SerializeField] private StandaloneInputModule inputModule;
    private string uiSelectSound = "UI/Select", uiCancelSound = "UI/Cancel", uiCursorSound = "UI/Move Cursor";
    private void Start()
    {
        pauseManager = this;
        GM = GameManager.instance;
        audioManager = AudioManager.instance;
        SetMenuInput();
    }
    private void SetMenuInput()
    {
        inputModule.horizontalAxis = GameEngine.coreData.rawInputs[13].name;
        inputModule.verticalAxis = GameEngine.coreData.rawInputs[14].name;
        inputModule.submitButton = GameEngine.coreData.rawInputs[0].name;
        inputModule.cancelButton = GameEngine.coreData.rawInputs[2].name;
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
        PlaySelectSound(0);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsGamePaused = false;
    }
    private void Pause()
    {
        PlaySelectSound(0);
        pauseMenuUI.SetActive(true);
        Time.timeScale=0f;
        IsGamePaused = true;
    }
    public void Results()
    {
        resultsMenuUI.SetActive(true);
        resultsMenuUI.GetComponentInChildren<Button>().Select();
        DisplayResults();
        Time.timeScale=0f;
        IsGamePaused = true;
    }
    private void DisplayResults()
    {
        missionResults = Mission.instance.scoreText;
        //pointsText[0].text = ($"{Mission.instance.menuTimer.text}\r\n {Mission.instance.missionScore * 5}%\r\n {Mission.instance.enemiesKilled}\r\n {Mission.instance.damageCount}\r\n {Mission.instance.retryCount}");//stats
        pointsText[1].text = ($"{missionResults[0]}\r\n {missionResults[1]}\r\n\r\n {missionResults[3]}p");
        pointsText[2].text = ($"{missionResults[5]}");
    }
    public void LoadMenu()
    {
        PlaySelectSound(0);

        IsGamePaused = false;
        Time.timeScale = 1f;
        Destroy(Mission.instance.gameObject);
        MusicManager.instance.StopMusic();
        GM.RestoreCheckpointStart();
        if (EnemySpawner.spawnerInstance != null)
            Destroy(EnemySpawner.spawnerInstance.gameObject);
        if (GameManager.instance != null)
            Destroy(GameManager.instance.gameObject);
        SceneTransitionController.instance.LoadScene(sceneToLoad);
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
                skillList[i].GetComponentInChildren<Toggle>().Select();
            }
            else
            {
                skillList[i].SetActive(false);
            }
        }
        eyeCon.sprite = spriteList[GameEngine.gameEngine.globalMovelistIndex];
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
            case 4://zoe

                break;
            case 5://majin

                break;
            default:
                break;
        }
    }
    public void PlaySelectSound(int sound)
    {
        switch (sound)
        {
            case 0:
                audioManager.PlaySound(uiSelectSound);
                break;
            case 1:
                audioManager.PlaySound(uiCancelSound);
                break;
            case 2:
                audioManager.PlaySound(uiCursorSound);
                break;
        }
    }
}
