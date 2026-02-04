using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Inven : UI_Scene
{
    // UI_Base 바인딩용: Hierarchy 이름과 동일해야 함
    enum GameObjects
    {
        GridPanel,   // 아이템 슬롯들이 들어갈 부모(Content)
        DetailPanel, // 툴팁(상세) 패널
    }

    enum Texts
    {
        DetailNameText, // 툴팁 이름
        DetailDescText, // 툴팁 설명
    }

    UI_Inven_Item _hoverItem = null;   // 현재 마우스가 올라간 아이템(툴팁 표시용)
    RectTransform _tooltipRt;          // 툴팁 패널 RectTransform 캐싱
    RectTransform _canvasRt;           // 캔버스 기준 좌표 변환용 캐싱
    Canvas _canvas;

    // 툴팁 위치 보정(마우스 커서 기준 오프셋)
    Vector2 _offset = new Vector2(20f, -20f);

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        // UI 바인딩(이름 기준 자동 매핑)
        Bind<GameObject>(typeof(GameObjects));
        Bind<TMPro.TMP_Text>(typeof(Texts));

        // 시작 시 툴팁은 숨김
        GetObject((int)GameObjects.DetailPanel).SetActive(false);

        GameObject gridPanel = Get<GameObject>((int)GameObjects.GridPanel);

        // 테스트/리프레시용: 기존 슬롯 제거
        foreach (Transform child in gridPanel.transform)
            Managers.Resource.Destroy(child.gameObject);

        // 테스트 데이터로 슬롯 생성
        for (int i = 0; i < 12; i++)
        {
            UI_Inven_Item item = Managers.UI.MakeSubitem<UI_Inven_Item>(gridPanel.transform);
            item.SetInfo($"단검{i + 1}", $"공격력 {i + 2}");
        }

        // 툴팁/캔버스 캐싱
        _tooltipRt = GetObject((int)GameObjects.DetailPanel).GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasRt = _canvas.GetComponent<RectTransform>();
    }

    /// <summary>
    /// 마우스가 아이템 위에 올라갔을 때(Enter) 호출
    /// </summary>
    /// <param name="item"></param>
    /// <param name="eventData"></param>
    public void ShowToolTip(UI_Inven_Item item, PointerEventData eventData)
    {
        _hoverItem = item;

        // 툴팁 텍스트 갱신
        GetText((int)Texts.DetailNameText).text = item.ItemName;
        GetText((int)Texts.DetailDescText).text = item.ItemDesc;

        // 툴팁 표시
        GetObject((int)GameObjects.DetailPanel).SetActive(true);
    }

    /// <summary>
    /// 마우스가 아이템에서 벗어났을 때(Exit) 호출
    /// </summary>
    /// <param name="item"></param>
    public void HideTooltip(UI_Inven_Item item)
    {
        if (_hoverItem == null) return;

        _hoverItem = null;
        GetObject((int)GameObjects.DetailPanel).SetActive(false);
    }
}
