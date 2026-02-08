using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginScene : BaseScene
{
    UI_Login _loginUI;
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;

    }
    public override void Clear()
    {
        Managers.UI.ClosedAllPopUI();
    }
}
