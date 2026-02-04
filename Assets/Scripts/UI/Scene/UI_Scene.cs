using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Scene : UI_Base
{
    public override void Init()
    {
        // Scene UI는 기본 레이어(0)에 고정  
        Managers.UI.SetCanvas(gameObject, false);
    }
}
