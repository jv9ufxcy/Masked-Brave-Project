using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler 
{
    public string dataDirPath = "";
    public string dataFileName = "";
    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }
    public GameData Load(string profileID)
    {
        if (profileID==null)
        {
            return null;
        }
        string fullPath = Path.Combine(dataDirPath, profileID, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    { 
                        dataToLoad= reader.ReadToEnd();
                    }
                }
                //from Json to GameData
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when loading file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }
    public void Save(GameData data, string profileID)
    {
        if (profileID == null)
        {
            return;
        }
        string fullPath = Path.Combine(dataDirPath, profileID, dataFileName);
        try
        {
            //create directory fill if doesnt exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            //serialize C# game data to Json
            string dataToStore = JsonUtility.ToJson(data, true);
            //write data to file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when saving to file: " + fullPath + "\n" + e);
        }
    }
    public void Delete(string profileId)
    {
        if (profileId==null)
        {
            return;
        }
        string fullPath = Path.Combine(dataDirPath,profileId, dataFileName);
        try
        {
            if (File.Exists(fullPath))
            {
                Directory.Delete(Path.GetDirectoryName(fullPath), true);
            }
            else
            {
                Debug.Log("No data found to delete");
            }
        }
        catch (Exception e)
        {

            Debug.LogError("Failed to delete profile data: "+profileId+" at path: "+fullPath+"\n" + e);
        }
    }
    public Dictionary<string, GameData> LoadAllProfiles()
    {
        Dictionary<string, GameData> profileDictionary = new Dictionary<string, GameData>();
        //loops all director names in path
        IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(dataDirPath).EnumerateDirectories();
        foreach (DirectoryInfo dirInfo in dirInfos)
        {
            string profileID = dirInfo.Name;
            string fullPath = Path.Combine(dataDirPath, profileID,dataFileName);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning("Skipping crrent directory at ProfileID: " + profileID);
                continue;
            }
            //load game data
            GameData profileData = Load(profileID);
            if (profileData!=null)
            {
                profileDictionary.Add(profileID, profileData);
            }
            else
            {
                Debug.LogError("Something went wrong at ProfileID: "+profileID);
            }
        }
        return profileDictionary;
    }
    public string GetMostRecentProfileId()
    {
        string mostRecentProfileId = null;
        Dictionary<string, GameData> profileGameData = LoadAllProfiles();
        foreach (KeyValuePair<string,GameData> pair in profileGameData)
        {
            string profileId = pair.Key;
            GameData gameData = pair.Value;
            if (gameData==null)
            {
                continue;
            }
            if (mostRecentProfileId==null)
            {
                mostRecentProfileId = profileId;
            }
            else
            {
                DateTime mostRecentDateTime = DateTime.FromBinary(profileGameData[mostRecentProfileId].lastUpdated);
                DateTime newDateTime = DateTime.FromBinary(gameData.lastUpdated);
                if (newDateTime>mostRecentDateTime)
                {
                    mostRecentProfileId=profileId;
                }
            }
        }
        return mostRecentProfileId;
    }
}
