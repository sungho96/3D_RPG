using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : Stat
{
   
    [SerializeField] string _playerName = "Kata(name)";

    [SerializeField] int _mp = 50;
    [SerializeField] int _maxMp = 50;

    [SerializeField] int _exp = 0;
    
    Data.Stat stat;
    public int Exp
    {
        get { return _exp; }
        set
        {
            _exp = value;

            int level = Level;

            while (Managers.Data.StatDict.TryGetValue(level + 1, out var next))
            {
                if (_exp < next.totalExp) break;
                level++;
            }

            if (level != Level)
            {
                Level = level;
                SetStat(Level);
            }
            NotifyChanged();
        }
    }

    [SerializeField] int _gold = 0;

    [SerializeField] float _skill1 = 40f;
    [SerializeField] float _skill2 = 30f;
    [SerializeField] float _skill3 = 20f;
    [SerializeField] float _skill4 = 10f;

    public float Skill1 => _skill1;
    public float Skill2 => _skill2;
    public float Skill3 => _skill3;
    public float Skill4 => _skill4;

    public void Start()
    {
        _level = 1;

        Dictionary<int, Data.Stat> dict = Managers.Data.StatDict;
        _defense = 5;
        _moveSpeed = 5;
        _exp = 0;
        _gold = 0;

        SetStat(_level);

    }

    public void SetStat(int level)
    {
        var dict = Managers.Data.StatDict;

        // 필드 stat에 저장!
        this.stat = dict[level];

        _hp = this.stat.maxHp;
        _maxHp = this.stat.maxHp;
        _attack = this.stat.attack;
        _skill1 = this.stat.skill1;
        _skill2 = this.stat.skill2;
        _skill3 = this.stat.skill3;
        _skill4 = this.stat.skill4;
    }

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

    public float Exp01
    {
        get
        {
            var dict = Managers.Data.StatDict;
            if (dict == null) return 0f;

            int curReq = 0;
            if (dict.TryGetValue(Level, out var cur))
                curReq = cur.totalExp;

            // 만렙이면 게이지는 꽉 찬 상태(또는 0으로 처리하고 싶으면 0f로)
            if (dict.TryGetValue(Level + 1, out var next) == false)
                return 1f;

            int nextReq = next.totalExp;

            int span = Mathf.Max(1, nextReq - curReq);
            return Mathf.Clamp01((float)(_exp - curReq) / span);
        }
    }

    public int Gold
    {
        get => _gold;
        set { _gold = Mathf.Max(0, value); NotifyChanged(); }
    }

    public string CurrentHpText => $"{Hp} / {MaxHp}";
    public string CurrentMpText => $"{Mp} / {MaxMp}";
    protected override void OnDead(Stat attacker)
    {
        Managers.Game.Despawn(gameObject);
    }
}