using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager
{
    public Action KeyAction = null;                          // 키보드 입력 이벤트(외부에서 구독)
    public Action<Define.MouseEvent> MouseAction = null;     // 마우스 입력 이벤트(외부에서 구독)

    bool _pressed = false;          // 마우스 버튼이 눌린 상태인지(드래그/업 구분)
    float _pressedTime = 0;         // 버튼 눌린 시각(클릭/드래그 판정용)

    /// <summary>
    /// 입력 업데이트(매 프레임 호출).
    /// - UI 위 포인터면 입력 무시(EventSystem 기반)
    /// - 키 입력이 있으면 KeyAction 호출
    /// - 마우스 상태에 따라 MouseEvent(PointerDown/Press/Click/PointerUp) 호출
    /// </summary>
    public void OnUpdate()
    {
        // UI 위에 마우스가 올라가 있으면 게임 입력을 막음(버튼/슬라이더 클릭 등)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 어떤 키든 눌려있으면(또는 누르는 중이면) KeyAction 실행
        if (Input.anyKey && KeyAction != null)
            KeyAction.Invoke();

        // MouseAction을 구독한 곳이 있을 때만 마우스 이벤트 처리
        if (MouseAction != null)
        {
            // 마우스 좌클릭 누르는 중
            if (Input.GetMouseButton(0))
            {
                // 이번 프레임에 "처음 눌림"이면 PointerDown 1회만 보내기
                if (!_pressed)
                {
                    MouseAction.Invoke(Define.MouseEvent.PointerDown);
                    _pressedTime = Time.time; // 눌린 시각 기록
                }

                // 누르고 있는 동안은 매 프레임 Press 보내기(드래그/홀드 처리용)
                MouseAction.Invoke(Define.MouseEvent.Press);
                _pressed = true;
            }
            else
            {
                // 버튼을 뗀 프레임(이전에 눌려있었다면)
                if (_pressed)
                {
                    // 눌렀다 빠르게 떼면 Click으로 간주(클릭/드래그 구분)
                    if (Time.time < _pressedTime +0.2f)
                    {
                        MouseAction.Invoke(Define.MouseEvent.Click);
                    }

                    // 떼는 순간 PointerUp 1회 호출
                    MouseAction.Invoke(Define.MouseEvent.PointerUp);
                }

                // 상태 초기화
                _pressed = false;
                _pressedTime = 0;
            }
        }
    }

    /// <summary>
    /// 입력 이벤트 초기화.
    /// - 씬 전환/리셋 시 구독 해제용
    /// </summary>
    public void Clear()
    {
        KeyAction = null;
        MouseAction = null;
    }
}