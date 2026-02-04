using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_HUD : UI_Scene
{
    // UI_Base Bind용 enum (Hierarchy 오브젝트 이름과 동일해야 함)
    enum Images
    {
        HPLine, // HP 게이지(fillAmount)
        MPLine, // MP 게이지(fillAmount)
        XPLine, // EXP 게이지(fillAmount)
    }

    enum Texts
    {
        PlayerNameText, // 플레이어 이름 표시
        LevelText,      // 레벨 표시
        CurrentHpText,  // "현재HP / 최대HP" 같은 문자열
        CurrentMpText,  // "현재MP / 최대MP" 같은 문자열
    }

    // 게이지 Image 캐시(매번 GetImage() 호출 안 하려고 저장)
    Image _hpLine;
    Image _mpLine;
    Image _xpLine;

    // HUD 텍스트 캐시
    TMP_Text _nameText;
    TMP_Text _levelText;
    TMP_Text _currentHpText;
    TMP_Text _currentMpText;

    // 스탯 데이터(실제 값은 여기서 가져옴)
    PlayerStats _stats;

    private void Start()
    {
        // UI 생성 후 초기 바인딩/구독 세팅
        Init();
    }

    public override void Init()
    {
        base.Init();

        //Hierarchy 이름 기준으로 UI 요소들을 찾아서 캐싱(UI_Base의 Bind 시스템)
        Bind<Image>(typeof(Images));
        Bind<TMP_Text>(typeof(Texts));

        //자주 쓰는 UI 참조는 멤버로 캐싱(성능 + 코드 가독성)
        _hpLine = GetImage((int)Images.HPLine);
        _mpLine = GetImage((int)Images.MPLine);
        _xpLine = GetImage((int)Images.XPLine);

        _nameText = GetText((int)Texts.PlayerNameText);
        _levelText = GetText((int)Texts.LevelText);
        _currentHpText = GetText((int)Texts.CurrentHpText);
        _currentMpText = GetText((int)Texts.CurrentMpText);

        //씬에서 PlayerStats 찾기
        //    (HUD가 스탯을 직접 들고있지 않으니 참조를 찾아와야 함)
        if (_stats == null)
            _stats = FindFirstObjectByType<PlayerStats>();

        //이벤트 기반 갱신:
        //    매 프레임 LateUpdate로 갱신하지 않고, 스탯이 바뀌는 순간에만 RefreshHUD가 호출되게 함
        if (_stats != null)
        {
            _stats.OnChanged -= RefreshHUD;
            _stats.OnChanged += RefreshHUD;
        }

            //시작 시 1회 갱신(초기 HUD 비어있는 문제 방지)
            RefreshHUD();
    }

    // 오브젝트가 파괴될 때 이벤트 구독 해제
    // - 씬 전환/재생성 시 중복 구독으로 RefreshHUD가 여러 번 호출되는 문제 방지
    // - 파괴된 UI가 이벤트를 계속 받는 상황 방지
    private void OnDestroy()
    {
        if (_stats != null)
            _stats.OnChanged -= RefreshHUD;
    }

    // PlayerStats 값으로 HUD를 갱신하는 함수(텍스트 + 게이지)
    // - OnChanged 이벤트가 발생할 때만 호출되는 것이 목표
    void RefreshHUD()
    {
        if (_stats == null)
            return;

        // 텍스트 갱신 (null 체크는 UI 누락/이름 불일치 상황에서 크래시 방지)
        if (_nameText != null) _nameText.text = _stats.PlayerName;
        if (_levelText != null) _levelText.text = _stats.Level.ToString();
        if (_currentHpText != null) _currentHpText.text = _stats.CurrentHpText;
        if (_currentMpText != null) _currentMpText.text = _stats.CurrentMpText;

        // 게이지 갱신(fillAmount는 0~1)
        // - SafeDiv01로 max가 0인 상황에서도 안전하게 처리
        if (_hpLine != null) _hpLine.fillAmount = SafeDiv01(_stats.Hp, _stats.MaxHp);
        if (_mpLine != null) _mpLine.fillAmount = SafeDiv01(_stats.Mp, _stats.MaxMp);

        // EXP는 이미 0~1로 관리되는 값이라 Clamp01만
        if (_xpLine != null) _xpLine.fillAmount = Mathf.Clamp01(_stats.Exp01);
    }

    // value/max를 0~1로 환산하는 보조 함수
    // - max가 0이면 나눗셈 오류/NaN 방지
    float SafeDiv01(float value, float max)
    {
        if (max <= 0.0001f) return 0f;
        return Mathf.Clamp01(value / max);
    }
}
