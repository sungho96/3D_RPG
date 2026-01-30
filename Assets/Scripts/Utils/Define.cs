using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define //프로젝트 공통 규칙(상태/타입)을 한 곳에 모아두는 설계
{
    public enum MouseEvent
    { 
        Press,
        Click,
    }

    public enum CameraMode
    {
        QuaterView,
    }
}
