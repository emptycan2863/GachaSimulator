using UnityEngine;

public class LoginManager : MonoBehaviour
{
    protected LoginManager _instance;

    [SerializeField] private string port = "";
    [SerializeField] private string server1URL = "";
    [SerializeField] private string server2URL = "";

    [SerializeField] private GameObject server1URL_textBox = null;
    [SerializeField] private GameObject server2URL_textBox = null;
    [SerializeField] private GameObject server1status = null;
    [SerializeField] private GameObject server2status = null;

    private void Awake() {
        _instance = this;
    }

    void Start() {

    }
}
