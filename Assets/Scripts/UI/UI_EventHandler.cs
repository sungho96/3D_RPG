using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Unity EventSystem 입력 이벤트 수신용 인터페이스
// - IDragHandler: 드래그 중 OnDrag(eventData) 호출됨
// - IPointerClickHandler: 클릭 시 OnPointerClick(eventData) 호출됨
// EventSystem이 PointerEventData(클릭/드래그 정보)를 만들어 콜백으로 전달한다.
public class UI_EventHandler : MonoBehaviour,IDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    //Unity 인터페이스 이벤트를 C# Action으로 외부(UI_Base)에게 넘겨주기 위한 델리게이트
    public Action<PointerEventData> OnClickHandler = null;
    public Action<PointerEventData> OnDragHandler = null;

    public Action<PointerEventData> OnEnterHandler = null;
    public Action<PointerEventData> OnExitHandler = null;
    //UI Event 발생시 캐치해서 콜백해주는 역할

    /// <summary>
    /// Unity EventSystem이 호출 → 여기서 Action 호출로 “중계(브릿지)”하는 구조
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        //구독자가 있을 때만 안전 호출
        if (OnClickHandler != null)
            OnClickHandler.Invoke(eventData);
    }


    public void OnDrag(PointerEventData eventData)
    {
        if(OnDragHandler != null)
            OnDragHandler.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(OnEnterHandler !=null)
            OnEnterHandler.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData) // 추가
    {
       if(OnExitHandler != null)
            OnExitHandler.Invoke(eventData);
    }
}
