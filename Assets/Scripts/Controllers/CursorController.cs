using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    int _mask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Monster);

    Texture2D _attackIcon;
    Texture2D _HandIcon;

    enum CursorType
    {
        None,
        Attack,
        Hand,
    }
    CursorType _cursorType = CursorType.None;
    void Start()
    { 
        _attackIcon = Managers.Resource.Load<Texture2D>("Textures/Cursor/Attack");
        _HandIcon = Managers.Resource.Load<Texture2D>("Textures/Cursor/Hand");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
            return;
        // 마우스 화면 좌표를 월드 Ray로 변환
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        // Wall 레이어만 검사하여 이동 가능한 지점만 목적지로 설정
        if (Physics.Raycast(ray, out hit, 100.0f, _mask))
        {
            if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
            {
                if (_cursorType != CursorType.Attack)
                {
                    Cursor.SetCursor(_attackIcon, new Vector2(_attackIcon.width / 5, 0), CursorMode.Auto);
                    _cursorType = CursorType.Attack;
                }
            }
            else
            {
                if (_cursorType != CursorType.Hand)
                {
                    Cursor.SetCursor(_HandIcon, new Vector2(_HandIcon.width / 3, 0), CursorMode.Auto);
                    _cursorType = CursorType.Hand;
                }
            }
        }
    }
}
