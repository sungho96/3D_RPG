using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    // Raycast 대상 레이어(바닥 + 몬스터)
    int _mask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Monster);

    Texture2D _attackIcon; // 몬스터 위: 공격 커서
    Texture2D _HandIcon;   // 바닥 위: 이동/상호작용 커서

    enum CursorType
    {
        None,
        Attack,
        Hand,
    }

    CursorType _cursorType = CursorType.None; // 불필요한 SetCursor 호출 방지용 캐싱

    /// <summary>
    /// 커서 텍스처 로드.
    /// - Resources/Textures/Cursor/ 경로의 커서 이미지 로드
    /// - (프로젝트 규칙에 따라 Managers.Resource 사용)
    /// </summary>
    void Start()
    {
        _attackIcon = Managers.Resource.Load<Texture2D>("Textures/Cursor/Attack");
        _HandIcon = Managers.Resource.Load<Texture2D>("Textures/Cursor/Hand");
    }

    /// <summary>
    /// 커서 타입 갱신.
    /// - 마우스 클릭(드래그) 중에는 커서 변경을 잠시 중단
    /// - Raycast로 마우스 아래 오브젝트 레이어를 확인해 커서 변경
    /// - 동일 커서면 SetCursor 재호출을 하지 않도록 _cursorType으로 상태 캐싱
    /// </summary>
    void Update()
    {
        // 클릭/드래그 중에는 커서 변경하지 않음(깜빡임/의도치 변경 방지)
        if (Input.GetMouseButton(0))
            return;

        // 마우스 화면 좌표 → 월드 Ray
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        // Ground/Monster 레이어만 검사
        if (Physics.Raycast(ray, out hit, 100.0f, _mask))
        {
            // 몬스터 위면 공격 커서
            if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
            {
                // 이미 공격 커서면 다시 SetCursor 하지 않음
                if (_cursorType != CursorType.Attack)
                {
                    // 커서 핫스팟(클릭 기준점): 아이콘 이미지 내 기준점 조절
                    Cursor.SetCursor(_attackIcon, new Vector2(_attackIcon.width / 5, 0), CursorMode.Auto);
                    _cursorType = CursorType.Attack;
                }
            }
            else
            {
                // 바닥(또는 기타)면 손 커서
                if (_cursorType != CursorType.Hand)
                {
                    Cursor.SetCursor(_HandIcon, new Vector2(_HandIcon.width / 3, 0), CursorMode.Auto);
                    _cursorType = CursorType.Hand;
                }
            }
        }
    }
}