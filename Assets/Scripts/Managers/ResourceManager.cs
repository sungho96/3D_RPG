using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Resources 폴더 기반 로딩 매니저
// 전제: Resources/Prefabs/ 아래에 프리팹이 있어야 Load 가능
public class ResourceManager
{
    /// <summary>
    /// GameObject 로드일 경우 "풀에 등록된 원본"이 있으면 그걸 우선 사용
    /// 그 외는 Resources.Load로 기본 로드
    /// </summary>
    public T Load<T>(string path) where T : Object
    {
        // GameObject 로딩인 경우만 "풀 원본" 우선 조회 로직 적용
        if (typeof(T) == typeof(GameObject))
        {
            // path에서 마지막 이름만 뽑기
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0)
                name = name.Substring(index + 1);

            /*
             * 풀 원본 우선 반환
             * 이미 풀을 만들었다면 Resources.Load를 다시 할 필요가 없음
             * PoolManager가 가진 Original을 재사용 (일관성 + 최적화)
             */
            GameObject go = Managers.Pool.GetOriginal(name);
            if (go != null)
                return go as T;
        }

        // 기본 로딩(리소스 폴더에서 직접 로드)
        return Resources.Load<T>(path);
    }

    /// <summary>
    /// Prefabs/{path} 프리팹을 인스턴스화.
    /// (Clone)을 제거해 디버깅/이름 기반 탐색 시 혼동을 줄임.
    /// - Poolable이 붙어 있으면: "생성" 대신 Pool.Pop
    /// - Poolable이 없으면: 일반 Instantiate
    /// </summary>
    public GameObject Instantiate(string path, Transform parent = null)
    {
        // Resources/Prefabs/ 아래에서 로드
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if (original == null)
        {
            Debug.Log($"Failed to load prefab : {path}");
            return null;
        }

        // 풀링 대상이면 Pop으로 가져오기
        if (original.GetComponent<Poolable>() != null)
            return Managers.Pool.Pop(original, parent).gameObject;

        // 풀링 대상이 아니면 일반 생성
        GameObject go = Object.Instantiate(original, parent);

        //프리팹 복사품에 Clone 지우기 (하이어라키/검색 시 혼동 줄이기)
        int index = go.name.IndexOf("(Clone)");
        if (index > 0)
            go.name = go.name.Substring(0, index);

        return go;
    }

    /// <summary>
    /// 호출 시 해당 오브젝트 제거
    /// Poolable이면: Destroy가 아니라 Pool로 반환(push)
    /// Poolable이 아니면: 일반 Object.Destroy
    /// 프로젝트 전체에서 "삭제"를 이 함수로 통일하면
    /// 풀링 적용/비적용이 자동으로 분기되어 관리가 쉬워짐
    /// </summary>
    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        // 풀링 대상이면 반환 처리
        Poolable poolable = go.GetComponent<Poolable>();
        if (poolable != null)
        {
            Managers.Pool.push(poolable);
            return;
        }

        // 풀링 대상이 아니면 실제 파괴
        Object.Destroy(go);
    }
}