using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

//기능성함수 모음
public class Util
{
    /// <summary>
    /// FindChild<Transform> 결과를 GameObject로 변환해서 반환하는 버전입니다.
    /// </summary>
    /// <param name="go"></param>
    /// <param name="name"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;
        else
        {
            return transform.gameObject;
        }
    }

    /// <summary>
    /// go 기준으로 자식(또는 하위 전체)에서 이름(name)이 일치하는 T를 찾아 반환.
    /// </summary>
    /// <param name="go">탐색 시작 오브젝트</param>
    /// <param name="name">찾을 이름(비우면 첫 번째 T 반환)</param>
    /// <param name="recursive">false: 직계 자식 / true: 하위 전체</param>
    /// <returns>찾은 T, 없으면 null</returns>
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;
        // recursive=false: 직계 자식 Transform을 대상으로 이름을 비교한 뒤, T를 반환하려는 분기
        if (recursive ==false)
        {
            for(int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(0);
                if (string.IsNullOrEmpty(name) || transform.name == name) // name이 비어있으면 이름 조건 무시, 아니면 이름이 같은지 비교
                {
                     T component = go.GetComponent<T>();
                     if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach ( T component  in go.GetComponentsInChildren<T>() )//결과값이 component로 받아져서 사용
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }
        return null;
    }
}
