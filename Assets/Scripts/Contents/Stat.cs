using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Stat : MonoBehaviour
{
    public event Action OnChanged;

    [SerializeField] protected int _level = 1;
    [SerializeField] protected int _hp = 100;
    [SerializeField] protected int _maxHp = 100;
    [SerializeField] protected int _attack = 10;
    [SerializeField] protected int _defense = 5;
    [SerializeField] protected float _moveSpeed = 5f;
    protected void NotifyChanged() => OnChanged?.Invoke();
    public int Level
    {
        get => _level;
        set { _level = Mathf.Max(1, value); NotifyChanged(); }
    }
    public int MaxHp
    {
        get => _maxHp;
        set
        {
            _maxHp = Mathf.Max(1, value);
            _hp = Mathf.Clamp(_hp, 0, _maxHp); // MaxHp 변경 시 Hp도 보정
            NotifyChanged();
        }
    }
    public int Hp
    {
        get => _hp;
        set { _hp = Mathf.Clamp(value, 0, _maxHp); 
        NotifyChanged(); }
    }
    public int Attack
    {
        get => _attack;
        set { _attack = Mathf.Max(0, value); 
        NotifyChanged(); }
    }
    public int Defense
    {
        get => _defense;
        set { _defense = Mathf.Max(0, value); 
        NotifyChanged(); }
    }
    public float MoveSpeed
    {
        get => _moveSpeed;
        set { _moveSpeed = Mathf.Max(0f, value); 
        NotifyChanged(); }
    }
    public float Hp01 => (_maxHp <= 0) ? 0f : Mathf.Clamp01((float)_hp / _maxHp);

    public virtual void OnAttacked(Stat attacker)
    {
        // 방어력을 고려한 최종 데미지 계산(0 미만 방지)
        int damage = Mathf.Max(0, attacker.Attack - Defense);
        Hp -= damage;
        if (Hp <= 0)
        {
            Hp = 0;
            OnDead(attacker);
        }

    }
    public void ResetHpToMax()
    {
        Hp = MaxHp; // OnChanged 자동 호출됨
    }
    protected  virtual void OnDead(Stat attacker)
    {
        PlayerStats playerStat = attacker as PlayerStats;
        if (playerStat != null)
        {
            playerStat.Exp += 3;
        }

        Managers.Game.Despawn(gameObject);
    }


}
