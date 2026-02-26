using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    // Bgm/Effect 채널 AudioSource 보관(Define.Sound enum 인덱스 기준)
    AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];

    // SFX는 반복 호출이 많아 클립 캐싱(경로 -> AudioClip)
    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    /// <summary>
    /// @Sound 루트와 Bgm/Effect 채널용 AudioSource를 1회 생성하고(DontDestroyOnLoad), BGM 루프 옵션을 설정합니다.
    /// </summary>
    public void Init()
    {
        // 이미 생성되어 있으면 재생성하지 않음(중복 방지)
        GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            // 씬 전환에도 유지되는 사운드 루트
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

            // Define.Sound enum 이름대로 자식 오브젝트 + AudioSource 생성
            // (MaxCount는 배열 크기용이므로 제외)
            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            for (int i = 0; i < soundNames.Length - 1; i++)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>(); // 채널별 AudioSource 확보
                go.transform.parent = root.transform;              // @Sound 아래에 정리
            }

            // BGM은 반복 재생이 기본
            _audioSources[(int)Define.Sound.Bgm].loop = true;
        }
    }

    /// <summary>
    /// 모든 채널 재생을 정지하고 clip을 해제하며, SFX 캐시(Dictionary)를 초기화합니다.
    /// </summary>
    public void Clear()
    {
        // 채널 초기화: 재생 중지 + 참조 해제
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.clip = null; // BGM 교체/정리 시 잔여 참조 방지
            audioSource.Stop();      // 즉시 정지
        }

        // SFX 캐시 비우기(다음 재생 시 다시 로드)
        _audioClips.Clear();
    }

    /// <summary>
    /// 경로로 AudioClip을 로드/캐싱한 뒤, 지정한 채널(Bgm/Effect)로 재생을 요청합니다.
    /// </summary>
    public void Play(string path, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
    {
        // 경로 기반으로 클립 확보(BGM: 즉시 로드 / SFX: 캐싱 우선)
        AudioClip audioClip = GetOrAddAudioClip(path, type);

        // 실제 재생 로직은 AudioClip 버전으로 통일
        Play(audioClip, type, pitch);
    }

    /// <summary>
    /// AudioClip을 채널에 맞게 재생합니다. (BGM: clip 교체 후 Play / SFX: PlayOneShot)
    /// </summary>
    public void Play(AudioClip audioClip, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
    {
        // 로드 실패(null)이면 안전하게 종료
        if (audioClip == null)
            return;

        if (type == Define.Sound.Bgm)
        {
            AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm]; // BGM 채널

            // BGM은 단일 트랙 정책: 재생 중이면 교체를 위해 Stop
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.pitch = pitch;  // 연출용 피치(기본 1.0)
            audioSource.clip = audioClip; // BGM 교체
            audioSource.Play();         // 재생 시작
        }
        else
        {
            AudioSource audioSource = _audioSources[(int)Define.Sound.Effect]; // SFX 채널

            audioSource.pitch = pitch;         // SFX 피치 변주
            audioSource.PlayOneShot(audioClip); // 겹침 재생 허용(타격/버튼 연타 등)
        }
    }

    /// <summary>
    /// Resources 경로 규칙에 맞춰 AudioClip을 로드합니다. (SFX는 Dictionary로 캐싱, BGM은 즉시 로드)
    /// </summary>
    AudioClip GetOrAddAudioClip(string path, Define.Sound type = Define.Sound.Effect)
    {
        // "Sounds/" 접두어가 없으면 자동 보정
        if (path.Contains("Sounds/") == false)
            path = $"Sounds/{path}";

        AudioClip audioClip = null;

        if (type == Define.Sound.Bgm)
        {
            // BGM: 캐싱 없이 바로 로드(정책)
            audioClip = Managers.Resource.Load<AudioClip>(path);
            if (audioClip == null)
                Debug.Log($"AudioClip Missing ! {path}");
        }
        else
        {
            // SFX: 캐시 우선 → 없으면 로드 후 캐시에 추가
            if (_audioClips.TryGetValue(path, out audioClip) == false)
            {
                audioClip = Managers.Resource.Load<AudioClip>(path);
                _audioClips.Add(path, audioClip);
            }

            if (audioClip == null)
                Debug.Log($"AudioClip Missing ! {path}");
        }

        return audioClip;
    }
}