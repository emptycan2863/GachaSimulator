using System;

[Serializable]
public class IdLoginRequest {
    public string id = "";
}

[Serializable]
public class LoginResponse {
    public bool success;
    public string id;
    public UserInfo user;
}

[Serializable]
public class UserInfo {
    public string id = "";
}