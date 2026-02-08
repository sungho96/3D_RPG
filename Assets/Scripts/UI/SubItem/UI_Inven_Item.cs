using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Inven_Item : UI_Base
{
    // UI_Base Bind용(= Hierarchy 이름과 동일)
    enum GameObjects
    {
        Selected, // 선택 표시(하이라이트/테두리 등)
        ItemIcon, // 아이콘(필요하면 나중에 Sprite 교체)
        ItemName, // 이름 텍스트 오브젝트
    }

    // 아이템 표시 데이터
    string _name;
    string _desc;

    // 부모(UI_Inven) 참조: 툴팁 호출용
    UI_Inven _owner;

    // 외부에서 읽기만(툴팁에서 사용)
    public string ItemName => _name;
    public string ItemDesc => _desc;

    bool _isSelected = false;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        // 1) 바인딩(자식 오브젝트 캐싱)
        Bind<GameObject>(typeof(GameObjects));

        // 2) 부모 UI(인벤토리) 찾기: Enter/Exit에서 툴팁 띄우는 용도
        _owner = GetComponentInParent<UI_Inven>();

        // 3) 초기 텍스트 반영(생성 직후 이름 표시)
        Get<GameObject>((int)GameObjects.ItemName)
            .GetComponent<TMPro.TMP_Text>().text = _name;

        // 4) 이벤트 연결(클릭/호버)
        gameObject.BindEvent(OnClickItem, Define.UIEvent.Click);
        gameObject.BindEvent(OnEnterItem, Define.UIEvent.Enter);
        gameObject.BindEvent(OnExitItem, Define.UIEvent.Exit);

        // 5) 선택 표시 초기화
        GetObject((int)GameObjects.Selected).SetActive(false);
        RefreshSelected();
    }

    // 마우스가 슬롯 위로 올라오면 툴팁 표시
    void OnEnterItem(PointerEventData eventData)
    {
        if (_owner == null) return;
        _owner.ShowToolTip(this, eventData);
    }

    // 마우스가 슬롯에서 벗어나면 툴팁 숨김
    void OnExitItem(PointerEventData eventData)
    {
        if (_owner == null) return;
        _owner.HideTooltip(this);
    }

    // 클릭 시 선택 토글
    void OnClickItem(PointerEventData eventData)
    {
        _isSelected = !_isSelected;
        RefreshSelected();

        Debug.Log($"아이템 클릭 : {_name}. Selected={_isSelected}");
    }

    // 선택 표시(Selected 오브젝트 On/Off)
    void RefreshSelected()
    {
        GetObject((int)GameObjects.Selected).SetActive(_isSelected);
    }

    // 외부에서 슬롯 정보 세팅(UI_Inven에서 생성 후 호출)
    public void SetInfo(string name, string desc)
    {
        _name = name;
        _desc = desc;

        // 생성 순서에 따라 SetInfo가 Init보다 먼저 호출될 수 있음
        // - 이미 Bind가 끝났다면 즉시 텍스트도 갱신
        if (_objects.Count > 0)
        {
            GetObject((int)GameObjects.ItemName)
                .GetComponent<TMPro.TMP_Text>().text = _name;
        }
    }
}
