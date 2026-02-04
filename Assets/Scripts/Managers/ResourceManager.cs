using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Resources 폴더 기반 로딩 매니저
// 전제: Resources/Prefabs/ 아래에 프리팹이 있어야 Load 가능
public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }

    /// <summary>
    ///  Prefabs/{path} 프리팹을 인스턴스화.
    /// (Clone)을 제거해 디버깅/이름 기반 탐색 시 혼동을 줄임.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject prefab = Load<GameObject>($"Prefabs/{path}");
        if (prefab == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        //프리팹 복사품에 Clone 지우기 
        GameObject go = Object.Instantiate(prefab, parent);
        int index = go.name.IndexOf("(Clone)");
        if (index >0) 
            go.name = go.name.Substring(0, index);

        return go;
    }

    /// <summary>
    /// 호출시 해당 프리팹 제거
    /// </summary>
    /// <param name="go"></param>
    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        Object.Destroy(go);
    }
}
