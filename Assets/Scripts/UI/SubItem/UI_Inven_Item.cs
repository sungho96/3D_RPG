using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Inven_Item : UI_Base
{
    enum GameObjects
    {
        Selected,
        ItemIcon,
        ItemName,
    }
    string _name;
    string _desc;

    UI_Inven _owner;

    public string ItemName => _name;
    public string ItemDesc => _desc;


    bool _isSelected = false;
    void Start()
    {
        Init();
    }

    public override void Init()
    {
        // 바인딩(1회)
        Bind<GameObject>(typeof(GameObjects));
        // 부모 찾기(현재 구조에서 가장 간단하고 안전)
        _owner = GetComponentInParent<UI_Inven>();
        //TMP 이름표기
        Get<GameObject>((int)GameObjects.ItemName)
            .GetComponent<TMPro.TMP_Text>().text = _name;

        //클릭이벤트
        gameObject.BindEvent(OnClickItem, Define.UIEvent.Click);
        //Hover 이벤트
        gameObject.BindEvent(OnEnterItem, Define.UIEvent.Enter);
        gameObject.BindEvent(OnExitItem, Define.UIEvent.Exit);

        GetObject((int)GameObjects.Selected).SetActive(false);

        //초기 선택 반영
        RefreshSelected(); 
    }

    void OnEnterItem(PointerEventData eventData)
    {
        if (_owner == null) return;
        _owner.ShowToolTip(this, eventData);
    }

    void OnExitItem(PointerEventData eventData)
    {
        if (_owner == null) return;
            _owner.HideTooltip(this);
    }

    void OnClickItem(PointerEventData eventData)
    {
        _isSelected = !_isSelected;
        RefreshSelected();

        Debug.Log($"아이템 클릭 : {_name}.Sselected={_isSelected}");
    }

    void RefreshSelected()
    {
        GetObject((int)GameObjects.Selected).SetActive(_isSelected);
    }

    public void SetInfo(string name, string desc)
    {
        _name = name;
        _desc = desc;

        // SetInfo가 Init보다 먼저 불릴 수도 있어서 방어
        // (Bind가 끝난 뒤라면 즉시 반영)
        if (_objects.Count > 0)
        {
            GetObject((int)GameObjects.ItemName).GetComponent<TMPro.TMP_Text>() .text = _name;
        }
    }


}
