using UnityEngine;

public class PlayerStats : Stat
{
    [SerializeField] string _playerName = "Kata(name)";

    [SerializeField] int _mp = 50;
    [SerializeField] int _maxMp = 50;

    [SerializeField] int _exp = 0;
    [SerializeField] int _needExp = 100;

    [SerializeField] int _gold = 0;

    public string PlayerName
    {
        get => _playerName;
        set { _playerName = value; NotifyChanged(); }
    }

    public int Mp
    {
        get => _mp;
        set { _mp = Mathf.Clamp(value, 0, _maxMp); NotifyChanged(); }
    }

    public int MaxMp
    {
        get => _maxMp;
        set
        {
            _maxMp = Mathf.Max(1, value);
            _mp = Mathf.Clamp(_mp, 0, _maxMp);
            NotifyChanged();
        }
    }

    public float Mp01 => (_maxMp <= 0) ? 0f : Mathf.Clamp01((float)_mp / _maxMp);

    public int Exp
    {
        get => _exp;
        set { _exp = Mathf.Max(0, value); NotifyChanged(); }
    }

    public int NeedExp
    {
        get => _needExp;
        set { _needExp = Mathf.Max(1, value); NotifyChanged(); }
    }

    public float Exp01 => (_needExp <= 0) ? 0f : Mathf.Clamp01((float)_exp / _needExp);

    public int Gold
    {
        get => _gold;
        set { _gold = Mathf.Max(0, value); NotifyChanged(); }
    }

    public string CurrentHpText => $"{Hp} / {MaxHp}";
    public string CurrentMpText => $"{Mp} / {MaxMp}";
}