using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterConroller : BaseController
{
    Stat _stat; // 몬스터 스탯(이동속도/공격력/체력 등)

    float _scanRange = 10;   // 플레이어 탐지 범위
    float _attackRange = 2;  // 근접 공격 시작 범위

    NavMeshAgent _nma; // NavMesh 이동 처리용

    /// <summary>
    /// 몬스터 초기화.
    /// - Stat / NavMeshAgent 캐싱
    /// - 월드 스페이스 HPBar가 없으면 생성
    /// </summary>
    public override void Init()
    {
        WorldObjectType = Define.WorldObject.Monster;
        // 몬스터 스탯 캐싱
        _stat = gameObject.GetComponent<Stat>();

        // HPBar가 아직 없을 때만 생성(중복 생성 방지)
        if (gameObject.GetComponentInChildren<UI_HPBar>() == null)
            Managers.UI.MakeWorldSpaceUI<UI_HPBar>(transform);

        // NavMeshAgent가 없으면 자동 추가
        _nma = gameObject.GetOrAddComponent<NavMeshAgent>();
    }

    /// <summary>
    /// 대기 상태 처리.
    /// - 플레이어를 탐색하고, 탐지 범위 안이면 락온 후 Moving 상태로 전환
    /// </summary>
    protected override void UpdateIdle()
    {
        // 태그 기반으로 플레이어 탐색
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        // 탐지 범위 체크
        float distance = (player.transform.position - transform.position).magnitude;
        if (distance < _scanRange)
        {
            _lockTarget = player;                    // 추적 대상 지정
            _destPos = player.transform.position;    // 현재 타겟 위치를 목적지로 저장
            State = Define.State.Moving;             // 추적 시작
            return;
        }
    }

    /// <summary>
    /// 이동(추적) 상태 처리.
    /// - 타겟이 있으면 계속 목적지를 갱신하며 추적
    /// - 공격 범위 도달 시 Skill 상태로 전환
    /// - NavMeshAgent.SetDestination으로 이동
    /// </summary>
    protected override void UpdateMoving()
    {
        // 락온 타겟이 있을 때: 거리 확인 후 공격 상태 진입 여부 판단
        if (_lockTarget != null)
        {
            float distance = (_lockTarget.transform.position - transform.position).magnitude;

            // 타겟은 움직일 수 있으므로 매 프레임 목적지 갱신
            _destPos = _lockTarget.transform.position;

            if (distance <= _attackRange)
            {
                // 공격 시작 전 이동 정지(현재 위치를 목적지로 고정)
                _nma.SetDestination(transform.position);
                State = Define.State.Skill;
                return;
            }
        }

        // 목적지 방향(회전은 수평만 반영)
        Vector3 dir = _destPos - transform.position;
        dir.y = 0f;

        // 도착 판정(목적지에 거의 도달)
        if (dir.magnitude < 0.1f)
        {
            State = Define.State.Idle;
            return;
        }
        else
        {
            // NavMesh 목적지/이동속도 갱신
            _nma.SetDestination(_destPos);
            _nma.speed = _stat.MoveSpeed;

            // 이동 방향으로 회전 보간
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
        }
    }

    /// <summary>
    /// 공격(스킬) 상태 처리.
    /// - 타겟이 있으면 타겟 방향을 바라보도록 회전 유지
    /// - 실제 공격 판정/데미지는 애니메이션 이벤트(OnHitEvent)에서 처리
    /// </summary>
    protected override void UpdateSkill()
    {
        if (_lockTarget != null)
        {
            Vector3 dir = _lockTarget.transform.position - transform.position;
            // (선택) 수평 회전만 쓰고 싶으면 dir.y = 0f; 추가 가능
            Quaternion quat = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, quat, 20 * Time.deltaTime);
        }
    }

    /// <summary>
    /// 몬스터 공격 타격 이벤트(애니메이션 이벤트에서 호출).
    /// - 타겟에게 데미지 적용
    /// - 타겟 생존/거리 상태에 따라 Skill(연속 공격) 또는 Moving(재추적)으로 전환
    /// - 타겟이 없거나 사망했으면 Idle 복귀
    /// </summary>
    void OnHitEvent()
    {
        if (_lockTarget != null)
        {
            // 타겟 스탯 / 내 스탯 참조
            Stat targetStat = _lockTarget.GetComponent<Stat>();

            // 방어력을 고려한 최종 데미지 계산(0 미만 방지)
            int damage = Mathf.Max(0, _stat.Attack - targetStat.Defense);
            targetStat.Hp -= damage;

            if(targetStat.Hp <=0)
            {
                Managers.Game.Despawn(targetStat.gameObject);
            }

            // 타겟이 살아있으면 거리 기준으로 공격 지속/재추적 판단
            if (targetStat.Hp > 0)
            {
                float distace = (_lockTarget.transform.position - transform.position).magnitude;

                if (distace <= _attackRange)
                    State = Define.State.Skill;   // 사거리 안이면 연속 공격
                else
                    State = Define.State.Moving;  // 멀어졌으면 다시 추적
            }
            else
            {
                // 타겟 사망 시 대기 상태 복귀
                State = Define.State.Idle;
            }
        }
        else
        {
            // 타겟을 잃어버렸으면 대기 상태 복귀
            State = Define.State.Idle;
        }
    }
}