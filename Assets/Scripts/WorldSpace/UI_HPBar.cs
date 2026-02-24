using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HPBar : UI_Base
{
    // UI_Base 바인딩용: Hierarchy 오브젝트 이름과 동일해야 함
    enum GameObjects
    {
        HPBar, // Slider(체력 게이지)
    }

    Stat _stat; // 부모(몬스터/플레이어)의 Stat 참조

    /// <summary>
    /// UI 초기화(바인딩 + 대상 스탯 캐싱).
    /// - HPBar(Slider) 오브젝트 바인딩
    /// - 월드 UI의 부모(캐릭터)에서 Stat을 가져와 참조
    /// </summary>
    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));

        // 월드 스페이스 UI는 보통 캐릭터 하위로 붙기 때문에 parent에서 Stat 탐색
        _stat = transform.parent.GetComponent<Stat>();
    }

    /// <summary>
    /// 매 프레임 월드 UI 위치/회전/게이지 갱신.
    /// - 부모 콜라이더 높이만큼 위로 올려 머리 위에 붙임
    /// - 카메라를 바라보도록 회전(빌보드)
    /// - Hp/MaxHp 비율로 Slider 값 업데이트
    /// </summary>
    private void Update()
    {
        Transform parent = transform.parent;

        // 부모의 콜라이더 높이만큼 위로 올려서 "머리 위" 위치에 배치
        // (캐릭터마다 키가 달라도 콜라이더 기준으로 자동 보정)
        transform.position = parent.position + Vector3.up * (parent.GetComponent<Collider>().bounds.size.y);

        // 카메라를 항상 바라보도록 회전(월드 UI 가독성)
        transform.rotation = Camera.main.transform.rotation;

        // 체력 비율 계산 -> 슬라이더에 반영
        float ratio = _stat.Hp / (float)_stat.MaxHp;
        SetHPRatio(ratio);
    }

    /// <summary>
    /// HP 게이지 비율 적용(0~1).
    /// - Slider.value에 ratio를 넣어 fill을 갱신
    /// </summary>
    public void SetHPRatio(float ratio)
    {
        GetObject((int)GameObjects.HPBar).GetComponent<Slider>().value = ratio;
    }
}