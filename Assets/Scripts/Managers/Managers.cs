using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    // 전역 단일 인스턴스(싱글톤)
    static Managers s_instance;

    // 외부에서 Managers 접근 시, 없으면 자동 초기화 후 반환
    static Managers Instance { get { Init(); return s_instance; } }

    // ===== Contents =====
    GameManager _gameManager = new GameManager(); // 게임 진행/상태 관련 매니저(컨텐츠 영역)
    public static GameManager Game { get { return Instance._gameManager; } }

    // ===== Core Managers =====
    DataManager _data = new DataManager();             // JSON/데이터 로딩
    InputManager _input = new InputManager();          // 키/마우스 입력 이벤트 처리
    PoolManager _pool = new PoolManager();             // 오브젝트 풀링
    ResourceManager _resource = new ResourceManager(); // Resources 로드/생성/삭제
    SceneManagerEx _scene = new SceneManagerEx();      // 씬 전환/현재 씬 관리
    SoundManager _sound = new SoundManager();          // BGM/SFX 재생 관리
    UIManager _ui = new UIManager();                   // UI 생성/정리/캔버스 설정

    // 전역 접근용 프로퍼티(Managers.X 형태로 사용)
    public static DataManager Data { get { return Instance._data; } }
    public static InputManager Input { get { return Instance._input; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static UIManager UI { get { return Instance._ui; } }

    /// <summary>
    /// Unity Start 진입점.
    /// - 씬에 Managers가 직접 배치된 경우에도 초기화 보장
    /// - 이미 생성된 인스턴스가 있으면 Init 내부에서 중복 초기화 방지
    /// </summary>
    void Start()
    {
        Init();
    }

    /// <summary>
    /// 매 프레임 입력 처리.
    /// - InputManager.OnUpdate()를 통해 KeyAction / MouseAction 이벤트를 발행
    /// </summary>
    void Update()
    {
        _input.OnUpdate();
    }

    /// <summary>
    /// Managers 싱글톤 초기화.
    /// - @Managers 오브젝트를 찾거나 새로 생성
    /// - DontDestroyOnLoad 적용
    /// - 코어 매니저(Data/Pool/Sound) 초기화
    /// </summary>
    static void Init()
    {
        // 이미 초기화되어 있으면 재진입 방지
        if (s_instance == null)
        {
            // 씬에 배치된 @Managers가 있으면 재사용
            GameObject go = GameObject.Find("@Managers");

            // 없으면 런타임에 생성
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            // 씬 전환 시 파괴되지 않도록 유지
            DontDestroyOnLoad(go);

            // 실제 Managers 컴포넌트 참조 캐싱
            s_instance = go.GetComponent<Managers>();

            // 코어 매니저 초기화(1회)
            s_instance._data.Init();
            s_instance._pool.Init();
            s_instance._sound.Init();
        }
    }

    /// <summary>
    /// 씬 전환/리셋 시 매니저 내부 상태 정리.
    /// - 입력 구독 해제, 사운드 정리, UI 정리, 풀 정리 등
    /// - Managers 오브젝트 자체는 유지(DontDestroyOnLoad)
    /// </summary>
    public static void Clear()
    {
        Input.Clear();
        Sound.Clear();
        Scene.Clear();
        UI.Clear();
        Pool.Clear();
    }
}