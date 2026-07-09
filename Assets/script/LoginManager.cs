using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour {
    private static LoginManager _instance;
    private float serverCheckTime = 0;

    [SerializeField] private string port = "8000";
    [SerializeField] private string[] serverURL;

    [SerializeField] private GameObject[] serverURL_textBox;
    [SerializeField] private GameObject[] serverStatus;

    private List<TMP_InputField> tmpIfList = new List<TMP_InputField>();
    private List<bool> isServerLoading = new List<bool>();
    private List<bool> isServerActive = new List<bool>();

    private void Awake() {
        _instance = this;
    }

    void Start() {
        for (int i = 0, len = serverURL_textBox.Length; i < len; ++i) {
            int _i = i;
            TMP_InputField tmpIf = serverURL_textBox[i].GetComponent<TMP_InputField>();
            tmpIfList.Add(tmpIf);
            isServerLoading.Add(false);
            isServerActive.Add(false);
            tmpIf.text = serverURL[i] + ":" + port;
            tmpIf.onEndEdit.AddListener((t) => {
                StartCoroutine(ServerActiveCheck(_i));
            });
        }
    }

    void Update() {
        if (serverCheckTime < Time.time) {
            serverCheckTime = Time.time + 5;

            for (int i = 0, len = serverURL_textBox.Length; i < len; ++i) {
                StartCoroutine(ServerActiveCheck(i));
            }
        }
    }

    private IEnumerator ServerActiveCheck(int index) {
        if (serverURL_textBox.Length <= index) yield break;
        if (isServerLoading[index]) yield break;
        isServerActive[index] = false;
        isServerLoading[index] = true;
        string serverUrl = tmpIfList[index].text;
        string url = "http://" + serverUrl + "/serverActive";
        serverStatus[index].GetComponent<Image>().color = new Color(1, 1, 0, 1);

        UnityWebRequest request = null;
        try {
            request = UnityWebRequest.Get(url);
            request.timeout = 2;
        } catch (Exception e) {
            Debug.LogWarning("서버 호출 실패. URL: " + url + ", error: " + e.Message);
            serverStatus[index].GetComponent<Image>().color = new Color(1, 0, 0, 1);
            isServerLoading[index] = false;
            yield break;
        }

        try {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                serverStatus[index].GetComponent<Image>().color = new Color(0, 1, 0, 1);
                isServerActive[index] = true;
            } else {
                Debug.LogWarning("서버 호출 실패. URL: " + url + ", error: " + request.error);
                serverStatus[index].GetComponent<Image>().color = new Color(1, 0, 0, 1);
            }
        } finally {
            isServerLoading[index] = false;
            request?.Dispose();
        }
    }

    public static void ServerLogin(int index) {
        if (_instance == null) return;
        _instance._ServerLogin(index);
    }

    private void _ServerLogin(int index) {
        StartCoroutine(ServerLoginRun(index));
    }

    private IEnumerator ServerLoginRun(int index) {
        GameManager.blockButton = true;
        tmpIfList[index].interactable = false;

        try {
            yield return new WaitUntil(() => !isServerLoading[index]);

            yield return StartCoroutine(ServerActiveCheck(index));

            if (!isServerActive[index]) {
                Debug.Log("로그인 실패");
                yield break;
            }

            string serverUrl = tmpIfList[index].text;
            _UserData user = SaveManager.GetUserDataByURL(serverUrl);

            if (user.id == "") {
            } else { 
            }
        } finally {
            GameManager.blockButton = false;
            tmpIfList[index].interactable = true;
        }
    }
}