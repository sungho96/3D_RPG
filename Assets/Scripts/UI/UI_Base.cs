using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UI_Base : MonoBehaviour
{
    // UI 바인딩 캐시: 타입별(Button/TMP_Text/Image/GameObject 등)로 enum 순서대로 저장
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    public abstract void Init();
    /// <summary>
    /// 전제: enum 항목명 == Hierarchy 오브젝트 이름(대소문자 포함)
    /// UI자동화하기위해서 BInding 하기(UI맵핑자동화)
    /// enum 이름을 기준으로 Hierarchy에서 동일한 이름의 UI를 찾아 타입(T)별로 캐싱.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    protected void Bind<T>(Type type) where T : UnityEngine.Object// 전제: enum 이름 == Hierarchy 오브젝트 이름 (대소문자 포함 정확히 일치해야 함)
    {
        //string으로 이름 가져오기
        string[] names = Enum.GetNames(type);

        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects.Add(typeof(T), objects);//dict _object에 타입추가

        //순차적으로 찾기
        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
            {
                // GameObject는 컴포넌트가 아니라 오브젝트 자체를 반환해야 해서 전용 FindChild(GameObject) 사용
                objects[i] = Util.FindChild(gameObject, names[i], true);
            }
            else
            {
                objects[i] = Util.FindChild<T>(gameObject, names[i], true); //go 기준으로 자식(또는 하위 전체)에서 이름(name)이 일치하는 T를 찾아 반환하는 함수
            }
        }
    }

    /// <summary>
    /// Bind로 저장해 둔 캐시에서 idx에 해당하는 UI 참조
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="idx"></param>
    /// <returns></returns>
    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }

    // 자주 쓰는 타입을 빠르게 꺼내기 위한 래퍼
    
    protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
    protected TMP_Text GetText(int idx) { return Get<TMP_Text>(idx); }
    protected Button GetButton(int idx) { return Get<Button>(idx); }
    protected Image GetImage(int idx) { return Get<Image>(idx); }

    /// <summary>
    /// UI 오브젝트(go)에 클릭/드래그 이벤트를 코드로 연결해주는 공용 함수.
    /// </summary>
    /// <param name="go"></param>
    /// <param name="action"></param>
    /// <param name="type"></param>
    public static void BindEvent(GameObject go, Action<PointerEventData> action,Define.UIEvent type = Define.UIEvent.Click)
    {
        // 이벤트를 받는 중계 컴포넌트(UI_EventHandler)를 보장(없으면 자동 추가)
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

       switch (type)
        {
            //중복방지
            case Define.UIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
            case Define.UIEvent.Drag:
                evt.OnDragHandler -= action;
                evt.OnDragHandler += action;
                break;
            case Define.UIEvent.Enter: 
                evt.OnEnterHandler -= action;
                evt.OnEnterHandler += action;
                break;
            case Define.UIEvent.Exit: 
                evt.OnExitHandler -= action;
                evt.OnExitHandler += action;
                break;
        }
    }
}
