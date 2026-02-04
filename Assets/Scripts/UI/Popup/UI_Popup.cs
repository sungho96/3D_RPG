using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Popup : UI_Base
{
    public override void Init()
    {
        // 팝업은 항상 sortingOrder를 증가시키며 최상단으로
        Managers.UI.SetCanvas(gameObject, true);
    }
    public virtual void ClosePopupUI()
    {
        // 팝업 닫기는 UIManager가 스택 규칙(LIFO)을 유지하며 처리
        Managers.UI.ClosePopUI(this);
    }
}
