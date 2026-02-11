using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBGMAutoPlayer : MonoBehaviour
{
    // 씬별로 자동 재생을 끄고 싶을 때를 대비한 옵션
    [SerializeField] bool _playOnstart = true;

    /// <summary>
    /// 씬 시작 시 현재 씬을 Define.Scene으로 변환하고, 매핑된 BGM이 있으면 SoundManager로 재생합니다.
    /// </summary>
    private void Start()
    {
        // 이 씬에서는 자동 재생을 하지 않도록 옵션 처리
        if (_playOnstart == false)
            return;

        // 현재 활성 씬의 이름을 가져와 프로젝트 공통 enum(Define.Scene)으로 변환
        string activeSceneName = SceneManager.GetActiveScene().name;
        Define.Scene sceneType = ConvertScene(activeSceneName);

        // 씬에 매핑된 BGM이 있으면 재생(실제 재생은 SoundManager가 담당)
        if (BgmDatabase.TryGetBGM(sceneType, out string bgmPath))
            Managers.Sound.Play(bgmPath, Define.Sound.Bgm);
    }

    /// <summary>
    /// Unity 씬 이름(string)을 프로젝트 공통 씬 타입(Define.Scene)으로 변환합니다.
    /// </summary>
    Define.Scene ConvertScene(string unitySceneName)
    {
        // 씬 이름 규칙이 바뀌면 여기만 수정하면 전체 로직이 유지됨
        if (unitySceneName == "Login") return Define.Scene.Login;
        if (unitySceneName == "Game") return Define.Scene.Game;

        // 등록되지 않은 씬의 기본 처리(현재는 Game으로 처리)
        return Define.Scene.Game;
    }
}