using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    PlayerStats _stat;

    Vector3 _destPos;          // 마우스 클릭 목적지
    UI_Inven _inven;           // 인벤 UI 캐싱
    float wait_run_ratio = 0f; // 대기(0)↔달리기(1) 블렌드 값

    NavMeshAgent _nma;         // NavMesh 기반 이동 처리(회전은 직접 제어)
    Animator _anim;            // 애니메이터 캐싱(파라미터만 갱신)

    // 근접 기본공격/스킬 공용 플래그
    bool _skillStarted = false;   // 기본공격(연속) 시작 여부(첫 진입 1회용)
    bool _stopSkill = false;      // 마우스 업 이후: “공격을 계속할지/이동으로 전환할지” 판단용
    bool _castingskill = false;   // 스킬 시전 중(이동/기본공격 차단)
    int _currentSkillHash = 0;    // 현재 시전중인 스킬 해시(필요 시 디버그/확장용)

    // Animator State Hash(문자열 비교 비용 줄이기)
    readonly int HASH_WAIT_RUN = Animator.StringToHash("WAIT_RUN");
    readonly int HASH_ATTACK1 = Animator.StringToHash("Attack1");
    readonly int HASH_SKILL1 = Animator.StringToHash("SKILL 1");
    readonly int HASH_SKILL2 = Animator.StringToHash("SKILL 2");
    readonly int HASH_SKILL3 = Animator.StringToHash("SKILL 3");
    readonly int HASH_SKILL4 = Animator.StringToHash("Attack3");

    public enum PlayerState
    {
        Die,
        Moving,
        Idle,
        Skill, // 기본공격/스킬 상태 공용(애니 이벤트로 루프/종료 제어)
    }

    [SerializeField]
    PlayerState _state = PlayerState.Idle;

    /// <summary>
    /// 상태 프로퍼티(현재는 스위치만 있고 동작은 비어있음).
    /// - 필요하면 상태 진입 시 1회 처리(애니/이동/플래그 리셋)를 여기에 모을 수 있음
    /// </summary>
    public PlayerState State
    {
        get { return _state; }
        set
        {
            _state = value;

            Animator anim = GetComponent<Animator>();
            switch (_state)
            {
                case PlayerState.Die:
                    break;
                case PlayerState.Idle:
                    break;
                case PlayerState.Moving:
                    break;
                case PlayerState.Skill:
                    break;
            }
        }
    }

    /// <summary>
    /// 입력 이벤트 연결 + UI/컴포넌트 캐싱.
    /// - KeyAction / MouseAction 구독
    /// - 인벤 UI는 비활성 포함으로 1회 탐색
    /// - NavMeshAgent/Animator 캐싱 및 초기 설정
    /// </summary>
    void Start()
    {
        // 스탯 캐싱(이동 속도 등 사용)
        _stat = gameObject.GetComponent<PlayerStats>();

        // 입력 이벤트는 중복 구독 방지 후 연결
        Managers.Input.KeyAction -= OnKeyboard;
        Managers.Input.KeyAction += OnKeyboard;

        Managers.Input.MouseAction -= OnMouseEvent;
        Managers.Input.MouseAction += OnMouseEvent;

        // UI는 비활성 포함으로 찾아 캐싱
        _inven = FindFirstObjectByType<UI_Inven>(FindObjectsInactive.Include);

        // NavMeshAgent가 없으면 자동 추가
        _nma = gameObject.GetOrAddComponent<NavMeshAgent>();
        _nma.updateRotation = false; // 회전은 직접 Slerp로 처리

        // Animator 캐싱 및 초기 상태 재생(대기/이동 블렌드용)
        _anim = GetComponent<Animator>();
        _anim.Play("WAIT_RUN");
    }

    /// <summary>
    /// 상태(FSM) 분기 업데이트.
    /// - Die / Moving / Idle / Skill 전용 Update로 분기
    /// </summary>
    void Update()
    {
        switch (_state)
        {
            case PlayerState.Die:
                UpdateDie();
                break;

            case PlayerState.Moving:
                UpdateMoving();
                break;

            case PlayerState.Idle:
                UpdateIdle();
                break;

            case PlayerState.Skill:
                UPdateSkill();
                break;
        }
    }

    /// <summary>
    /// 사망 상태 처리(현재 비어있음).
    /// - 필요 시 입력 차단/연출/리스폰 로직 추가
    /// </summary>
    void UpdateDie()
    {
    }

    /// <summary>
    /// 이동 상태 처리.
    /// - 스킬 시전 중이면 이동 차단
    /// - 타겟이 있으면 일정 거리 도달 시 Skill 상태로 전환(근접 공격 진입)
    /// - NavMeshAgent.Move로 목적지 이동 + 회전 보간
    /// - 이동 중 wait_run_ratio를 1로 수렴
    /// </summary>
    void UpdateMoving()
    {
        if (_castingskill) return;

        // 타겟 락이 있으면 근접 거리 도달 시 공격 상태로 전환
        if (_lockTarget != null)
        {
            float distance = (_destPos - transform.position).magnitude;
            if (distance <= 1)
            {
                _state = PlayerState.Skill;
                _skillStarted = false; // 공격 첫 진입 플래그 리셋
                _stopSkill = false;    // 이동 입력 상태 초기화
                return;
            }
        }

        Vector3 dir = _destPos - transform.position;
        dir.y = 0f;

        if (dir.magnitude < 0.1f)
        {
            _state = PlayerState.Idle;
            return;
        }
        else
        {
            float moveDist = Mathf.Clamp(_stat.MoveSpeed * Time.deltaTime, 0, dir.magnitude);
            _nma.Move(dir.normalized * moveDist);

            Debug.DrawRay(transform.position + Vector3.up * 0.5f, dir.normalized, Color.green);

            // 가까운 장애물(블록) 감지 시 이동 중단
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, 1.0f, LayerMask.GetMask("Block")))
            {
                _state = PlayerState.Idle;
                return;
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
        }

        wait_run_ratio = Mathf.Lerp(wait_run_ratio, 1f, 10 * Time.deltaTime);
        _anim.SetFloat("wait_run_ratio", wait_run_ratio);
    }

    /// <summary>
    /// 대기 상태 처리.
    /// - wait_run_ratio를 0으로 수렴(대기 비중 증가)
    /// </summary>
    void UpdateIdle()
    {
        wait_run_ratio = Mathf.Lerp(wait_run_ratio, 0f, 10 * Time.deltaTime);
        _anim.SetFloat("wait_run_ratio", wait_run_ratio);
    }

    /// <summary>
    /// 공격/스킬 상태 처리.
    /// - _castingskill == true면(스킬 시전 중) : 이동/공격 시작 로직은 막고 “타겟 바라보기”만 유지
    /// - 기본공격(Attack1)은 _skillStarted가 false일 때 1회만 시작(CrossFade)
    /// - 실제 타격/루프/종료는 애니메이션 이벤트(OnHitEvent/OnSkillEndEvent)에서 결정
    /// </summary>
    void UPdateSkill()
    {
        // 스킬 시전 중이면(스킬 애니 재생 중) 공격 시작 로직은 하지 않음
        if (_castingskill)
        {
            // 시전 중에도 타겟 바라보기 유지(원하면)
            if (_lockTarget != null)
            {
                Vector3 dir = _lockTarget.transform.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
            }
            return;
        }

        // 기본공격도 타겟이 있으면 바라보기(근접 타격 방향 보정)
        if (_lockTarget != null)
        {
            Vector3 dir = _lockTarget.transform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
        }

        // 기본공격(Attack1) 첫 진입 1회만 시작
        if (_skillStarted == false)
        {
            _skillStarted = true;

            _nma.isStopped = true;
            _nma.ResetPath();

            _anim.CrossFade(HASH_ATTACK1, 0.05f, 0, 0f);
        }
    }

    /// <summary>
    /// 애니메이션 타격 이벤트(Attack1용).
    /// - 타격 프레임마다 호출되는 구조를 전제로 함
    /// - 타겟 유무/마우스 업 여부(_stopSkill)에 따라 다음 상태를 결정
    /// </summary>
    public void OnHitEvent()
    {
        // 스킬 시전 중에는 기본공격 히트 이벤트 무시
        if (_castingskill) return;

        _skillStarted = false;     // 다음 루프를 위해 리셋(다음 프레임에 다시 Attack1 시작 가능)
        _nma.isStopped = false;

        // 타겟이 없으면 Idle로 복귀
        if (_lockTarget == null)
        {
            _state = PlayerState.Idle;
            _anim.CrossFade(HASH_WAIT_RUN, 0.05f, 0, 0f);
            return;
        }

        // 마우스 업 이후면 공격 루프 끊고 타겟 쪽으로 이동 상태로 전환
        if (_stopSkill)
        {
            _destPos = _lockTarget.transform.position;
            _state = PlayerState.Moving;
            return;
        }

        // 계속 공격 유지(상태는 Skill 유지, Attack1을 다시 재생)
        _state = PlayerState.Skill;
        _anim.CrossFade(HASH_ATTACK1, 0.05f, 0, 0f);
    }

    /// <summary>
    /// 스킬 종료 이벤트(스킬 애니 마지막 프레임에서 호출).
    /// - _castingskill 플래그 해제
    /// - 타겟이 있으면 다시 접근(Moving), 없으면 Idle 복귀
    /// </summary>
    public void OnSkillEndEvent()
    {
        _castingskill = false;
        _stopSkill = false;
        _skillStarted = false;

        _nma.isStopped = false;

        if (_lockTarget != null)
        {
            _destPos = _lockTarget.transform.position;
            _state = PlayerState.Moving;
        }
        else
        {
            _state = PlayerState.Idle;
            _anim.CrossFade(HASH_WAIT_RUN, 0.05f, 0, 0f);
        }
    }

    /// <summary>
    /// 스킬 시전 시도(키 입력으로 호출).
    /// - Die/시전중이면 무시
    /// - 이동 정지 + 경로 리셋
    /// - Skill 상태로 전환 후 지정된 스킬 애니로 CrossFade
    /// </summary>
    void TryCastSkill(int skillHash)
    {
        if (_state == PlayerState.Die) return;
        if (_castingskill) return;

        _castingskill = true;
        _currentSkillHash = skillHash;

        _nma.isStopped = true;
        _nma.ResetPath();

        // 시전 중에는 기본공격 루프/이동 로직이 섞이지 않도록 플래그 리셋
        _stopSkill = true;
        _skillStarted = false;

        _state = PlayerState.Skill;
        _anim.CrossFade(skillHash, 0.05f, 0, 0f);
    }

    /// <summary>
    /// 키보드 입력 처리(Managers.Input.KeyAction에서 호출).
    /// - I: 인벤 UI 토글 + 클릭 사운드
    /// - Q/W/E/R: 스킬 시전(해당 애니 해시로 CrossFade)
    /// </summary>
    void OnKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (_inven == null)
                _inven = FindFirstObjectByType<UI_Inven>(FindObjectsInactive.Include);
            if (_inven == null)
                _inven = Managers.UI.ShowSceneUI<UI_Inven>();

            Managers.Sound.Play("SFX/UI/Click", Define.Sound.Effect);
            _inven.gameObject.SetActive(!_inven.gameObject.activeSelf);
        }

        // 스킬 키(시전중이면 TryCastSkill에서 자동 차단)
        if (Input.GetKeyDown(KeyCode.Q)) TryCastSkill(HASH_SKILL1);
        if (Input.GetKeyDown(KeyCode.W)) TryCastSkill(HASH_SKILL2);
        if (Input.GetKeyDown(KeyCode.E)) TryCastSkill(HASH_SKILL3);
        if (Input.GetKeyDown(KeyCode.R)) TryCastSkill(HASH_SKILL4);
    }

    // Raycast 대상 레이어(바닥 + 몬스터)
    int _mask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Monster);

    GameObject _lockTarget; // 몬스터 클릭 시 락온 타겟(Press 동안 _destPos를 타겟으로 계속 갱신)

    /// <summary>
    /// 마우스 이벤트 처리(Managers.Input.MouseAction에서 호출).
    /// - PointerDown: 목적지 설정 + 몬스터면 락온 타겟 지정
    /// - Press: 누르는 동안 목적지 갱신(타겟 있으면 타겟 추적)
    /// - PointerUp: 공격 루프 중단 플래그(_stopSkill) 설정
    /// </summary>
    void OnMouseEvent(Define.MouseEvent evt)
    {
        if (_state == PlayerState.Die) return;
        if (_castingskill) return;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool raycastHit = Physics.Raycast(ray, out hit, 100.0f, _mask);

        switch (evt)
        {
            case Define.MouseEvent.PointerDown:
                {
                    if (raycastHit)
                    {
                        _destPos = hit.point;
                        _state = PlayerState.Moving;

                        _stopSkill = false; // 새 입력 시작이므로 “중단” 해제

                        // 몬스터 클릭이면 락온(거리 도달 후 자동 공격 진입)
                        if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
                            _lockTarget = hit.collider.gameObject;
                        else
                            _lockTarget = null;
                    }
                }
                break;

            case Define.MouseEvent.Press:
                {
                    // 누르는 동안 타겟이 있으면 타겟 추적, 없으면 마우스 지점 추적
                    if (_lockTarget != null)
                    {
                        _destPos = _lockTarget.transform.position;
                    }
                    else
                    {
                        if (raycastHit)
                            _destPos = hit.point;
                    }
                }
                break;

            case Define.MouseEvent.PointerUp:
                {
                    // 마우스를 떼면 “공격 루프 끊고 이동으로 전환” 신호
                    _stopSkill = true;
                }
                break;
        }
    }
}