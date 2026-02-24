using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define //프로젝트 공통 규칙(상태/타입)을 한 곳에 모아두는 설계
{
    public enum State
    {
        Die,
        Moving,
        Idle,
        Skill, // 기본공격/스킬 상태 공용(애니 이벤트로 루프/종료 제어)
    }
    public enum Layer
    {
        Monster = 9,
        Ground = 8,
        Block = 10,
    }

    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Game,
    }
    
    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }
    public enum UIEvent
    {
        Click,
        Drag,
        Enter,
        Exit,
    }

    public enum MouseEvent
    { 
        Press,
        Click,
        PointerDown,
        PointerUp,
    }

    public enum CameraMode
    {
        QuaterView,
    }
}
