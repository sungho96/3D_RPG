using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    // 팝업 정렬 순서(sorting order) 관리용으로 자주 쓰이는 값
    int _order = 10;
    //팝업 UI를 LIFO(마지막에 연 팝업부터 닫기) 규칙으로 관리하기 위한 스택
    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    // 현재 씬의 대표 UI(HUD 등) 참조를 저장(보통 1개만 유지)
    UI_Scene _sceneUI = null;

    // 모든 UI를 모아두는 최상위 부모(없으면 자동 생성)
    public GameObject Root
    {
        get
        {
            // 이미 존재하면 재사용, 없으면 생성해서 항상 UI 부모를 보장
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
                root = new GameObject { name = "@UI_Root" };

            return root;
        }
    }

    /// <summary>
    /// Canvas 공통 세팅 + 팝업은 sortingOrder를 증가시켜 항상 최상단에 배치
    /// </summary>
    /// <param name="go"></param>
    /// <param name="sort"></param>
    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        if(sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = 0;
        }
    }

    /// <summary>
    /// SubItem(UI 조각/슬롯)을 동적으로 생성하는 공용 팩토리.
    /// 예: 인벤 슬롯, 리스트 아이템, 스크롤 요소 등 반복 생성되는 UI 단위.
    /// </summary>
    /// <param name="parent">생성 후 붙일 부모(레이아웃 그룹이 붙어있는 컨테이너)</param>
    /// <param name="name">프리팹 이름(없으면 클래스명 사용)</param>
    public T MakeSubitem<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        // name이 없으면 "클래스명 == 프리팹명" 규칙으로 자동 로딩
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        // 전제: Prefabs/UI/SubItem/{name}.prefab 형태로 존재해야 함(ResourceManager 규칙)
        GameObject go = Managers.Resource.Instantiate($"UI/SubItem/{name}");

        // SubItem은 부모가 핵심(부모가 Grid/Content면 레이아웃이 자동 적용됨)
        if (parent != null)
            go.transform.SetParent(parent);

        return Util.GetOrAddComponent<T>(go);
    }

    /// <summary>
    /// Scene UI 생성: HUD처럼 1개만 깔리는 UI
    /// 전제: Prefabs/UI/Scene/{name}.prefab 존재
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
       
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        //리소스 로딩을 Managers.Resource로 통일
        GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");

        // 프리팹에 컴포넌트가 없어도 T를 보장(세팅 누락 방지)
        T SceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = SceneUI;

        // 로드한 UI는 @UI_Root 아래로 넣어서 Hierarchy를 정리
        go.transform.SetParent(Root.transform);

        return SceneUI;
    }

    /// <summary>
    /// Popup UI 생성: 여러 개 겹칠 수 있어 Stack으로 관리
    /// 전제: Prefabs/UI/Popup/{name}.prefab 존재
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T ShowPopUI<T>(string name = null) where T  :UI_Popup
    {
        // name이 없으면 "클래스명 == 프리팹명" 규칙으로 자동 로딩
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        //리소스 로딩을 Managers.Resource로 통일
        GameObject go =Managers.Resource.Instantiate($"UI/Popup/{name}");

        // 프리팹에 컴포넌트가 없어도 T를 보장(세팅 누락 방지)
        T popup = Util.GetOrAddComponent<T>(go);

        // 열린 순서대로 닫기 위해 스택에 저장
        _popupStack.Push(popup);

        // 로드한 UI는 @UI_Root 아래로 넣어서 Hierarchy를 정리
        go.transform.SetParent(Root.transform);
        
        return popup;
    }

    /// <summary>
    /// 특정 팝업을 닫으려는 요청 처리
    /// 규칙: 최상단 팝업만 닫을 수 있음(아니면 스택 순서가 깨져 UI 상태가 꼬임)
    /// </summary>
    /// <param name="popup"></param>
    public void ClosePopUI(UI_Popup popup)
    {
        // 현재 최상단 팝업이 아닌데 닫으려 하면 스택이 꼬일 수 있어 경고(안전장치)
        if (_popupStack.Count == 0)
            return;

        // 최상단 팝업만 닫게 강제(스택 꼬임 방지)
        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed");
            return;
        }

        ClosePopUI();
    }

    /// <summary>
    /// popup이 한개이상일경우 popup닫기
    /// </summary>
    public void ClosePopUI()
    {
        if (_popupStack.Count == 0)
            return;

        UI_Popup popup = _popupStack.Pop();
        Managers.Resource.Destroy(popup.gameObject);
        popup = null;

        _order--;
    }

    /// <summary>
    /// popup한번에 정리하기(제거)
    /// </summary>
    public void ClosedAllPopUI()
    {
        while (_popupStack.Count > 0)
            ClosePopUI();
    }
}
