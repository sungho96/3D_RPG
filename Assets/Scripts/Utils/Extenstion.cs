using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Extension 
{
    // 확장 메서드: Util.GetOrAddComponent(go) → go.GetOrAddComponent<T>() 형태로 사용하게 하여 호출부를 간결하게 함
    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        return Util.GetOrAddComponent<T>(go);
    }

    // 호출부(UI_Button) 코드를 읽기 쉽게(체이닝 느낌) 만들려는 목적
    public static void BindEvent(this GameObject go, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_Base.BindEvent(go, action, type);
    }
}
