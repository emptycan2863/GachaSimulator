using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour {
    private static LobbyManager _instance;

    void Awake() {
        if (!GameManager.isLoad) {
            SceneManager.LoadScene("login");
            return;
        }

        _instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.inst.StartJoin();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
