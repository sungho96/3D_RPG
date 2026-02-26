using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{

    UI_Inven _inven;
    UI_HUD _hud;

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Game;
        //Inven持失
        _inven = Managers.UI.ShowSceneUI<UI_Inven>();
        _inven.gameObject.SetActive(false);

        Dictionary<int, Data.Stat> dict = Managers.Data.StatDict;

        //HUD持失
        _hud = Managers.UI.ShowSceneUI<UI_HUD>();

        gameObject.GetOrAddComponent<CursorController>();

        GameObject player = Managers.Game.Spawn(Define.WorldObject.Player, "UnityChan");
        Camera.main.gameObject.GetOrAddComponent<CameraController>().SetPlayer(player);

        Managers.Game.Spawn(Define.WorldObject.Monster, "Knight");
    }



    public override void Clear()
    {

    }
}
