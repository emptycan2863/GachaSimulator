using System;
using System.Collections.Generic;

[Serializable]
public class UserInfo {
    public string id = "";

    public string name = "";
    public int gold = 0;
    public List<MailInfo> mail = new List<MailInfo>();
}

[Serializable]
public class MailInfo {
    public int type;
    public int reward;
}