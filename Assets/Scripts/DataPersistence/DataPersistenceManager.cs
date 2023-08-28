using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool disableDP = false;
    [SerializeField] private bool initDataIfNull = false;
    [SerializeField] private bool overrideSelectedProfileId = false;
    [SerializeField] private string testSelectedProfileId = "test";

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    private string selectedProfileID = "";
    public static DataPersistenceManager instance { get; private set; }
    private void Awake()
    {
        if (instance!=null)
        {
            Debug.Log("Found more than one DPManager in the scene");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (disableDP)
        {
            Debug.LogWarning("Data Persistence is disabled");
        }
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        InitializeSelectedProfileId();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded+=OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded-=OnSceneLoaded;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }
    public void ChangeSelectedProfileId(string newProfileId)
    {
        this.selectedProfileID = newProfileId;//update profile
        LoadGame();
    }
    private void Start()
    {
        
    }
    public void DeleteProfileData(string profileId)
    {
        dataHandler.Delete(profileId);
        InitializeSelectedProfileId();
        LoadGame();
    }
    private void InitializeSelectedProfileId()
    {
        this.selectedProfileID = dataHandler.GetMostRecentProfileId();
        if (overrideSelectedProfileId)
        {
            this.selectedProfileID = testSelectedProfileId;
            Debug.LogWarning("Selected ProfileID overwritten");
        }
    }
    public void NewGame()
    {
        this.gameData = new GameData();
    }
    public void LoadGame()
    {
        if (disableDP)
        {
            return;
        }
            this.gameData = dataHandler.Load(selectedProfileID);


        if (this.gameData==null&&initDataIfNull)//if no data do not continue
        { NewGame(); }
        if (this.gameData==null)//if no data do not continue
        {
            Debug.Log("No data found. Create New Data");
            //NewGame();
            return;
        }
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
        Debug.Log("Loaded skillList = " + gameData.unlockedSkillsData);
    }
    public void SaveGame()
    {
        if (disableDP)
        {
            return;
        }
        if (this.gameData==null)
        {
            Debug.LogWarning("No data. Create a new game.");
            return;
        }
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(gameData);
        }
        gameData.lastUpdated = System.DateTime.Now.ToBinary();
        Debug.Log("Saved skillList = " + gameData.unlockedSkillsData);
        dataHandler.Save(gameData, selectedProfileID);
    }
    private void OnApplicationQuit()
    {
        SaveGame();
    }
    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
    public bool HasGameData()
    {
        return gameData != null;
    }
    public Dictionary<string,GameData> GetAllProfilesGameData()
    {
        return dataHandler.LoadAllProfiles();
    }
}
