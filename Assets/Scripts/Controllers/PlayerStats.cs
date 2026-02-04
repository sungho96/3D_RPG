using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // 스탯이 바뀌면 UI가 갱신하도록 알림
    public event Action OnChanged;

    // 아래 값들은 예시(이미 갖고 계신 멤버가 있으면 거기에 맞춰서 호출만 추가)
    public string PlayerName = "Kata(name)";
    public int Level = 1;

    public float MaxHp = 100f;
    public float Hp { get; private set; } = 100f;

    public float MaxMp = 50f;
    public float Mp { get; private set; } = 50f;

    public float Exp01 { get; private set; } = 0f;

    // HUD에서 쓰는 텍스트(이미 있으면 그대로 사용)
    public string CurrentHpText => $"{Mathf.RoundToInt(Hp)} / {Mathf.RoundToInt(MaxHp)}";
    public string CurrentMpText => $"{Mathf.RoundToInt(Mp)} / {Mathf.RoundToInt(MaxMp)}";

    private void Awake()
    {
        // 초기값도 범위 보정 (혹시 Inspector 값이 이상해도 안전)
        Hp = Mathf.Clamp(Hp, 0f, MaxHp);
        Mp = Mathf.Clamp(Mp, 0f, MaxMp);
        Exp01 = Mathf.Clamp01(Exp01);
        Level = Mathf.Max(1, Level);

        NotifyChanged(); 
    }

    // 값 변경이 끝났을 때 호출(한 곳에서만 호출하면 관리 쉬움)
    void NotifyChanged()
    {
        OnChanged?.Invoke();
    }

    // 예: 데미지/회복/MP소모 같은 함수들에서 마지막에 NotifyChanged() 호출
    public void SetHp(float newHp)
    {
        Hp = Mathf.Clamp(newHp, 0f, MaxHp);
        NotifyChanged();
    }

    public void SetMp(float newMp)
    {
        Mp = Mathf.Clamp(newMp, 0f, MaxMp);
        NotifyChanged();
    }

    public void SetExp01(float exp01)
    {
        Exp01 = Mathf.Clamp01(exp01);
        NotifyChanged();
    }

    public void SetName(string name)
    {
        PlayerName = name;
        NotifyChanged();
    }

    public void SetLevel(int level)
    {
        Level = level;
        NotifyChanged();
    }
}
