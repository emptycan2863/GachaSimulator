using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Overlays;

[Serializable]
public class _UserData {
    public string id = "";
}
[Serializable]
public class _SaveData {
    public string saveKey = "";
    public Dictionary<string, _UserData> loginData = new Dictionary<string, _UserData>();
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


        if (saveData == null || saveData.saveKey != saveKey) saveData = new _SaveData();

        return saveData;
    }

    public static void Save(_SaveData data) {
        data.saveKey = saveKey;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public static _UserData GetUserDataByURL(string url) {
        if (saveData == null) return null;

        if (saveData.loginData.ContainsKey(url)) {
            return saveData.loginData[url];
        } else {
            _UserData data = new _UserData();
            saveData.loginData.Add(url, data);
            return data;
        }
    }
}
