using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{

    UI_Inven _inven;
    UI_HUD _hud;

    void Start()
    {
        base.Init();

        SceneType = Define.Scene.Game;
        //Inven持失
        _inven = Managers.UI.ShowSceneUI<UI_Inven>();
        _inven.gameObject.SetActive(false);

        //HUD持失
        _hud = Managers.UI.ShowSceneUI<UI_HUD>();
    }



    public override void Clear()
    {

    }
}
