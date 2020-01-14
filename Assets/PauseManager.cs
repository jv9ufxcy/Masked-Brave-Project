using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    GameManager GM;
    private Player player;
    public static bool IsGamePaused = false;
    [SerializeField] string sceneToLoad;
    [SerializeField] private GameObject pauseMenuUI;
    private void Start()
    {
        GM = GameManager.instance;
        player = Player.Instance;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown(player.startButtonName))
        {
            if (IsGamePaused)
                Resume();
            else
                Pause();
        }
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
}
