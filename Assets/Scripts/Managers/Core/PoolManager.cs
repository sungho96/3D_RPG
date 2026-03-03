using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    /*
     * "프리팹 1종류당 1개의 풀"이 필요함
     * 프리팹별로 (원본, Root, Stack)을 묶어서 관리하면
     * PoolManager는 "딕셔너리로 Pool 찾아주는 역할"만 하면 됨
     * 즉, Pool = 창고(프리팹 단위), PoolManager = 창고 관리자(여러 창고 통합)
     */
    class Pool
    {
        // 이 풀의 기준이 되는 원본 프리팹(이 프리팹만 이 풀에서 뽑고/반환함)
        public GameObject Original { get; private set; }

        // 풀에 들어있는 오브젝트들이 묶여 있을 부모 Root
        // (씬 하이어라키 정리 + DontDestroyOnLoad 루트 밑으로 모음)
        public Transform Root { get; set; }

        // 비활성화되어 대기 중인 Poolable 오브젝트 스택
        Stack<Poolable> _poolstack = new Stack<Poolable>();

        /// <summary>
        /// 풀 초기화
        /// Root 생성
        /// count 만큼 미리 생성해서 Push(대기 상태로 쌓기)
        /// count는 임의 숫자
        /// </summary>
        public void Init(GameObject original, int count = 5)
        {
            Original = original;

            // 이 풀 전용 Root 오브젝트 생성
            Root = new GameObject().transform;
            Root.name = $"{original.name}_Root";

            // 초기 개수만큼 미리 만들어서 비활성 상태로 쌓아둠
            for (int i = 0; i < count; i++)
            {
                Push(Create());
            }
        }

        /// <summary>
        /// 실제 인스턴스 생성
        /// 풀에 남은 게 없을 때만 호출되는게 이상적
        /// </summary>
        Poolable Create()
        {
            // 원본 프리팹 복제
            GameObject go = Object.Instantiate<GameObject>(Original);

            // (Clone) 이름 제거 목적: name을 원본과 동일하게 맞춤
            // (딕셔너리 키를 name으로 쓰기 때문에 이름이 중요)
            go.name = Original.name;

            // Poolable이 없으면 붙이고, 있으면 그대로 사용
            return go.GetOrAddComponent<Poolable>();
        }

        /// <summary>
        /// 사용 끝난 오브젝트를 Root 아래로 옮기고 비활성화
        /// 스택에 넣어 다음 Pop 때 재사용
        /// </summary>
        public void Push(Poolable poolable)
        {
            if (poolable == null)
                return;

            // 풀 루트 밑으로 되돌려서 하이어라키 정리
            poolable.transform.parent = Root;

            // 화면/로직에서 완전히 내려가도록 비활성화
            poolable.gameObject.SetActive(false);

            // 사용 상태 플래그(대기 상태)
            poolable.IsUsing = false;

            // 스택에 쌓기
            _poolstack.Push(poolable);
        }

        /// <summary>
        /// 스택에 남아있으면 재사용, 없으면 새로 Create
        /// 활성화 후 parent로 붙임
        /// </summary>
        public Poolable Pop(Transform parent)
        {
            Poolable poolable;

            // 1) 남아있으면 재사용
            if (_poolstack.Count > 0)
                poolable = _poolstack.Pop();
            // 2) 없으면 새로 생성(=풀 확장)
            else
                poolable = Create();

            // 활성화(보이기/업데이트 대상 복귀)
            poolable.gameObject.SetActive(true);

            Transform targetParent = parent ?? Managers.Scene.CurrentScene.transform;
            poolable.transform.SetParent(targetParent);

            poolable.IsUsing = true;

            return poolable;
        }
    }

    // "프리팹 이름"을 키로 풀을 찾아오는 딕셔너리
    // 주의: 이름이 같은 프리팹이 생기면 충돌할 수 있음(규칙으로 방지하거나 키 확장 필요)
    Dictionary<string, Pool> _pool = new Dictionary<string, Pool>();

    // 모든 풀들의 Root를 묶어둘 상위 루트(씬 전환해도 유지)
    Transform _root;

    /// <summary>
    /// PoolManager 루트 초기화
    /// @Pool_Root를 만들고 DontDestroyOnLoad로 유지
    /// </summary>
    public void Init()
    {
        if (_root == null)
        {
            _root = new GameObject { name = "@Pool_Root" }.transform;
            Object.DontDestroyOnLoad(_root);
        }
    }

    /// <summary>
    /// 특정 프리팹에 대한 풀을 생성
    /// count만큼 미리 만들어 스택에 쌓아둠
    /// </summary>
    public void CreatePool(GameObject original, int count = 5)
    {
        if (_root == null)
            Init();

        if (_pool.ContainsKey(original.name))
            return;

        Pool pool = new Pool();
        pool.Init(original, count);

        // 각 풀 Root를 공용 루트 밑으로 묶기(관리/정리 목적)
        pool.Root.parent = _root;

        // 키는 "원본 프리팹 이름"
        _pool.Add(original.name, pool);
    }

    /// <summary>
    /// 반환(외부에서 Poolable을 풀로 돌려보낼 때 사용)
    /// </summary>
    public void push(Poolable poolable) // 반환
    {
        if (poolable == null)
            return;

        if (poolable.IsUsing == false)
            return;

        // key로 name 사용(위에서 Create할 때 name을 원본과 동일하게 맞춘 이유)
        string name = poolable.gameObject.name;

        // 해당 이름의 풀이 없으면, 풀 관리 대상이 아니므로 그냥 파괴
        if (_pool.ContainsKey(name) == false)
        {
            GameObject.Destroy(poolable.gameObject);
            return;
        }

        _pool[name].Push(poolable);
    }

    /// <summary>
    /// 꺼내기(외부에서 원본 프리팹 기준으로 요청)
    /// 풀이 없으면 CreatePool로 자동 생성(기본 count=5)
    /// </summary>
    public Poolable Pop(GameObject orginal, Transform parent = null) // 주는것
    {
        if (_pool.ContainsKey(orginal.name) == false)
            CreatePool(orginal);

        return _pool[orginal.name].Pop(parent);
    }

    /// <summary>
    /// 원본 프리팹 조회
    /// ResourceManager.Load에서 "풀에 등록된 원본"을 우선 반환하려고 사용
    /// </summary>
    public GameObject GetOriginal(string name)
    {
        if (_pool.ContainsKey(name) == false)
            return null;

        return _pool[name].Original;
    }

    /// <summary>
    /// 전체 풀 정리
    /// 모든 풀 Root 오브젝트 삭제
    /// 딕셔너리 초기화
    /// </summary>
    public void Clear()
    {
        foreach (Transform child in _root)
            GameObject.Destroy(child.gameObject);

        _pool.Clear();
    }
}