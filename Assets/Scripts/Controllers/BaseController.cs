using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseController : MonoBehaviour
{

    [SerializeField]
    protected Define.State _state = Define.State.Idle;

    [SerializeField]
    protected GameObject _lockTarget; // 몬스터 클릭 시 락온 타겟(Press 동안 _destPos를 타겟으로 계속 갱신)

    [SerializeField]
    protected Vector3 _destPos;          // 마우스 클릭 목적지

    private void Start()
    {
        Init();
    }

    /// <summary>
    /// 상태 프로퍼티(현재는 스위치만 있고 동작은 비어있음).
    /// - 필요하면 상태 진입 시 1회 처리(애니/이동/플래그 리셋)를 여기에 모을 수 있음
    /// </summary>
	public virtual Define.State State
    {
        get { return _state; }
        set
        {
            _state = value;

            Animator anim = GetComponent<Animator>();
            switch (_state)
            {
                case Define.State.Die:
                    break;
                case Define.State.Idle:
                    anim.CrossFade("WAIT", 0.1f);
                    break;
                case Define.State.Moving:
                    anim.CrossFade("RUN", 0.1f);
                    break;
                case Define.State.Skill:
                    anim.CrossFade("ATTACK", 0.1f, -1, 0);
                    break;
            }
        }
    }
    /// <summary>
    /// 상태(FSM) 분기 업데이트.
    /// - Die / Moving / Idle / Skill 전용 Update로 분기
    /// </summary>
    void Update()
    {
        switch (_state)
        {
            case Define.State.Die:
                UpdateDie();
                break;

            case Define.State.Moving:
                UpdateMoving();
                break;

            case Define.State.Idle:
                UpdateIdle();
                break;

            case Define.State.Skill:
                UpdateSkill();
                break;
        }
    }
    ///자식패널에서 Init 설정
    public abstract void Init();

    /// <summary>사망 상태 전용 업데이트(자식에서 override).</summary>
    protected virtual void UpdateDie() { }

    /// <summary>이동 상태 전용 업데이트(자식에서 override).</summary>
    protected virtual void UpdateMoving() { }

    /// <summary>대기 상태 전용 업데이트(자식에서 override).</summary>
    protected virtual void UpdateIdle() { }

    /// <summary>스킬 상태 전용 업데이트(자식에서 override).</summary>
    protected virtual void UpdateSkill() { }
}
