using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.Audio.ProcessorInstance;

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
        for (int i = 0, len = tmpIfList.Count; i < len; ++i) {
            tmpIfList[i].interactable = false;
        }

        string url = "http://" + tmpIfList[index].text;
        string loginUrl = url + "/login";
        string idLoginUrl = loginUrl + "/idLogin";
        string signinUrl = loginUrl + "/signIn";

        try {
            yield return new WaitUntil(() => !isServerLoading[index]);

            yield return StartCoroutine(ServerActiveCheck(index));

            if (!isServerActive[index]) {
                Debug.Log("로그인 실패");
                yield break;
            }

            string serverUrl = tmpIfList[index].text;
            string userID = SaveManager.GetUserIDByURL(serverUrl);

            if (userID == "") {
                UnityWebRequest signinRequest = null;
                try {
                    signinRequest = new UnityWebRequest(signinUrl, "POST");
                    signinRequest.downloadHandler = new DownloadHandlerBuffer();
                    signinRequest.timeout = 5;
                } catch (Exception e) {
                    Debug.LogWarning("계정 생성 실패. URL: " + url + ", error: " + e.Message);
                    yield break;
                }

                yield return signinRequest.SendWebRequest();

                if (signinRequest.result != UnityWebRequest.Result.Success) {
                    Debug.LogWarning("계정 생성 실패. URL: " + idLoginUrl + ", error: " + signinRequest.error);
                    yield break;
                }

                string signinResponseJson = signinRequest.downloadHandler.text;

                LoginResponse signinResponse = JsonUtility.FromJson<LoginResponse>(signinResponseJson);

                if (signinResponse == null || !signinResponse.success || signinResponse.user == null) {
                    Debug.Log("계정 생성 실패");
                    yield break;
                }

                userID = signinResponse.id;
                Debug.Log("계정 생성 성공: " + signinResponse.user);

            }

            IdLoginRequest loginRequest = new IdLoginRequest {
                id = userID
            };

            string requestJson = JsonUtility.ToJson(loginRequest);

            UnityWebRequest idLoginRequest = null;

            try {
                idLoginRequest = UnityWebRequest.Post(idLoginUrl, requestJson, "application/json");
                idLoginRequest.timeout = 5;
            } catch (Exception e) {
                Debug.LogWarning("로그인 실패. URL: " + url + ", error: " + e.Message);
                yield break;
            }

            try {
                yield return idLoginRequest.SendWebRequest();

                if (idLoginRequest.result != UnityWebRequest.Result.Success) {
                    Debug.LogWarning("로그인 요청 실패. URL: " + idLoginUrl + ", error: " + idLoginRequest.error);
                    yield break;
                }

                string loginResponseJson = idLoginRequest.downloadHandler.text;

                LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(loginResponseJson);

                if (loginResponse == null || !loginResponse.success || loginResponse.user == null) {
                    Debug.Log("로그인 실패");
                    yield break;
                }

                Debug.Log("로그인 성공: " + loginResponse.user);

                SaveManager.SetUserIDByURL(serverUrl, userID);
            } finally {
                idLoginRequest?.Dispose();
                SaveManager.Save();
            }
        } finally {
            GameManager.blockButton = false;
            for (int i = 0, len = tmpIfList.Count; i < len; ++i) {
                tmpIfList[i].interactable = true;
            }
        }
    }
}