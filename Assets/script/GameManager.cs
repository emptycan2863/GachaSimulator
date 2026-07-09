using UnityEngine;

public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    public static bool blockButton = false;

    private void Awake() {
        _instance = this;
        DontDestroyOnLoad(gameObject);
        SaveManager.Load();
    }
    void Start() {

    }

    void Update() {
        
    }
}
