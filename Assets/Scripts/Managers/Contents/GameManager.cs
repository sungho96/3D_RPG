using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    GameObject _player;                         // 현재 플레이어(1개 전제)
    HashSet<GameObject> _monsters = new HashSet<GameObject>(); // 현재 몬스터 목록(중복 방지)

    public Action<int> OnSpawnEvent;
    public GameObject GetPlayer() { return _player; }

    /// <summary>
    /// 월드 오브젝트 생성.
    /// - Resources 기반 Instantiate(풀링 대상이면 Pool 사용)
    /// - 생성한 오브젝트를 타입에 따라 내부 목록에 등록
    /// </summary>
    public GameObject Spawn(Define.WorldObject type, string path, Transform parent = null)
    {
        GameObject go = Managers.Resource.Instantiate(path, parent);

        // 타입별로 관리 대상 등록
        switch (type)
        {
            case Define.WorldObject.Monster:
                _monsters.Add(go);
                if (OnSpawnEvent != null)
                    OnSpawnEvent.Invoke(1);
                break;

            case Define.WorldObject.Player:
                _player = go;
                break;
        }

        return go;
    }

    /// <summary>
    /// 오브젝트의 월드 타입 판별.
    /// - BaseController가 없으면 Unknown 처리
    /// - BaseController가 가진 WorldObjectType을 기준으로 반환
    /// </summary>
    public Define.WorldObject GetWorldObjectType(GameObject go)
    {
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return Define.WorldObject.Unknown;

        return bc.WorldObjectType;
    }

    /// <summary>
    /// 월드 오브젝트 제거(디스폰).
    /// - 타입에 따라 내부 목록에서 해제
    /// - ResourceManager.Destroy로 제거(풀링이면 반환, 아니면 실제 Destroy)
    /// </summary>
    public void Despawn(GameObject go)
    {
        Define.WorldObject type = GetWorldObjectType(go);

        // 타입별 관리 목록에서 제거
        switch (type)
        {
            case Define.WorldObject.Monster:
                {
                    if (_monsters.Contains(go))
                    {
                        _monsters.Remove(go);
                        if (OnSpawnEvent != null)
                            OnSpawnEvent.Invoke(-1);
                           
                    }
                }
                break;

            case Define.WorldObject.Player:
                {
                    if (_player == go)
                        _player = null;
                }
                break;
        }

        // 최종 제거(풀링 대상이면 Pool로 반환)
        Managers.Resource.Destroy(go);
    }
}