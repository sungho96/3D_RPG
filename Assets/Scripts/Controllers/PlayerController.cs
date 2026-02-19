using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _speed = 10.0f;

    Vector3 _destPos;          // 마우스 클릭 목적지
    UI_Inven _inven;           // 인벤 UI 캐싱
    float wait_run_ratio = 0f; // 대기(0)↔달리기(1) 블렌드 값

    NavMeshAgent _nma;         // NavMesh 기반 이동 처리(회전은 직접 제어)
    Animator _anim;            // 애니메이터 캐싱(파라미터만 갱신)

    public enum PlayerState { Die, Moving, Idle }
    PlayerState _state = PlayerState.Idle;

    /// <summary>
    /// 입력 이벤트 연결 + UI/컴포넌트 캐싱.
    /// - KeyAction / MouseAction 구독
    /// - 인벤 UI는 비활성 포함으로 1회 탐색
    /// - NavMeshAgent/Animator 캐싱 및 초기 설정
    /// </summary>
    void Start()
    {
        // 입력 이벤트는 중복 구독 방지 후 연결
        Managers.Input.KeyAction -= OnKeyboard;
        Managers.Input.KeyAction += OnKeyboard;

        Managers.Input.MouseAction -= OnMouseClicked;
        Managers.Input.MouseAction += OnMouseClicked;

        // UI는 비활성 포함으로 찾아 캐싱
        _inven = FindFirstObjectByType<UI_Inven>(FindObjectsInactive.Include);

        // NavMeshAgent가 없으면 자동 추가(유틸 확장 함수 기반)
        _nma = gameObject.GetOrAddComponent<NavMeshAgent>();

        // 회전은 Agent가 아니라 스크립트에서 직접 처리(Quaternion.Slerp)
        _nma.updateRotation = false;

        // Animator 캐싱 및 초기 상태 재생(대기/이동 블렌드용 상태)
        _anim = GetComponent<Animator>();
        _anim.Play("WAIT_RUN");
    }

    /// <summary>
    /// 상태(FSM) 분기 업데이트.
    /// - Die / Moving / Idle 각각 전용 Update로 분기
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
    /// - 목적지까지 NavMeshAgent.Move로 이동
    /// - 도착 판정/장애물 감지 시 Idle 전환
    /// - 애니메이션 블렌드 값(wait_run_ratio)을 1로 수렴
    /// </summary>
    void UpdateMoving()
    {
        // 목적지 방향(수평 이동만 고려)
        Vector3 dir = _destPos - transform.position;
        dir.y = 0f;

        // 도착 판정(거리 임계값)
        if (dir.magnitude < 0.1f)
        {
            _state = PlayerState.Idle;
            return;
        }
        else
        {
            // 프레임당 이동 거리(남은 거리 이상 이동하지 않도록 Clamp)
            float moveDist = Mathf.Clamp(_speed * Time.deltaTime, 0, dir.magnitude);

            // NavMeshAgent 기반 이동(충돌/회전은 별도로 처리)
            _nma.Move(dir.normalized * moveDist);

            // 디버그: 이동 방향 표시
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, dir.normalized, Color.green);

            // 전방(수평) 장애물 체크: Block 레이어에 닿으면 이동 중단
            // - Ray 원점은 발밑 충돌을 피하려고 살짝 위로 올림
            // - Ray 방향은 dir(정규화 전), 길이는 1m로 근거리만 확인
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, 1.0f, LayerMask.GetMask("Block")))
            {
                _state = PlayerState.Idle;
                return;
            }

            // 캐릭터가 이동 방향을 바라보도록 회전 보간
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 20 * Time.deltaTime);
        }

        // 이동 중이면 블렌드 값 1로 수렴(달리기 비중 증가)
        wait_run_ratio = Mathf.Lerp(wait_run_ratio, 1f, 10 * Time.deltaTime);

        // Animator 파라미터 갱신(BlendTree/상태 전환에서 사용)
        _anim.SetFloat("wait_run_ratio", wait_run_ratio);
    }

    /// <summary>
    /// 대기 상태 처리.
    /// - wait_run_ratio를 0으로 수렴(대기 비중 증가)
    /// - Animator 파라미터만 갱신(상태는 Start에서 WAIT_RUN 고정 재생)
    /// </summary>
    void UpdateIdle()
    {
        wait_run_ratio = Mathf.Lerp(wait_run_ratio, 0f, 10 * Time.deltaTime);
        _anim.SetFloat("wait_run_ratio", wait_run_ratio);
    }

    /// <summary>
    /// 키보드 입력 처리(Managers.Input.KeyAction에서 호출).
    /// - I: 인벤 UI 토글 + 클릭 사운드
    /// </summary>
    void OnKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // 없으면 찾고, 그래도 없으면 생성(안전장치)
            if (_inven == null)
                _inven = FindFirstObjectByType<UI_Inven>(FindObjectsInactive.Include);
            if (_inven == null)
                _inven = Managers.UI.ShowSceneUI<UI_Inven>();

            Managers.Sound.Play("SFX/UI/Click", Define.Sound.Effect);
            _inven.gameObject.SetActive(!_inven.gameObject.activeSelf);
        }
    }

    /// <summary>
    /// 마우스 클릭 처리(Managers.Input.MouseAction에서 호출).
    /// - Wall 레이어 클릭 지점을 목적지로 설정 후 Moving 전환
    /// </summary>
    void OnMouseClicked(Define.MouseEvent evt)
    {
        // 사망 상태면 클릭 무시
        if (_state == PlayerState.Die)
            return;

        // 마우스 화면 좌표를 월드 Ray로 변환
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        // Wall 레이어만 검사하여 이동 가능한 지점만 목적지로 설정
        if (Physics.Raycast(ray, out hit, 100.0f, LayerMask.GetMask("Wall")))
        {
            _destPos = hit.point;
            _state = PlayerState.Moving;
        }
    }
}