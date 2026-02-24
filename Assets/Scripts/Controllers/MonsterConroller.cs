using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterConroller : BaseController
{
    Stat _stat;

    float _scanRange = 10;

    float _attackRange = 2;

    NavMeshAgent _nma;
    public override void Init()
    {
        // 스탯 캐싱(이동 속도/공격/스킬 데미지 참조)
        _stat = gameObject.GetComponent<Stat>();

        // 월드 스페이스 HPBar 생성(타겟/플레이어 머리 위 UI)
        if (gameObject.GetComponentInChildren<UI_HPBar>()== null)
            Managers.UI.MakeWorldSpaceUI<UI_HPBar>(transform);
        _nma = gameObject.GetOrAddComponent<NavMeshAgent>();
    }

    protected override void UpdateIdle()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
            return;

        float distance = (player.transform.position - transform.position).magnitude;
        if(distance < _scanRange)
        {
            _lockTarget = player;
            _destPos = player.transform.position;
            State = Define.State.Moving;
            return;
        }
    }

    protected override void UpdateMoving()
    {
        // 락온 타겟이 있을 때: 근접 거리 도달하면 공격 상태로 전환
        if (_lockTarget != null)
        {
            float distance = (_lockTarget.transform.position - transform.position).magnitude;
            _destPos = _lockTarget.transform.position;

            if (distance <= _attackRange)
            {
                _nma.SetDestination(transform.position);
                State = Define.State.Skill;
                return;
            }
        }

        // 목적지 방향(수평 이동만)
        Vector3 dir = _destPos - transform.position;
        dir.y = 0f;

        // 도착 판정
        if (dir.magnitude < 0.1f)
        {
            State = Define.State.Idle;
            return;
        }

        else
        {
            _nma.SetDestination(_destPos);
            _nma.speed = _stat.MoveSpeed;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
        }
    }

    protected override void UpdateSkill()
    {
        if (_lockTarget != null)
        {
            Vector3 dir = _lockTarget.transform.position - transform.position;
            Quaternion quat = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, quat, 20 * Time.deltaTime);
        }
    }

    void OnHitEvent()
    {
        if(_lockTarget != null )
        {
            Stat targetStat =  _lockTarget.GetComponent<Stat>();
            Stat myStat = gameObject.GetComponent<Stat>();
            int damage = Mathf.Max(0, myStat.Attack - targetStat.Defense);
            targetStat.Hp -= damage;

            if(targetStat.Hp > 0 )
            {
                float distace = (_lockTarget.transform.position - transform.position).magnitude;
                if (distace <= _attackRange)
                    State = Define.State.Skill;
                else
                    State = Define.State.Moving;
            }
            else
            {
                State = Define.State.Idle;
            }
        }
        else
        {
            State = Define.State.Idle;
        }
    }
}
