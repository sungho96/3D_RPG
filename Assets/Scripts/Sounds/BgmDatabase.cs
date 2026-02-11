using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgmDatabase : MonoBehaviour
{
    // Define.Scene 기준으로 BGM 경로를 고정(한 곳에서만 관리)
    static readonly Dictionary<Define.Scene, string> _map = new Dictionary<Define.Scene, string>()
    {
        // SoundManager가 내부에서 "Sounds/"를 붙이므로, 여기엔 하위 경로만 적는 규칙
        { Define.Scene.Login, "BGM/Login" },
        { Define.Scene.Game,  "BGM/Game"  },
    };

    /// <summary>
    /// 씬 타입(Define.Scene)에 해당하는 BGM 경로가 있으면 반환합니다. (성공: true + out)
    /// </summary>
    public static bool TryGetBGM(Define.Scene scene, out string bgmPath)
    {
        // Dictionary에서 씬 키로 경로를 찾아 반환
        return _map.TryGetValue(scene, out bgmPath);
    }
}