using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    public static GameManager inst { get { return _instance; } }
    public static bool isLoad { get { return _instance != null; } }
    public static bool blockButton = false;

    private static string onlineUrl = "";
    private static string onlineID = "";
    private static bool isJoin = false;

    private static float pingTime = 0f;
    private static bool isPingLoading = false;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        SaveManager.Load();
    }
    void Start() {

    }

    void Update() {


        if (!isJoin) return;
        if (isPingLoading) return;

        if (pingTime < Time.time) {
            pingTime = Time.time + 5f;
            StartCoroutine(Ping());
        }
    }

    public static void Login(string url, string id) {
        onlineUrl = url;
        onlineID = id;
        SaveManager.SetUserIDByURL(url, id);
        SceneManager.LoadScene("lobby");
    }


    public void StartJoin() {
        StartCoroutine(Join());
    }

    private IEnumerator Join() {
        string url = "http://" + onlineUrl + "/online/join";

        OnlineRequest joinRequest = new OnlineRequest {
            id = onlineID
        };
        string requestJson = JsonUtility.ToJson(joinRequest);
        UnityWebRequest request = null;
        try {
            request = UnityWebRequest.Post(url, requestJson, "application/json");
            request.timeout = 5;
        } catch (Exception e) {
            Debug.LogWarning("조인 실패. URL: " + url + ", error: " + e.Message);
            yield break;
        }

        try {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.LogWarning("조인 실패. URL: " + url + ", error: " + request.error);
                yield break;
            }

            string signinResponseJson = request.downloadHandler.text;

            OnlineResponse joinResponse = JsonUtility.FromJson<OnlineResponse>(signinResponseJson);

            if (joinResponse == null || !joinResponse.success) {
                Debug.Log("조인 실패");
                yield break;
            }

            isJoin = true;
            pingTime = Time.time + 5f;
        } finally {
            request?.Dispose();
        }
    }

    private IEnumerator Ping() {
        isPingLoading = true;

        string url = "http://" + onlineUrl + "/online/ping";

        OnlineRequest pingRequest = new OnlineRequest {
            id = onlineID
        };

        string requestJson = JsonUtility.ToJson(pingRequest);

        UnityWebRequest request = null;

        try {
            request = UnityWebRequest.Post(url, requestJson, "application/json");
            request.timeout = 5;
        } catch (Exception e) {
            Debug.LogWarning("핑 요청 생성 실패. URL: " + url + ", error: " + e.Message);
            DisconnectToLogin();
            isPingLoading = false;
            yield break;
        }

        try {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.LogWarning("핑 실패. URL: " + url + ", error: " + request.error);
                DisconnectToLogin();
                yield break;
            }

            string responseJson = request.downloadHandler.text;

            OnlineResponse pingResponse = JsonUtility.FromJson<OnlineResponse>(responseJson);

            if (pingResponse == null || !pingResponse.success) {
                Debug.LogWarning("핑 실패. 서버에서 success=false 반환");
                DisconnectToLogin();
                yield break;
            }

            Debug.LogWarning("5초 주기 핑");
        } finally {
            isPingLoading = false;
            request?.Dispose();
        }
    }
    private static void DisconnectToLogin() {
        isJoin = false;
        SceneManager.LoadScene("login");
    }
}
