using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 목표 몬스터 수를 유지하기 위해, NavMesh 위의 랜덤 위치로 스폰을 예약하는 풀입니다.
/// </summary>
public class SpawningPool : MonoBehaviour
{
    [SerializeField]
    int _monsterCount = 0;
    int _reserveCount = 0;

    [SerializeField]
    int _keepMonsterCount = 0;

    [SerializeField]
    Vector3 _spawnPos;
    [SerializeField]
    float _spawnRadious = 15.0f;
    [SerializeField]
    float _spawnTime = 15.0f;

    /// <summary>
    /// 스폰 중심 좌표를 설정합니다.
    /// </summary>
    public void SetSpawnPos(Vector3 pos) { _spawnPos = pos; }

    /// <summary>
    /// 외부에서 전달된 변화량만큼 몬스터 수를 누적합니다.
    /// </summary>
    public void AddMonsterCount(int value) { _monsterCount += value; }

    /// <summary>
    /// 유지할 목표 몬스터 수를 설정합니다.
    /// </summary>
    public void SetKeepMonsterCount(int count) { _keepMonsterCount = count; }

    /// <summary>
    /// 스폰 중심과 반경을 동시에 설정합니다.
    /// </summary>
    public void SetSpawnCenter(Vector3 center, float radius)
    {
        _spawnPos = center;
        _spawnRadious = radius;
    }

    /// <summary>
    /// 스폰 카운트 갱신 이벤트를 연결합니다. 중복 구독을 방지하기 위해 먼저 해제합니다.
    /// </summary>
    void Start()
    {
        Managers.Game.OnSpawnEvent -= AddMonsterCount;
        Managers.Game.OnSpawnEvent += AddMonsterCount;
    }

    /// <summary>
    /// (현재 + 예약) 수가 목표보다 부족하면 스폰을 예약합니다.
    /// </summary>
    void Update()
    {
        // 코루틴이 겹쳐 예약되는 것을 막기 위해 예약 수까지 합쳐서 판단합니다.
        if (_reserveCount + _monsterCount < _keepMonsterCount)
            StartCoroutine(ReserveSpawn());
    }

    /// <summary>
    /// 랜덤 지연 후 NavMesh 위의 유효한 위치를 찾아 스폰합니다.
    /// </summary>
    IEnumerator ReserveSpawn()
    {
        // 예약을 먼저 올려두면 같은 프레임에서 중복 예약이 줄어듭니다.
        _reserveCount++;

        yield return new WaitForSeconds(Random.Range(0, _spawnTime));

        Vector3 randPos;
        while (true)
        {
            Vector3 randDir = Random.insideUnitSphere * Random.Range(0, _spawnRadious);
            randDir.y = 0;
            Vector3 candidate = _spawnPos + randDir;

            // NavMesh 위로 보정된 위치를 얻을 때까지 반복합니다.
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3.0f, NavMesh.AllAreas))
            {
                randPos = hit.position;
                break;
            }
        }

        GameObject obj = Managers.Game.Spawn(Define.WorldObject.Monster, "Knight", transform);

        // 위치/워프 처리 중 보이는 순간 튐을 줄이기 위해 먼저 꺼둡니다.
        obj.SetActive(false);

        obj.transform.position = randPos;

        // Agent가 있는 좌표를 강제로 맞춰서 시작 프레임 이동 계산을 안정화합니다.
        NavMeshAgent nma = obj.GetOrAddComponent<NavMeshAgent>();
        nma.Warp(randPos);

        obj.SetActive(true);

        _reserveCount--;
    }
}