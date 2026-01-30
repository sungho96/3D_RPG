using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]   // 현재 카메라 동작 모드 (기본: 쿼터뷰)
    Define.CameraMode _mode = Define.CameraMode.QuaterView;
    
    [SerializeField]
    // 플레이어 기준 카메라 오프셋(상대 위치) : (x=좌우, y=높이, z=뒤쪽)
    // 예: y=6, z=-5  → 45도 위에서 내려보는 느낌
    public Vector3 _delta = new Vector3(0.0f, 6.0f, -5.0f);

    [SerializeField] //대상자
    public GameObject _player;

    // 플레이어의 머리/상체 쪽을 바라보게 하기 위한 시선 높이 보정값
    [SerializeField]
    private float YOffset= 0.6f;

    void Start()
    {
        
    }

    void LateUpdate()//먼저 플레이어 이동을 받고 나서 Pos처리
    {
        // 벽에 카메라가 파고들지 않도록, 플레이어→카메라 방향으로 Raycast를 쏴서
        // 장애물이 있으면 카메라 거리를 줄이는 방식(간단한 충돌 회피)
        if (_mode == Define.CameraMode.QuaterView)
        {
            // 플레이어 → 카메라 방향에 Wall이 있으면 카메라를 당겨서 클리핑 방지
            RaycastHit hit;
            if (Physics.Raycast(_player.transform.position, _delta, out hit, _delta.magnitude, LayerMask.GetMask("Wall")))
            {
                float dist = (hit.point - _player.transform.position).magnitude * 0.8f; // 안전 마진 포함 거리
                transform.position = _player.transform.position+transform.up*YOffset + _delta.normalized * dist;

            }
            else
            {
                transform.position = _player.transform.position + _delta;//플레이어를 따라다니기(palyer.pos+ 고정값)
                transform.LookAt(_player.transform);// 플레이어를 바라보게 회전
            }
        }
    }

    /// <summary>
    /// 쿼터뷰 모드/옵셋 설정  
    /// </summary>
    /// <param name="delta"></param>
    public void SetQuaterview(Vector3 delta)
    {
        _mode = Define.CameraMode.QuaterView;
        _delta = delta; 
    }
}
