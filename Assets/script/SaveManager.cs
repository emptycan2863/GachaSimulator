using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class _LoginData {
    public string url = "";
    public string id = "";
}

[Serializable]
public class _SaveData {
    public string saveKey = "";
    public List<_LoginData> loginData = new List<_LoginData>();
}

public static class SaveManager {
    static string saveKey = "test001";
    static _SaveData saveData;

    private static string savePath {
        get {
            return Path.Combine(Application.persistentDataPath, "save.json");
        }
    }

    public static _SaveData Load() {
        saveData = null;
        if (File.Exists(savePath)) {
            string json = File.ReadAllText(savePath);
            saveData = JsonUtility.FromJson<_SaveData>(json);
        }

        if (saveData == null || saveData.saveKey != saveKey) { saveData = new _SaveData(); }

        return saveData;
    }

    public static void Save() {
        saveData.saveKey = saveKey;
        string json = JsonUtility.ToJson(saveData, true);
        Debug.Log(json);
        File.WriteAllText(savePath, json);
    }

    public static string GetUserIDByURL(string url) {
        if (saveData == null) return null;

        for (int i = 0, len = saveData.loginData.Count; i < len; ++i) {
            if (saveData.loginData[i].url == url) return saveData.loginData[i].id;
        }
        return "";
    }

    public static void SetUserIDByURL(string url, string id) {
        if (saveData == null) return;

        for (int i = 0, len = saveData.loginData.Count; i < len; ++i) {
            if (saveData.loginData[i].url == url) {
                saveData.loginData[i].id = id;
                return;
            }
        }
        saveData.loginData.Add(new _LoginData{ 
            url = url,
            id = id
        });
    }
}
