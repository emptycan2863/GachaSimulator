using UnityEngine;

public class LoginButton : MonoBehaviour {
    [SerializeField] private int index = 0;

    public void Click() {
        if (GameManager.blockButton) return;
        LoginManager.ServerLogin(index);
    }
}
