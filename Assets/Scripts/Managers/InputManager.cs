using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager
{
    public Action KeyAction = null;
    public Action<Define.MouseEvent> MouseAction = null;
    
    bool _pressed = false;

    public void OnUpdate()
    {
        if (EventSystem.current.IsPointerOverGameObject())//UI버튼을 누르면 다른 행동안하도록 리턴 
            return;

        if (Input.anyKey && KeyAction != null)
            KeyAction.Invoke();
        //press & click을 받을 수있는 인터페이스
        if (MouseAction != null)
        {
            if (Input.GetMouseButton(0))//왼쪽버튼누르고있으면 계속 발동
            { 
                MouseAction.Invoke(Define.MouseEvent.Press); 
                _pressed = true;
            }
            else
            {
                if (_pressed) //마우스를 땠을때는 클릭으로 인정
                    MouseAction.Invoke(Define.MouseEvent.Click);
                _pressed = false;
            }
        }   
    }
}
