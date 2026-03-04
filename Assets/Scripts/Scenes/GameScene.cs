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

        GameObject go = new GameObject { name = "SpawningPool" };
        SpawningPool pool = go.GetOrAddComponent<SpawningPool>();

        pool.SetSpawnCenter(new Vector3(-36f, -1.6f, -26.11f), 15f);
        pool.SetKeepMonsterCount(3);
        
    }



    public override void Clear()
    {

    }
}
