using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Button : UI_Popup
{
    // UI 자동 바인딩용 맵: enum 이름이 Hierarchy 오브젝트 이름과 1:1로 매칭되어야 함
    enum Buttons//button 종류
    {
        pointButton,
    }

    enum Texts//Text 종류
    {
        pointText,
    }

    enum GameObjects
    {
        TestObject,
    }

    enum Images
    { 
        ItemIcon,
    }

    int score;

    private void Start()
    {
        // 생성 직후 1회 초기화(바인딩/이벤트 연결)
        Init();
    }

    public override void Init()
    {
        // 팝업 공통 초기화(캔버스 정렬/오더 세팅)
        base.Init();
        // UI 바인딩은 시작 시 1회 수행(이후에는 Get으로 꺼내 쓰는 구조)
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));

        //확장 메서드 호출: go.BindEvent 형태로 깔끔하게 연결
        GetButton((int)Buttons.pointButton).gameObject.BindEvent(OnButtonClicked);

        GameObject go = GetImage((int)Images.ItemIcon).gameObject;
        BindEvent(go, (PointerEventData data) => { go.transform.position = data.position; }, Define.UIEvent.Drag);

    }
    // PointerEventData를 받는 시그니처(Action<PointerEventData>)와 맞추기 위해
    // 클릭 이벤트 함수도 동일한 형태로 작성
    public void OnButtonClicked(PointerEventData data)
    {
        score++;
    }
}
