using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : BaseController
{
    PlayerStats _stat; // 플레이어 스탯(이동속도/공격력/스킬데미지 등)

    UI_Inven _inven;           // 인벤 UI 캐싱
    float wait_run_ratio = 0f; // 대기(0)↔달리기(1) 블렌드 값

    NavMeshAgent _nma;         // NavMesh 기반 이동 처리(회전은 직접 제어)
    Animator _anim;            // 애니메이터 캐싱(파라미터만 갱신)

    // 근접 기본공격/스킬 공용 플래그
    bool _skillStarted = false;   // 기본공격 시작 여부(첫 진입 1회용)
    bool _stopSkill = false;      // PointerUp 이후: “공격 지속 vs 이동 전환” 판단용
    bool _castingskill = false;   // 스킬 시전 중(이동/기본공격 차단)
    int _currentSkillHash = 0;    // 현재 시전중인 스킬 해시(스킬 타격 이벤트에서 사용)

    // Animator State Hash(문자열 비교 비용 줄이기)
    readonly int HASH_WAIT_RUN = Animator.StringToHash("WAIT_RUN");
    readonly int HASH_ATTACK1 = Animator.StringToHash("Attack1");
    readonly int HASH_SKILL1 = Animator.StringToHash("SKILL 1");
    readonly int HASH_SKILL2 = Animator.StringToHash("SKILL 2");
    readonly int HASH_SKILL3 = Animator.StringToHash("SKILL 3");
    readonly int HASH_SKILL4 = Animator.StringToHash("Attack3");


    /// <summary>
    /// 입력 이벤트 연결 + UI/컴포넌트 캐싱.
    /// - KeyAction / MouseAction 구독
    /// - 인벤 UI는 비활성 포함으로 1회 탐색
    /// - NavMeshAgent/Animator 캐싱 및 초기 설정
    /// - 월드 스페이스 HPBar 생성(플레이어 머리 위 UI)
    /// </summary>
    public override void Init()
    {
        WorldObjectType = Define.WorldObject.Player;
        // 스탯 캐싱(이동 속도/공격/스킬 데미지 참조)
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

        // 월드 스페이스 HPBar 생성(타겟/플레이어 머리 위 UI)
         if (gameObject.GetComponentInChildren<UI_HPBar>()== null)
        Managers.UI.MakeWorldSpaceUI<UI_HPBar>(transform);
    }

    /// <summary>
    /// 이동 상태 처리.
    /// - 스킬 시전 중이면 이동 차단
    /// - 타겟이 있으면 근접 거리 도달 시 Skill 상태로 전환(근접 공격 진입)
    /// - NavMeshAgent.Move로 목적지 이동 + 회전 보간
    /// - 이동 중 wait_run_ratio를 1로 수렴
    /// </summary>
    protected override void UpdateMoving()
    {
        // 스킬 시전 중에는 이동 로직 중단
        if (_castingskill) return;

        // 락온 타겟이 있을 때: 근접 거리 도달하면 공격 상태로 전환
        if (_lockTarget != null)
        {
            float distance = (_destPos - transform.position).magnitude;
            if (distance <= 1)
            {
                _state = Define.State.Skill;
                _skillStarted = false; // 공격 첫 진입 플래그 리셋
                _stopSkill = false;    // 새 공격 루프 시작(중단 신호 해제)
                return;
            }
        }

        // 목적지 방향(수평 이동만)
        Vector3 dir = _destPos - transform.position;
        dir.y = 0f;

        // 도착 판정
        if (dir.magnitude < 0.1f)
        {
            _state = Define.State.Idle;
            return;
        }
        else
        {
            // 디버그: 이동 방향 표시
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, dir.normalized, Color.green);

            // 가까운 장애물(블록) 감지 시 이동 중단
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, 1.0f, LayerMask.GetMask("Block")))
            {
                _state = Define.State.Idle;
                return;
            }
            // 이동 거리 계산(오버슈트 방지)
            float moveDist = Mathf.Clamp(_stat.MoveSpeed * Time.deltaTime, 0, dir.magnitude);
            transform.position += dir.normalized * moveDist;
            // 이동 방향으로 회전 보간
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
        }

        // 이동 애니 블렌딩(달리기 비중 증가)
        wait_run_ratio = Mathf.Lerp(wait_run_ratio, 1f, 10 * Time.deltaTime);
        _anim.SetFloat("wait_run_ratio", wait_run_ratio);
    }

    /// <summary>
    /// 대기 상태 처리.
    /// - wait_run_ratio를 0으로 수렴(대기 비중 증가)
    /// </summary>
    protected override void UpdateIdle()
    {
        wait_run_ratio = Mathf.Lerp(wait_run_ratio, 0f, 10 * Time.deltaTime);
        _anim.SetFloat("wait_run_ratio", wait_run_ratio);
    }

    /// <summary>
    /// 공격/스킬 상태 처리.
    /// - _castingskill == true : 스킬 시전 중(타겟 바라보기만 유지)
    /// - _castingskill == false : 기본공격(Attack1) 루프 시작/유지
    /// - 실제 타격/종료는 애니메이션 이벤트(OnHitEvent / OnSkillHitEvent / OnSkillEndEvent)에서 처리
    /// </summary>
    protected override void UpdateSkill()
    {
        // 스킬 시전 중이면 기본공격 시작 로직은 막고 방향만 보정
        if (_castingskill)
        {
            if (_lockTarget != null)
            {
                Vector3 dir = _lockTarget.transform.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
            }
            return;
        }

        // 기본공격 중에도 타겟을 바라보게 유지
        if (_lockTarget != null)
        {
            Vector3 dir = _lockTarget.transform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
        }

        // 기본공격(Attack1) 첫 진입 1회만 CrossFade
        if (_skillStarted == false)
        {
            _skillStarted = true;

            _nma.isStopped = true;
            _nma.ResetPath();

            _anim.CrossFade(HASH_ATTACK1, 0.05f, 0, 0f);
        }
    }

    /// <summary>
    /// 기본공격 타격 이벤트(Attack1용).
    /// - 타격 프레임마다 호출되는 구조를 전제로 함
    /// - 타겟 유무/PointerUp(_stopSkill)에 따라 다음 상태를 결정
    /// </summary>
    public void OnHitEvent()
    {
        // 스킬 시전 중에는 기본공격 히트 이벤트 무시
        if (_castingskill) return;

        // (선택) 타겟이 있으면 데미지 적용
        if (_lockTarget != null)
        {
            // 락온 타겟의 스탯 가져오기
            Stat targetStat = _lockTarget.GetComponent<Stat>();
            targetStat.OnAttacked(_stat);
        }

        // 다음 루프를 위해 리셋(다음 프레임에 Attack1 재시작 가능)
        _skillStarted = false;
        _nma.isStopped = false;

        // 타겟이 없으면 Idle로 복귀
        if (_lockTarget == null)
        {
            _state = Define.State.Idle;
            _anim.CrossFade(HASH_WAIT_RUN, 0.05f, 0, 0f);
            return;
        }

        // 마우스 업 이후면 공격 루프 중단 → 이동 상태로 전환
        if (_stopSkill)
        {
            _destPos = _lockTarget.transform.position;
            _state = Define.State.Moving;
            return;
        }

        // 계속 공격 유지(Attack1 재생)
        _state = Define.State.Skill;
        _anim.CrossFade(HASH_ATTACK1, 0.05f, 0, 0f);
    }

    /// <summary>
    /// 스킬 종료 이벤트(스킬 애니 마지막 프레임에서 호출).
    /// - 시전 플래그/공격 플래그 리셋
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
            _state = Define.State.Moving;
        }
        else
        {
            _state = Define.State.Idle;
            _anim.CrossFade(HASH_WAIT_RUN, 0.05f, 0, 0f);
        }
    }

    /// <summary>
    /// 스킬 타격 이벤트(스킬 애니 타격 프레임에서 호출).
    /// - 현재 시전중인 스킬(_currentSkillHash)에 따라 데미지를 결정
    /// - 방어력을 고려해 타겟 HP 감소
    /// </summary>
    public void OnSkillHitEvent()
    {
        // 스킬 시전 중일 때만 처리
        if (!_castingskill) return;

        // 타겟이 없으면 타격 처리 불가
        if (_lockTarget == null) return;

        // 타겟 스탯 컴포넌트 확인
        Stat targetStat = _lockTarget.GetComponent<Stat>();
        if (targetStat == null) return;

        // 현재 스킬 데미지 값 계산
        int skillDamage = GetCurrentSkillDamage();

        // 방어력을 고려한 데미지 적용
        int damage = Mathf.Max(0, skillDamage - targetStat.Defense);
        targetStat.Hp -= damage;
    }

    /// <summary>
    /// 현재 시전중인 스킬 해시(_currentSkillHash)에 대응하는 스킬 데미지 반환.
    /// - PlayerStats의 Skill1~Skill4 값을 사용
    /// </summary>
    int GetCurrentSkillDamage()
    {
        if (_currentSkillHash == HASH_SKILL1) return Mathf.RoundToInt(_stat.Skill1);
        if (_currentSkillHash == HASH_SKILL2) return Mathf.RoundToInt(_stat.Skill2);
        if (_currentSkillHash == HASH_SKILL3) return Mathf.RoundToInt(_stat.Skill3);
        if (_currentSkillHash == HASH_SKILL4) return Mathf.RoundToInt(_stat.Skill4);
        return 0;
    }

    /// <summary>
    /// 스킬 시전 시도(키 입력으로 호출).
    /// - Die/시전중이면 무시
    /// - 이동 정지 + 경로 리셋
    /// - Skill 상태로 전환 후 지정된 스킬 애니로 CrossFade
    /// </summary>
    void TryCastSkill(int skillHash)
    {
        if (_state == Define.State.Die) return;
        if (_castingskill) return;

        // 시전 시작 플래그 + 현재 스킬 해시 저장
        _castingskill = true;
        _currentSkillHash = skillHash;

        // 시전 중 이동 중단
        _nma.isStopped = true;
        _nma.ResetPath();

        // 기본공격 루프가 섞이지 않도록 리셋
        _stopSkill = true;
        _skillStarted = false;

        _state = Define.State.Skill;
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

        // 스킬 키 입력(시전중이면 TryCastSkill에서 자동 차단)
        if (Input.GetKeyDown(KeyCode.Q)) TryCastSkill(HASH_SKILL1);
        if (Input.GetKeyDown(KeyCode.W)) TryCastSkill(HASH_SKILL2);
        if (Input.GetKeyDown(KeyCode.E)) TryCastSkill(HASH_SKILL3);
        if (Input.GetKeyDown(KeyCode.R)) TryCastSkill(HASH_SKILL4);
    }

    // Raycast 대상 레이어(바닥 + 몬스터)
    int _mask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Monster);



    /// <summary>
    /// 마우스 이벤트 처리(Managers.Input.MouseAction에서 호출).
    /// - PointerDown: 목적지 설정 + 몬스터면 락온 타겟 지정
    /// - Press: 누르는 동안 목적지 갱신(타겟 있으면 타겟 추적)
    /// - PointerUp: 공격 루프 중단 플래그(_stopSkill) 설정
    /// </summary>
    void OnMouseEvent(Define.MouseEvent evt)
    {
        if (_state == Define.State.Die) return;
        if (_castingskill) return;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool raycastHit = Physics.Raycast(ray, out hit, 100.0f, _mask);

        switch (evt)
        {
            case Define.MouseEvent.PointerDown:
                {
                    // 클릭 지점이 있으면 목적지 갱신 + 이동 상태로 전환
                    if (raycastHit)
                    {
                        _destPos = hit.point;
                        _state = Define.State.Moving;

                        // 새 입력 시작이므로 “중단” 해제
                        _stopSkill = false;

                        // 몬스터 클릭이면 락온, 아니면 락온 해제
                        if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
                            _lockTarget = hit.collider.gameObject;
                        else
                            _lockTarget = null;
                    }
                }
                break;

            case Define.MouseEvent.Press:
                {
                    // 누르는 동안: 타겟 있으면 타겟 추적 / 없으면 마우스 지점 추적
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